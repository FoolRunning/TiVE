using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVE.RenderSystem.Lighting;
using ProdigalSoftware.TiVE.RenderSystem.World;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;
using ProdigalSoftware.TiVEPluginFramework.Internal;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.VoxelMeshSystem
{
    internal sealed class VoxelMeshSystem : EngineSystem
    {
        #region Constants
        private const int TotalChunkMeshBuilders = 40;

        //private const int DistClosest = Block.VoxelSize * Block.VoxelSize * 225;   // voxelSize * 15 = 480v
        //private const int DistClose = Block.VoxelSize * Block.VoxelSize * 676;     // voxelSize * 26 = 832v
        //private const int DistMid = Block.VoxelSize * Block.VoxelSize * 1369;      // voxelSize * 37 = 1184v
        //private const int DistFar = Block.VoxelSize * Block.VoxelSize * 2401;      // voxelSize * 49 = 1568v
        //private const int DistFurthest = Block.VoxelSize * Block.VoxelSize * 3600; // voxelSize * 60 = 1920v
        private const int DistClosest = BlockLOD32.VoxelSize * 12;   // voxelSize * 12 = 384v
        private const int DistClose = BlockLOD32.VoxelSize * 19;     // voxelSize * 19 = 608v
        private const int DistMid = BlockLOD32.VoxelSize * 26;       // voxelSize * 26 = 832v
        private const int DistFar = BlockLOD32.VoxelSize * 33;       // voxelSize * 33 = 1056v
        private const int DistFurthest = BlockLOD32.VoxelSize * 40;  // voxelSize * 40 = 1280v
        #endregion

        #region Member variables
        private readonly HashSet<IEntity> loadedEntities = new HashSet<IEntity>();

        private readonly List<Thread> meshCreationThreads = new List<Thread>();
        private readonly EntityMeshLoadQueue entityLoadQueue = new EntityMeshLoadQueue();
        private readonly List<MeshBuilder> meshBuilders = new List<MeshBuilder>(TotalChunkMeshBuilders);

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
                meshBuilders.Add(new MeshBuilder(750000));

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

            foreach (MeshBuilder meshBuilder in meshBuilders) // Make sure all mesh builders are available for the new scene
                meshBuilder.DropMesh();

            loadedEntities.Clear();

            // Start threads for new scene
            endCreationThreads = false;
            for (int i = 0; i < meshCreationThreads.Count; i++)
                meshCreationThreads[i] = StartMeshCreateThread(i);

            loadedScene = newScene;
            newScene.LoadingInitialChunks = true;
        }

        protected override bool UpdateInternal(int ticksSinceLastUpdate, Scene currentScene)
        {
            CameraComponent cameraData = currentScene.FindCamera();
            if (cameraData == null)
                return true;

            VoxelDetailLevelDistance currentVoxelDetalLevelSetting = (VoxelDetailLevelDistance)(int)TiVEController.UserSettings.Get(UserSettings.DetailDistanceKey);

            using (new PerformanceLock(entityLoadQueue))
            {
                entityLoadQueue.Sort(cameraData);

                foreach (IEntity entity in cameraData.NewlyHiddenEntitites.Where(e => e.HasComponent<VoxelMeshComponent>()))
                {
                    entityLoadQueue.Remove(entity);
                    loadedEntities.Remove(entity);
                }
            }

            foreach (IEntity entity in loadedEntities)
            {
                VoxelMeshComponent renderData = entity.GetComponent<VoxelMeshComponent>();
                Debug.Assert(renderData != null);

                LODLevel perferedDetailLevel = GetPerferedVoxelDetailLevel(renderData, cameraData, currentVoxelDetalLevelSetting);
                if (NeedToUpdateMesh(renderData, perferedDetailLevel))
                    ReloadEntityMesh(entity, renderData, perferedDetailLevel);
            }

            foreach (IEntity entity in cameraData.NewlyVisibleEntitites.Where(e => e.HasComponent<VoxelMeshComponent>()))
            {
                VoxelMeshComponent renderData = entity.GetComponent<VoxelMeshComponent>();
                Debug.Assert(renderData != null);
                EnsureVisible(entity, renderData);

                LODLevel perferedDetailLevel = GetPerferedVoxelDetailLevel(renderData, cameraData, currentVoxelDetalLevelSetting);
                if (NeedToUpdateMesh(renderData, perferedDetailLevel))
                    ReloadEntityMesh(entity, renderData, perferedDetailLevel);
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
                ReloadEntityMesh(chunk, chunk.GetComponent<VoxelMeshComponent>(), LODLevel.V4); // TODO: Reload with the correct detail level and shadow type
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

                MeshBuilder meshBuilder;
                using (new PerformanceLock(meshBuilders))
                {
                    meshBuilder = meshBuilders.Find(mb => !mb.IsLocked);
                    meshBuilder?.StartNewMesh(); // Found a mesh builder - grab it quick!
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
        private void LoadChunkMesh(ChunkComponent chunkData, MeshBuilder meshBuilder, EntityLoadQueueItem queueItem)
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

            int bitShift = BlockLOD.GetVoxelSizeBitShift(queueItem.DetailLevel);
            int voxelStartX = blockStartX << bitShift;
            int voxelStartY = blockStartY << bitShift;
            int voxelStartZ = blockStartZ << bitShift;

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
                    if (polygonCount > 0)
                        chunkData.MeshBuilder = meshBuilder;
                    else
                    {
                        // Loading resulted in no mesh data (i.e. all blocks were empty). Since there is no mesh, we can just release the lock on the mesh builder
                        // since we're done with it. However, we need to make sure that this chunk isn't re-loaded again later so we pretend like it was initialized.
                        meshBuilder.DropMesh();
                        chunkData.VisibleVoxelDetailLevel = chunkData.VoxelDetailLevelToLoad;
                        chunkData.VoxelDetailLevelToLoad = LODLevel.NotSet;
                        return;
                    }
                    chunkData.PolygonCount = polygonCount;
                    chunkData.VoxelCount = voxelCount;
                    chunkData.RenderedVoxelCount = renderedVoxelCount;
                }
            }
        }

        private static void CreateMeshForBlockInChunk(Block block, int blockX, int blockY, int blockZ,
            int voxelStartX, int voxelStartY, int voxelStartZ, EntityLoadQueueItem queueItem, Scene scene,
            MeshBuilder meshBuilder, ref int polygonCount, ref int renderedVoxelCount)
        {
            LODLevel detailLevel = queueItem.DetailLevel;

            int shadowDetailOffset = TiVEController.UserSettings.Get(UserSettings.WorldShadowDetailKey);
            bool useShadows = (int)detailLevel + shadowDetailOffset < (int)LODLevel.NumOfLevels;
            LightProvider lightProvider = scene.GetLightProvider(useShadows);
            LODLevel shadowDetailLevel = (LODLevel)Math.Min((int)detailLevel + shadowDetailOffset, (int)LODLevel.V4);

            GameWorld gameWorld = scene.GameWorldInternal;
            BlockLOD blockLOD = block.GetLOD(detailLevel);

            int maxBlockVoxel = blockLOD.VoxelAxisSize - 1;
            int bitShift = blockLOD.VoxelAxisSizeBitShift;
            int renderedVoxelSize = blockLOD.RenderedVoxelSize;
            int maxBlockX = gameWorld.BlockSize.X - 1;
            int maxBlockY = gameWorld.BlockSize.Y - 1;
            int maxBlockZ = gameWorld.BlockSize.Z - 1;

            VoxelNoiseComponent voxelNoiseComponent = block.GetComponent<VoxelNoiseComponent>();
            RenderedVoxel[] renderedVoxels = blockLOD.RenderedVoxels;
            for (int i = 0; i < renderedVoxels.Length; i++)
            {
                RenderedVoxel renVox = renderedVoxels[i];
                int bvx = renVox.Location.X;
                int bvy = renVox.Location.Y;
                int bvz = renVox.Location.Z;
                int voxelX = (blockX << bitShift) + bvx;
                int voxelY = (blockY << bitShift) + bvy;
                int voxelZ = (blockZ << bitShift) + bvz;
                Voxel vox = renVox.Voxel;
                CubeSides sides = renVox.Sides;
                if (renVox.CheckSurroundingVoxels)
                {
                    // Check to see if the back side is really visible
                    if (bvz == 0 && blockZ > 0 && gameWorld[blockX, blockY, blockZ - 1] != Block.Empty && gameWorld.GetVoxel(voxelX, voxelY, voxelZ - 1, detailLevel) != Voxel.Empty)
                        sides ^= CubeSides.Back;

                    // Check to see if the front side is really visible
                    if (bvz == maxBlockVoxel && blockZ < maxBlockZ && gameWorld[blockX, blockY, blockZ + 1] != Block.Empty && gameWorld.GetVoxel(voxelX, voxelY, voxelZ + 1, detailLevel) != Voxel.Empty)
                        sides ^= CubeSides.Front;

                    // Check to see if the left side is really visible
                    if (bvx == 0 && blockX > 0 && gameWorld[blockX - 1, blockY, blockZ] != Block.Empty && gameWorld.GetVoxel(voxelX - 1, voxelY, voxelZ, detailLevel) != Voxel.Empty)
                        sides ^= CubeSides.Left;

                    // Check to see if the right side is really visible
                    if (bvx == maxBlockVoxel && blockX < maxBlockX && gameWorld[blockX + 1, blockY, blockZ] != Block.Empty && gameWorld.GetVoxel(voxelX + 1, voxelY, voxelZ, detailLevel) != Voxel.Empty)
                        sides ^= CubeSides.Right;

                    // Check to see if the bottom side is really visible
                    if (bvy == 0 && blockY > 0 && gameWorld[blockX, blockY - 1, blockZ] != Block.Empty && gameWorld.GetVoxel(voxelX, voxelY - 1, voxelZ, detailLevel) != Voxel.Empty)
                        sides ^= CubeSides.Bottom;

                    // Check to see if the top side is really visible
                    if (bvy == maxBlockVoxel && blockY < maxBlockY && gameWorld[blockX, blockY + 1, blockZ] != Block.Empty && gameWorld.GetVoxel(voxelX, voxelY + 1, voxelZ, detailLevel) != Voxel.Empty)
                        sides ^= CubeSides.Top;
                }
                
                if (sides != CubeSides.None)
                {
                    if (voxelNoiseComponent != null)
                        vox = voxelNoiseComponent.Adjust(vox);

                    byte chunkVoxelX = (byte)((voxelX - voxelStartX) * renderedVoxelSize);
                    byte chunkVoxelY = (byte)((voxelY - voxelStartY) * renderedVoxelSize);
                    byte chunkVoxelZ = (byte)((voxelZ - voxelStartZ) * renderedVoxelSize);

                    polygonCount += meshBuilder.AddVoxel(sides, chunkVoxelX, chunkVoxelY, chunkVoxelZ, 
                        vox.IgnoreLighting ? (Color4b)vox : lightProvider.GetFinalColor(vox, voxelX, voxelY, voxelZ, detailLevel, shadowDetailLevel, blockX, blockY, blockZ, sides));
                    renderedVoxelCount++;
                }
            }
        }
        #endregion

        #region Methods for loading entity meshes
        private void LoadMesh(IEntity entity, VoxelMeshComponent renderData, MeshBuilder meshBuilder, EntityLoadQueueItem queueItem)
        {
        }
        #endregion

        #region Other private helper methods
        /// <summary>
        /// Given the specified render data and camera data determine the wanted voxel detail level given the specified user detail setting
        /// </summary>
        private static LODLevel GetPerferedVoxelDetailLevel(VoxelMeshComponent renderData, CameraComponent cameraData, 
            VoxelDetailLevelDistance currentVoxelDetalLevelSetting)
        {
            // TODO: Make the renderdata location be the center of the renderdata instead of the corner
            float distX = renderData.Location.X - cameraData.Location.X;
            float distY = renderData.Location.Y - cameraData.Location.Y;
            float distZ = renderData.Location.Z - cameraData.Location.Z;

            float dist = MathUtils.FastSqrt(distX * distX + distY * distY + distZ * distZ);
            float distancePerLevel;
            switch (currentVoxelDetalLevelSetting)
            {
                case VoxelDetailLevelDistance.Closest: distancePerLevel = DistClosest; break;
                case VoxelDetailLevelDistance.Close: distancePerLevel = DistClose; break;
                case VoxelDetailLevelDistance.Mid: distancePerLevel = DistMid; break;
                case VoxelDetailLevelDistance.Far: distancePerLevel = DistFar; break;
                default: distancePerLevel = DistFurthest; break;
            }

            for (int level = (int)LODLevel.V32; level < (int)LODLevel.NumOfLevels; level++)
            {
                if (dist <= distancePerLevel)
                    return (LODLevel)level;
                dist -= distancePerLevel;
            }
            return LODLevel.V4;
        }
        
        private static bool NeedToUpdateMesh(VoxelMeshComponent renderData, LODLevel perferedDetailLevel)
        {
            return renderData.VoxelDetailLevelToLoad != perferedDetailLevel && renderData.VisibleVoxelDetailLevel != perferedDetailLevel;
        }

        private void ReloadEntityMesh(IEntity entity, VoxelMeshComponent renderData, LODLevel voxelDetailLevel)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            Debug.Assert(loadedEntities.Contains(entity));

            using (new PerformanceLock(renderData.SyncLock))
                renderData.VoxelDetailLevelToLoad = voxelDetailLevel;

            using (new PerformanceLock(entityLoadQueue))
                entityLoadQueue.Enqueue(new EntityLoadQueueItem(entity, voxelDetailLevel));
        }

        private void EnsureVisible(IEntity entity, VoxelMeshComponent renderData)
        {
            using (new PerformanceLock(renderData.SyncLock))
                renderData.Visible = true;
            loadedEntities.Add(entity);
        }
        #endregion
    }
}
