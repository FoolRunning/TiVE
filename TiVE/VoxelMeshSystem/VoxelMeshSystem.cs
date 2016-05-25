using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVE.RenderSystem.Lighting;
using ProdigalSoftware.TiVE.RenderSystem.World;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.VoxelMeshSystem
{
    /// <summary>
    /// User setting for the voxel detail distance
    /// </summary>
    internal enum VoxelDetailLevelDistance
    {
        Closest = 0,
        Close = 1,
        Mid = 2,
        Far = 3,
        Furthest = 4
    }

    internal sealed class VoxelMeshSystem : EngineSystem
    {
        #region Constants
        public const byte VoxelDetailLevelSections = 4; // 32x32x32 = 32768v, 16x16x16 = 4096v, 8x8x8 = 512v, 4x4x4 = 64v, not worth going to 2x2x2 = 8v.
        private const byte BestVoxelDetailLevel = 0;
        private const byte WorstVoxelDetailLevel = VoxelDetailLevelSections - 1;
        private const int TotalChunkMeshBuilders = 20;
        private const int TotalSpriteMeshBuilders = 3;
        private const int MaxQueueSize = 5000;
        #endregion

        #region Member variables
        private readonly HashSet<IEntity> loadedEntities = new HashSet<IEntity>();

        private readonly List<Thread> meshCreationThreads = new List<Thread>();
        private readonly EntityLoadQueue entityLoadQueue = new EntityLoadQueue(MaxQueueSize);
        private readonly List<MeshBuilder> chunkMeshBuilders = new List<MeshBuilder>(TotalChunkMeshBuilders);
        private readonly List<MeshBuilder> spriteMeshBuilders = new List<MeshBuilder>(TotalSpriteMeshBuilders);

        private Scene loadedScene;

        private volatile bool endCreationThreads;
        #endregion

        #region Constructor/Dispose
        public VoxelMeshSystem() : base("VoxelMeshes")
        {
            TiVEController.UserSettings.SettingChanged += UserSettings_SettingChanged;
        }
        #endregion

        #region Implementation of EngineSystem
        public override void Dispose()
        {
            Debug.Assert(Thread.CurrentThread.Name == "Main UI");

            TiVEController.UserSettings.SettingChanged -= UserSettings_SettingChanged;

            endCreationThreads = true;
            foreach (Thread thread in meshCreationThreads)
                thread.Join();
            meshCreationThreads.Clear();

            using (new PerformanceLock(entityLoadQueue))
                entityLoadQueue.Clear();

            loadedEntities.Clear();
        }

        public override bool Initialize()
        {
            for (int i = 0; i < TotalChunkMeshBuilders; i++)
                chunkMeshBuilders.Add(new MeshBuilder(1000000, 4000000));

            for (int i = 0; i < TotalSpriteMeshBuilders; i++)
                spriteMeshBuilders.Add(new MeshBuilder(6000000, 12000000)); // Sprites can be max of 255x255x255 and contain a lot of voxels

            int maxThreads = TiVEController.UserSettings.Get(UserSettings.ChunkCreationThreadsKey);
            for (int i = 0; i < maxThreads; i++)
                meshCreationThreads.Add(StartMeshCreateThread(i + 1));
            return true;
        }

        /// <summary>
        /// Performs the necessary cleanup when the current scene has changed
        /// </summary>
        public override void ChangeScene(Scene oldScene, Scene newScene)
        {
            // Clean up threads and data for previous scene
            endCreationThreads = true;
            foreach (Thread thread in meshCreationThreads)
                thread.Join();
            
            using (new PerformanceLock(entityLoadQueue))
                entityLoadQueue.Clear();

            foreach (MeshBuilder meshBuilder in chunkMeshBuilders.Concat(spriteMeshBuilders)) // Make sure all mesh builders are available for the new scene
                meshBuilder.DropMesh();

            // Start threads for new scene
            endCreationThreads = false;
            for (int i = 0; i < meshCreationThreads.Count; i++)
                meshCreationThreads[i] = StartMeshCreateThread(i);

            loadedScene = newScene;
            newScene.LoadingInitialChunks = true;
        }

        protected override bool UpdateInternal(int ticksSinceLastUpdate, float timeBlendFactor, Scene currentScene)
        {
            CameraComponent cameraData = currentScene.FindCamera();
            if (cameraData == null)
                return true;

            using (new PerformanceLock(entityLoadQueue))
            {
                foreach (IEntity entity in cameraData.NewlyHiddenEntitites.Where(e => e.HasComponent<VoxelMeshComponent>()))
                {
                    entityLoadQueue.Remove(entity);
                    loadedEntities.Remove(entity);
                }
            }

            VoxelDetailLevelDistance currentVoxelDetalLevelSetting = (VoxelDetailLevelDistance)(int)TiVEController.UserSettings.Get(UserSettings.DetailDistanceKey);

            foreach (IEntity entity in loadedEntities)
            {
                VoxelMeshComponent renderData = entity.GetComponent<VoxelMeshComponent>();
                Debug.Assert(renderData != null);

                if (renderData.MeshData != null)
                {
                    byte perferedDetailLevel = GetPerferedVoxelDetailLevel(renderData, cameraData, currentVoxelDetalLevelSetting);
                    if (renderData.LoadedVoxelDetailLevel != perferedDetailLevel)
                        LoadEntity(entity, renderData, perferedDetailLevel);
                }
            }

            foreach (IEntity entity in cameraData.NewlyVisibleEntitites.Where(e => e.HasComponent<VoxelMeshComponent>()))
            {
                VoxelMeshComponent renderData = entity.GetComponent<VoxelMeshComponent>();
                Debug.Assert(renderData != null);
                LoadEntity(entity, renderData, WorstVoxelDetailLevel); // Initially load at the worst detail level to quickly get something on screen
            }

            if (currentScene.LoadingInitialChunks && entityLoadQueue.Size < 5) // Let chunks load before showing scene
                currentScene.LoadingInitialChunks = false;

            return true;
        }
        #endregion

        #region Public methods
        public void ReloadAllEntities()
        {
            foreach (IEntity chunk in loadedEntities)
                ReloadEntity(chunk, WorstVoxelDetailLevel); // TODO: Reload with the correct detail level
        }
        #endregion

        #region Event handlers
        private void UserSettings_SettingChanged(string settingName)
        {
            if (settingName == UserSettings.LightingTypeKey)
                ReloadAllEntities();
        }
        #endregion

        #region Mesh creation methods
        private Thread StartMeshCreateThread(int num)
        {
            Thread thread = new Thread(MeshCreateLoop);
            thread.Priority = ThreadPriority.BelowNormal;
            thread.IsBackground = true;
            thread.Name = "MeshLoad" + num;
            thread.Start();
            return thread;
        }

        private void MeshCreateLoop()
        {
            while (!endCreationThreads)
            {
                bool hasItemToLoad;
                using (new PerformanceLock(entityLoadQueue))
                    hasItemToLoad = !entityLoadQueue.IsEmpty;

                if (!hasItemToLoad)
                {
                    Thread.Sleep(1);
                    continue;
                }

                MeshBuilder meshBuilder;
                using (new PerformanceLock(chunkMeshBuilders))
                {
                    meshBuilder = chunkMeshBuilders.Find(mb => !mb.IsLocked);
                    if (meshBuilder != null)
                        meshBuilder.StartNewMesh(); // Found a mesh builder - grab it quick!
                }

                if (meshBuilder == null)
                {
                    Thread.Sleep(1);
                    continue; // No free meshbuilders to use
                }

                IEntity entity;
                byte foundEntityDetailLevel;
                using (new PerformanceLock(entityLoadQueue))
                    entity = entityLoadQueue.Dequeue(out foundEntityDetailLevel);

                if (entity == null)
                {
                    // Couldn't find a entity to load. No need to hold onto the mesh builder.
                    meshBuilder.DropMesh();
                    continue;
                }

                VoxelMeshComponent renderData = entity.GetComponent<VoxelMeshComponent>();
                Debug.Assert(renderData != null);

                if (!renderData.Visible)
                {
                    // Entity became invisible while waiting to be loaded. No need to hold onto the mesh builder.
                    meshBuilder.DropMesh();
                    continue;
                }

                ChunkComponent chunkData = renderData as ChunkComponent;
                if (chunkData != null)
                    LoadChunkMesh(chunkData, meshBuilder, foundEntityDetailLevel);
                else
                    LoadMesh(entity, renderData, meshBuilder, foundEntityDetailLevel);
            }
        }
        #endregion

        #region Methods for loading chunk meshes
        private void LoadChunkMesh(ChunkComponent chunkData, MeshBuilder meshBuilder, byte voxelDetailLevel)
        {
            Scene scene = loadedScene; // For thread safety
            chunkData.LoadedVoxelDetailLevel = voxelDetailLevel;

            Debug.Assert(meshBuilder.IsLocked);
            Debug.Assert(loadedScene != null);
            //Debug.WriteLine("Started chunk ({0},{1},{2})", chunkStartX, chunkStartY, chunkStartZ);

            GameWorld gameWorld = scene.GameWorld;

            scene.LightData.CacheLightsInBlocksForChunk(chunkData.ChunkLoc.X, chunkData.ChunkLoc.Y, chunkData.ChunkLoc.Z);

            int blockStartX = chunkData.ChunkBlockLoc.X;
            int blockEndX = Math.Min((chunkData.ChunkLoc.X + 1) * ChunkComponent.BlockSize, gameWorld.BlockSize.X);
            int blockStartY = chunkData.ChunkBlockLoc.Y;
            int blockEndY = Math.Min((chunkData.ChunkLoc.Y + 1) * ChunkComponent.BlockSize, gameWorld.BlockSize.Y);
            int blockStartZ = chunkData.ChunkBlockLoc.Z;
            int blockEndZ = Math.Min((chunkData.ChunkLoc.Z + 1) * ChunkComponent.BlockSize, gameWorld.BlockSize.Z);

            int voxelStartX = blockStartX << Block.VoxelSizeBitShift;
            int voxelStartY = blockStartY << Block.VoxelSizeBitShift;
            int voxelStartZ = blockStartZ << Block.VoxelSizeBitShift;

            int voxelCount = 0;
            int renderedVoxelCount = 0;
            int polygonCount = 0;
            for (int blockZ = blockEndZ - 1; blockZ >= blockStartZ; blockZ--)
            {
                if (!chunkData.Visible)
                    break;

                for (int blockX = blockStartX; blockX < blockEndX; blockX++)
                {
                    for (int blockY = blockStartY; blockY < blockEndY; blockY++)
                    {
                        Block block = gameWorld[blockX, blockY, blockZ];
                        if (block == Block.Empty)
                            continue; // Empty block so there are no voxels to process

                        voxelCount += block.TotalVoxels;

                        CreateMeshForBlockInChunk(block, blockX, blockY, blockZ, voxelStartX, voxelStartY, voxelStartZ, voxelDetailLevel, scene,
                            meshBuilder, ref polygonCount, ref renderedVoxelCount);
                    }
                }
            }

            if (polygonCount == 0)
            {
                // Loading resulted in no mesh data (i.e. all blocks were empty) so just release the lock on the mesh builder since we're done with it.
                meshBuilder.DropMesh();
                return;
            }

            using (new PerformanceLock(chunkData.SyncLock))
            {
                if (!chunkData.Visible)
                {
                    // Entity became invisible while loading so just release the lock on the mesh builder since we're done with it.
                    meshBuilder.DropMesh();
                }
                else
                {
                    chunkData.MeshBuilder = meshBuilder;
                    chunkData.PolygonCount = polygonCount;
                    chunkData.VoxelCount = voxelCount;
                    chunkData.RenderedVoxelCount = renderedVoxelCount;
                }
            }
        }

        private static void CreateMeshForBlockInChunk(Block block, int blockX, int blockY, int blockZ,
            int voxelStartX, int voxelStartY, int voxelStartZ, byte voxelDetailLevel, Scene scene,
            MeshBuilder meshBuilder, ref int polygonCount, ref int renderedVoxelCount)
        {
            GameWorld gameWorld = scene.GameWorld;
            LightProvider lightProvider = scene.LightProvider;
            VoxelMeshHelper meshHelper = VoxelMeshHelper.Get(false);
            Voxel[] blockVoxels = block.VoxelsArray;

            int voxelSize = 1 << voxelDetailLevel;
            int maxVoxelX = gameWorld.VoxelSize.X - 1 - voxelSize;
            int maxVoxelY = gameWorld.VoxelSize.Y - 1 - voxelSize;
            int maxVoxelZ = gameWorld.VoxelSize.Z - 1 - voxelSize;
            int maxBlockVoxelSize = Block.VoxelSize - voxelSize;

            VoxelNoiseComponent voxelNoiseComponent = block.GetComponent<VoxelNoiseComponent>();

            //for (int bvz = Block.VoxelSize - 1; bvz >= 0; bvz -= voxelSize)
            for (int bvz = 0; bvz < Block.VoxelSize; bvz += voxelSize)
            {
                int voxelZ = (blockZ << Block.VoxelSizeBitShift) + bvz;
                byte chunkVoxelZ = (byte)(voxelZ - voxelStartZ);
                for (int bvx = 0; bvx < Block.VoxelSize; bvx += voxelSize)
                {
                    int voxelX = (blockX << Block.VoxelSizeBitShift) + bvx;
                    byte chunkVoxelX = (byte)(voxelX - voxelStartX);
                    for (int bvy = 0; bvy < Block.VoxelSize; bvy += voxelSize)
                    {
                        Voxel vox = (voxelSize == 1) ? blockVoxels[GetBlockVoxelOffset(bvx, bvy, bvz)] : GetLODVoxel(blockVoxels, bvz, bvx, bvy, voxelSize);
                        if (vox == Voxel.Empty)
                            continue;

                        int voxelY = (blockY << Block.VoxelSizeBitShift) + bvy;

                        VoxelSides sides = VoxelSides.None;

                        // Check to see if the back side is visible
                        if (bvz >= voxelSize)
                        {
                            if (blockVoxels[GetBlockVoxelOffset(bvx, bvy, bvz - voxelSize)] == Voxel.Empty)
                                sides |= VoxelSides.Back;
                        }
                        else if (voxelZ >= voxelSize && gameWorld.GetVoxel(voxelX, voxelY, voxelZ - voxelSize) == Voxel.Empty)
                            sides |= VoxelSides.Back;

                        // Check to see if the front side is visible
                        if (bvz < maxBlockVoxelSize)
                        {
                            if (blockVoxels[GetBlockVoxelOffset(bvx, bvy, bvz + voxelSize)] == Voxel.Empty)
                                sides |= VoxelSides.Front;
                        }
                        else if (voxelZ <= maxVoxelZ && gameWorld.GetVoxel(voxelX, voxelY, voxelZ + voxelSize) == Voxel.Empty)
                            sides |= VoxelSides.Front;

                        // Check to see if the left side is visible
                        if (bvx >= voxelSize)
                        {
                            if (blockVoxels[GetBlockVoxelOffset(bvx - voxelSize, bvy, bvz)] == Voxel.Empty)
                                sides |= VoxelSides.Left;
                        }
                        else if (voxelX >= voxelSize && gameWorld.GetVoxel(voxelX - voxelSize, voxelY, voxelZ) == Voxel.Empty)
                            sides |= VoxelSides.Left;

                        // Check to see if the right side is visible
                        if (bvx < maxBlockVoxelSize)
                        {
                            if (blockVoxels[GetBlockVoxelOffset(bvx + voxelSize, bvy, bvz)] == Voxel.Empty)
                                sides |= VoxelSides.Right;
                        }
                        else if (voxelX <= maxVoxelX && gameWorld.GetVoxel(voxelX + voxelSize, voxelY, voxelZ) == Voxel.Empty)
                            sides |= VoxelSides.Right;

                        // Check to see if the bottom side is visible
                        if (bvy >= voxelSize)
                        {
                            if (blockVoxels[GetBlockVoxelOffset(bvx, bvy - voxelSize, bvz)] == Voxel.Empty)
                                sides |= VoxelSides.Bottom;
                        }
                        else if (voxelY >= voxelSize && gameWorld.GetVoxel(voxelX, voxelY - voxelSize, voxelZ) == Voxel.Empty)
                            sides |= VoxelSides.Bottom;

                        // Check to see if the top side is visible
                        if (bvy < maxBlockVoxelSize)
                        {
                            if (blockVoxels[GetBlockVoxelOffset(bvx, bvy + voxelSize, bvz)] == Voxel.Empty)
                                sides |= VoxelSides.Top;
                        }
                        else if (voxelY <= maxVoxelY && gameWorld.GetVoxel(voxelX, voxelY + voxelSize, voxelZ) == Voxel.Empty)
                            sides |= VoxelSides.Top;

                        if (sides != VoxelSides.None)
                        {
                            bool ignoreLighting = vox.IgnoreLighting;

                            if (voxelNoiseComponent != null)
                                vox = voxelNoiseComponent.Adjust(vox);

                            polygonCount += meshHelper.AddVoxel(meshBuilder, sides, chunkVoxelX, (byte)(voxelY - voxelStartY), chunkVoxelZ,
                                ignoreLighting ? (Color4b)vox : lightProvider.GetFinalColor(vox, voxelX, voxelY, voxelZ, voxelSize, blockX, blockY, blockZ, sides), voxelSize);
                            renderedVoxelCount++;
                        }
                    }
                }
            }
        }

        private static Voxel GetLODVoxel(Voxel[] blockVoxels, int bvz, int bvx, int bvy, int voxelSize)
        {
            int voxelsFound = 0;
            int totalA = 0;
            int totalR = 0;
            int totalG = 0;
            int totalB = 0;
            int maxX = bvx + voxelSize;
            int maxY = bvy + voxelSize;
            int maxZ = bvz + voxelSize;
            //for (int z = bvz; z > bvz - voxelSize; z--)
            VoxelSettings settings = VoxelSettings.None;
            for (int z = bvz; z < maxZ; z++)
            {
                for (int x = bvx; x < maxX; x++)
                {
                    for (int y = bvy; y < maxY; y++)
                    {
                        Voxel otherColor = blockVoxels[GetBlockVoxelOffset(x, y, z)];
                        if (otherColor == Voxel.Empty)
                            continue;

                        voxelsFound++;
                        totalA += otherColor.A;
                        totalR += otherColor.R;
                        totalG += otherColor.G;
                        totalB += otherColor.B;
                        settings |= otherColor.Settings;
                    }
                }
            }

            if (voxelsFound == 0) // Prevent divide-by-zero
                return Voxel.Empty;

            return new Voxel((byte)(totalR / voxelsFound), (byte)(totalG / voxelsFound), (byte)(totalB / voxelsFound), (byte)(totalA / voxelsFound), settings);
        }
        #endregion

        #region Methods for loading entity meshes
        private void LoadMesh(IEntity entity, VoxelMeshComponent renderData, MeshBuilder meshBuilder, int voxelDetailLevel)
        {
        }
        #endregion

        #region Other private helper methods
        /// <summary>
        /// Given the specified render data and camera data determine the wanted voxel detail level given the specified user detail setting
        /// </summary>
        private static byte GetPerferedVoxelDetailLevel(VoxelMeshComponent renderData, CameraComponent cameraData, 
            VoxelDetailLevelDistance currentVoxelDetalLevelSetting)
        {
            // TODO: Make the renderdata location be the center of the renderdata instead of the corner
            float distX = renderData.Location.X - cameraData.Location.X;
            float distY = renderData.Location.Y - cameraData.Location.Y;
            float distZ = renderData.Location.Z - cameraData.Location.Z;

            int dist = (int)Math.Sqrt(distX * distX + distY * distY + distZ * distZ);
            //if (dist > cameraData.FarDistance / 2)
            //    return BestVoxelDetailLevel + 1;
            //return BestVoxelDetailLevel;
            int distancePerLevel;
            switch (currentVoxelDetalLevelSetting)
            {
                case VoxelDetailLevelDistance.Closest: distancePerLevel = 300; break;
                case VoxelDetailLevelDistance.Close: distancePerLevel = 450; break;
                case VoxelDetailLevelDistance.Mid: distancePerLevel = 600; break;
                case VoxelDetailLevelDistance.Far: distancePerLevel = 750; break;
                default: distancePerLevel = 900; break;
            }

            for (byte i = BestVoxelDetailLevel; i <= WorstVoxelDetailLevel; i++)
            {
                if (dist <= distancePerLevel)
                    return i;
                dist -= distancePerLevel * (i + 1);
            }
            return WorstVoxelDetailLevel;
        }

        /// <summary>
        /// Queues the specified entity to be loaded at the specified detail level.
        /// </summary>
        private void LoadEntity(IEntity entity, VoxelMeshComponent renderData, byte voxelDetailLevel)
        {
            using (new PerformanceLock(renderData.SyncLock))
                renderData.Visible = true;
            loadedEntities.Add(entity);
            ReloadEntity(entity, voxelDetailLevel);
        }

        private void ReloadEntity(IEntity entity, byte voxelDetailLevel)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            Debug.Assert(loadedEntities.Contains(entity));

            using (new PerformanceLock(entityLoadQueue))
            {
                if (!entityLoadQueue.Contains(entity, voxelDetailLevel))
                    entityLoadQueue.Enqueue(entity, voxelDetailLevel);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetBlockVoxelOffset(int x, int y, int z)
        {
            return (((z << Block.VoxelSizeBitShift) + x) << Block.VoxelSizeBitShift) + y; // y-axis major for speed
        }
        #endregion
    }
}
