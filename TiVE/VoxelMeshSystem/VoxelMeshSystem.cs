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

    /// <summary>
    /// User setting for the voxel detail distance
    /// </summary>
    internal enum ShadowDistance
    {
        None = 0,
        Closest = 1,
        Close = 2,
        Mid = 3,
        Far = 4,
        Furthest = 5
    }

    internal sealed class VoxelMeshSystem : EngineSystem
    {
        #region Constants
        private const byte VoxelDetailLevelSections = 4; // 32x32x32 = 32768v, 16x16x16 = 4096v, 8x8x8 = 512v, 4x4x4 = 64v, not worth going to 2x2x2 = 8v.
        private const byte BestVoxelDetailLevel = 0;
        private const byte WorstVoxelDetailLevel = VoxelDetailLevelSections - 1;
        private const int TotalChunkMeshBuilders = 40;
        private const int TotalSpriteMeshBuilders = 3;
        #endregion

        #region Member variables
        private readonly HashSet<IEntity> loadedEntities = new HashSet<IEntity>();

        private readonly List<Thread> meshCreationThreads = new List<Thread>();
        private readonly EntityMeshLoadQueue entityLoadQueue = new EntityMeshLoadQueue();
        private readonly List<MeshBuilder2> chunkMeshBuilders = new List<MeshBuilder2>(TotalChunkMeshBuilders);
        private readonly List<MeshBuilder2> spriteMeshBuilders = new List<MeshBuilder2>(TotalSpriteMeshBuilders);

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
                chunkMeshBuilders.Add(new MeshBuilder2(300000));

            for (int i = 0; i < TotalSpriteMeshBuilders; i++)
                spriteMeshBuilders.Add(new MeshBuilder2(3000000)); // Sprites can be max of 256x256x256 and contain a lot of voxels

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

            foreach (MeshBuilder2 meshBuilder in chunkMeshBuilders.Concat(spriteMeshBuilders)) // Make sure all mesh builders are available for the new scene
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
                entityLoadQueue.Sort(cameraData);

                foreach (IEntity entity in cameraData.NewlyHiddenEntitites.Where(e => e.HasComponent<VoxelMeshComponent>()))
                {
                    entityLoadQueue.Remove(entity);
                    loadedEntities.Remove(entity);
                }
            }

            VoxelDetailLevelDistance currentVoxelDetalLevelSetting = (VoxelDetailLevelDistance)(int)TiVEController.UserSettings.Get(UserSettings.DetailDistanceKey);
            ShadowDistance currentShadowDistanceSetting = (ShadowDistance)(int)TiVEController.UserSettings.Get(UserSettings.ShadowDistanceKey);

            foreach (IEntity entity in loadedEntities)
            {
                VoxelMeshComponent renderData = entity.GetComponent<VoxelMeshComponent>();
                Debug.Assert(renderData != null);

                if (renderData.MeshData != null)
                {
                    byte perferedDetailLevel = GetPerferedVoxelDetailLevel(renderData, cameraData, currentVoxelDetalLevelSetting);
                    ShadowType shadowType = GetPerferedShadowType(renderData, cameraData, currentShadowDistanceSetting);
                    if (NeedToUpdateMesh(renderData, perferedDetailLevel, shadowType))
                        ReloadEntityMesh(entity, renderData, perferedDetailLevel, shadowType);
                }
            }

            foreach (IEntity entity in cameraData.NewlyVisibleEntitites.Where(e => e.HasComponent<VoxelMeshComponent>()))
            {
                VoxelMeshComponent renderData = entity.GetComponent<VoxelMeshComponent>();
                Debug.Assert(renderData != null);
                EnsureVisible(entity, renderData);

                byte perferedDetailLevel = GetPerferedVoxelDetailLevel(renderData, cameraData, currentVoxelDetalLevelSetting);
                ShadowType shadowType = GetPerferedShadowType(renderData, cameraData, currentShadowDistanceSetting);
                if (NeedToUpdateMesh(renderData, perferedDetailLevel, shadowType))
                    ReloadEntityMesh(entity, renderData, perferedDetailLevel, shadowType);
            }

            if (currentScene.LoadingInitialChunks && entityLoadQueue.Size < 5) // Let chunks load before showing scene
                currentScene.LoadingInitialChunks = false;

            return true;
        }
        #endregion

        private static bool NeedToUpdateMesh(VoxelMeshComponent renderData, byte perferedDetailLevel, ShadowType shadowType)
        {
            return (renderData.VisibleVoxelDetailLevel != perferedDetailLevel || renderData.VisibleShadowType != (byte)shadowType) &&
                (renderData.VoxelDetailLevelToLoad != perferedDetailLevel || renderData.ShadowTypeToLoad != (byte)shadowType);
        }

        #region Public methods
        public void ReloadAllEntities()
        {
            foreach (IEntity chunk in loadedEntities)
                ReloadEntityMesh(chunk, chunk.GetComponent<VoxelMeshComponent>(), WorstVoxelDetailLevel, ShadowType.None); // TODO: Reload with the correct detail level and shadow type
        }
        #endregion

        #region Event handlers
        private void UserSettings_SettingChanged(string settingName, Setting newValue)
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

                MeshBuilder2 meshBuilder;
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

                EntityLoadQueueItem queueItem;
                using (new PerformanceLock(entityLoadQueue))
                    queueItem = entityLoadQueue.Dequeue();

                if (queueItem == null)
                {
                    // Couldn't find a entity to load. No need to hold onto the mesh builder.
                    meshBuilder.DropMesh();
                    continue;
                }

                VoxelMeshComponent renderData = queueItem.Entity.GetComponent<VoxelMeshComponent>();
                Debug.Assert(renderData != null);

                if (!renderData.Visible)
                {
                    // Entity became invisible while waiting to be loaded. No need to hold onto the mesh builder.
                    meshBuilder.DropMesh();
                    continue;
                }

                ChunkComponent chunkData = renderData as ChunkComponent;
                if (chunkData != null)
                    LoadChunkMesh(chunkData, meshBuilder, queueItem);
                else
                    LoadMesh(queueItem.Entity, renderData, meshBuilder, queueItem);
            }
        }
        #endregion

        #region Methods for loading chunk meshes
        private void LoadChunkMesh(ChunkComponent chunkData, MeshBuilder2 meshBuilder, EntityLoadQueueItem queueItem)
        {
            Scene scene = loadedScene; // For thread safety

            Debug.Assert(meshBuilder.IsLocked);
            Debug.Assert(loadedScene != null);
            //Debug.WriteLine("Started chunk ({0},{1},{2})", chunkStartX, chunkStartY, chunkStartZ);

            GameWorld gameWorld = scene.GameWorldInternal;

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

                        CreateMeshForBlockInChunk(block, blockX, blockY, blockZ, voxelStartX, voxelStartY, voxelStartZ, queueItem, scene,
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
                if (!chunkData.Visible || chunkData.VoxelDetailLevelToLoad != queueItem.DetailLevel)
                {
                    // Entity became invisible while loading or a new detail level was requested while loading 
                    // so just release the lock on the mesh builder since we're done with it.
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
            int voxelStartX, int voxelStartY, int voxelStartZ, EntityLoadQueueItem queueItem, Scene scene,
            MeshBuilder2 meshBuilder, ref int polygonCount, ref int renderedVoxelCount)
        {
            GameWorld gameWorld = scene.GameWorldInternal;
            LightProvider lightProvider = scene.GetLightProvider(queueItem.ShadowType);
            VoxelMeshHelper meshHelper = VoxelMeshHelper.Get(false);
            Voxel[] blockVoxels = block.VoxelsArray;

            int voxelSize = 1 << queueItem.DetailLevel;
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

                            polygonCount += meshHelper.AddVoxel2(meshBuilder, sides, chunkVoxelX, (byte)(voxelY - voxelStartY), chunkVoxelZ,
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
        private void LoadMesh(IEntity entity, VoxelMeshComponent renderData, MeshBuilder2 meshBuilder, EntityLoadQueueItem queueItem)
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

            float dist = distX * distX + distY * distY + distZ * distZ;
            float distancePerLevel;
            switch (currentVoxelDetalLevelSetting)
            {
                case VoxelDetailLevelDistance.Closest: distancePerLevel = 90000; break; // 300v
                case VoxelDetailLevelDistance.Close: distancePerLevel = 202500; break;  // 450v
                case VoxelDetailLevelDistance.Mid: distancePerLevel = 360000; break;    // 600v
                case VoxelDetailLevelDistance.Far: distancePerLevel = 562500; break;    // 750v
                default: distancePerLevel = 810000; break;                              // 900v
            }

            for (byte i = BestVoxelDetailLevel; i <= WorstVoxelDetailLevel; i++)
            {
                if (dist <= distancePerLevel)
                    return i;
                dist -= distancePerLevel;// *(i + 1);
            }
            return WorstVoxelDetailLevel;
        }

        /// <summary>
        /// Given the specified render data and camera data determine the wanted voxel detail level given the specified user detail setting
        /// </summary>
        private static ShadowType GetPerferedShadowType(VoxelMeshComponent renderData, CameraComponent cameraData,
            ShadowDistance currentShadowDistanceSetting)
        {
            if (currentShadowDistanceSetting == ShadowDistance.None)
                return ShadowType.None;

            // TODO: Make the renderdata location be the center of the renderdata instead of the corner
            float distX = renderData.Location.X - cameraData.Location.X;
            float distY = renderData.Location.Y - cameraData.Location.Y;
            float distZ = renderData.Location.Z - cameraData.Location.Z;

            float dist = distX * distX + distY * distY + distZ * distZ;
            float shadowDistance;
            switch (currentShadowDistanceSetting)
            {
                case ShadowDistance.Closest: shadowDistance = 90000; break; // 300v
                case ShadowDistance.Close: shadowDistance = 202500; break;  // 450v
                case ShadowDistance.Mid: shadowDistance = 360000; break;    // 600v
                case ShadowDistance.Far: shadowDistance = 562500; break;    // 750v
                default: shadowDistance = 810000; break;                    // 900v
            }

            return dist <= shadowDistance ? ShadowType.Nice : ShadowType.None;
        }

        private void ReloadEntityMesh(IEntity entity, VoxelMeshComponent renderData, byte voxelDetailLevel, ShadowType shadowType)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            Debug.Assert(loadedEntities.Contains(entity));

            using (new PerformanceLock(renderData.SyncLock))
            {
                renderData.VoxelDetailLevelToLoad = voxelDetailLevel;
                renderData.ShadowTypeToLoad = (byte)shadowType;
            }

            using (new PerformanceLock(entityLoadQueue))
                entityLoadQueue.Enqueue(new EntityLoadQueueItem(entity, voxelDetailLevel, shadowType));
        }

        private void EnsureVisible(IEntity entity, VoxelMeshComponent renderData)
        {
            using (new PerformanceLock(renderData.SyncLock))
                renderData.Visible = true;
            loadedEntities.Add(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetBlockVoxelOffset(int x, int y, int z)
        {
            return (((z << Block.VoxelSizeBitShift) + x) << Block.VoxelSizeBitShift) + y; // y-axis major for speed
        }
        #endregion
    }
}
