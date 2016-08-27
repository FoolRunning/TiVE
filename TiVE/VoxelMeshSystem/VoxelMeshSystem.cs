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
                meshBuilders.Add(new MeshBuilder(500000));

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

        protected override bool UpdateInternal(int ticksSinceLastUpdate, float timeBlendFactor, Scene currentScene)
        {
            CameraComponent cameraData = currentScene.FindCamera();
            if (cameraData == null)
                return true;

            VoxelDetailLevelDistance currentVoxelDetalLevelSetting = (VoxelDetailLevelDistance)(int)TiVEController.UserSettings.Get(UserSettings.DetailDistanceKey);
            ShadowDistance currentShadowDistanceSetting = (ShadowDistance)(int)TiVEController.UserSettings.Get(UserSettings.ShadowDistanceKey);

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
                ShadowType shadowType = GetPerferedShadowType(renderData, cameraData, currentShadowDistanceSetting);
                if (NeedToUpdateMesh(renderData, perferedDetailLevel, shadowType))
                    ReloadEntityMesh(entity, renderData, perferedDetailLevel, shadowType);
            }

            foreach (IEntity entity in cameraData.NewlyVisibleEntitites.Where(e => e.HasComponent<VoxelMeshComponent>()))
            {
                VoxelMeshComponent renderData = entity.GetComponent<VoxelMeshComponent>();
                Debug.Assert(renderData != null);
                EnsureVisible(entity, renderData);

                LODLevel perferedDetailLevel = GetPerferedVoxelDetailLevel(renderData, cameraData, currentVoxelDetalLevelSetting);
                ShadowType shadowType = GetPerferedShadowType(renderData, cameraData, currentShadowDistanceSetting);
                if (NeedToUpdateMesh(renderData, perferedDetailLevel, shadowType))
                    ReloadEntityMesh(entity, renderData, perferedDetailLevel, shadowType);
            }

            if (currentScene.LoadingInitialChunks && entityLoadQueue.Size < 5) // Let chunks load before showing scene
                currentScene.LoadingInitialChunks = false;

            return true;
        }
        #endregion

        private static bool NeedToUpdateMesh(VoxelMeshComponent renderData, LODLevel perferedDetailLevel, ShadowType perferedShadowType)
        {
            return (renderData.VoxelDetailLevelToLoad != perferedDetailLevel && renderData.VisibleVoxelDetailLevel != perferedDetailLevel) ||
                (renderData.ShadowTypeToLoad != (byte)perferedShadowType && renderData.VisibleShadowType != (byte)perferedShadowType);
        }

        #region Public methods
        public void ReloadAllEntities()
        {
            foreach (IEntity chunk in loadedEntities)
                ReloadEntityMesh(chunk, chunk.GetComponent<VoxelMeshComponent>(), LODLevel.V4, ShadowType.None); // TODO: Reload with the correct detail level and shadow type
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
                        chunkData.VisibleShadowType = chunkData.ShadowTypeToLoad;
                        chunkData.VoxelDetailLevelToLoad = LODLevel.NotSet;
                        chunkData.ShadowTypeToLoad = VoxelMeshComponent.BlankShadowType;
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
            GameWorld gameWorld = scene.GameWorldInternal;
            LightProvider lightProviderShadow = scene.GetLightProvider(ShadowType.Nice);
            LightProvider lightProviderNoShadow = scene.GetLightProvider(ShadowType.None);
            LODLevel detailLevel = queueItem.DetailLevel;
            BlockLOD blockLOD = block.GetLOD(detailLevel);

            int maxBlockVoxel = blockLOD.VoxelAxisSize - 1;
            int bitShift = blockLOD.VoxelAxisSizeBitShift;
            int renderedVoxelSize = blockLOD.RenderedVoxelSize;
            int maxVoxelX = (gameWorld.BlockSize.X << bitShift) - 1;
            int maxVoxelY = (gameWorld.BlockSize.Y << bitShift) - 1;
            int maxVoxelZ = (gameWorld.BlockSize.Z << bitShift) - 1;

            VoxelNoiseComponent voxelNoiseComponent = block.GetComponent<VoxelNoiseComponent>();

            //for (int bvz = Block.VoxelSize - 1; bvz >= 0; bvz -= voxelSize)
            for (int bvz = 0; bvz <= maxBlockVoxel; bvz++)
            {
                int voxelZ = (blockZ << bitShift) + bvz;
                byte chunkVoxelZ = (byte)((voxelZ - voxelStartZ) * renderedVoxelSize);
                for (int bvx = 0; bvx <= maxBlockVoxel; bvx++)
                {
                    int voxelX = (blockX << bitShift) + bvx;
                    byte chunkVoxelX = (byte)((voxelX - voxelStartX) * renderedVoxelSize);
                    for (int bvy = 0; bvy <= maxBlockVoxel; bvy++)
                    {
                        Voxel vox = blockLOD.VoxelAt(bvx, bvy, bvz);
                        if (vox == Voxel.Empty)
                            continue;

                        int voxelY = (blockY << bitShift) + bvy;

                        VoxelSides sides = VoxelSides.None;

                        // Check to see if the back side is visible
                        if (bvz > 0)
                        {
                            if (blockLOD.VoxelAt(bvx, bvy, bvz - 1) == Voxel.Empty)
                                sides |= VoxelSides.Back;
                        }
                        else if (voxelZ > 0 && gameWorld.GetVoxel(voxelX, voxelY, voxelZ - 1, detailLevel) == Voxel.Empty)
                            sides |= VoxelSides.Back;

                        // Check to see if the front side is visible
                        if (bvz < maxBlockVoxel)
                        {
                            if (blockLOD.VoxelAt(bvx, bvy, bvz + 1) == Voxel.Empty)
                                sides |= VoxelSides.Front;
                        }
                        else if (voxelZ < maxVoxelZ && gameWorld.GetVoxel(voxelX, voxelY, voxelZ + 1, detailLevel) == Voxel.Empty)
                            sides |= VoxelSides.Front;

                        // Check to see if the left side is visible
                        if (bvx > 0)
                        {
                            if (blockLOD.VoxelAt(bvx - 1, bvy, bvz) == Voxel.Empty)
                                sides |= VoxelSides.Left;
                        }
                        else if (voxelX > 0 && gameWorld.GetVoxel(voxelX - 1, voxelY, voxelZ, detailLevel) == Voxel.Empty)
                            sides |= VoxelSides.Left;

                        // Check to see if the right side is visible
                        if (bvx < maxBlockVoxel)
                        {
                            if (blockLOD.VoxelAt(bvx + 1, bvy, bvz) == Voxel.Empty)
                                sides |= VoxelSides.Right;
                        }
                        else if (voxelX < maxVoxelX && gameWorld.GetVoxel(voxelX + 1, voxelY, voxelZ, detailLevel) == Voxel.Empty)
                            sides |= VoxelSides.Right;

                        // Check to see if the bottom side is visible
                        if (bvy > 0)
                        {
                            if (blockLOD.VoxelAt(bvx, bvy - 1, bvz) == Voxel.Empty)
                                sides |= VoxelSides.Bottom;
                        }
                        else if (voxelY > 0 && gameWorld.GetVoxel(voxelX, voxelY - 1, voxelZ, detailLevel) == Voxel.Empty)
                            sides |= VoxelSides.Bottom;

                        // Check to see if the top side is visible
                        if (bvy < maxBlockVoxel)
                        {
                            if (blockLOD.VoxelAt(bvx, bvy + 1, bvz) == Voxel.Empty)
                                sides |= VoxelSides.Top;
                        }
                        else if (voxelY < maxVoxelY && gameWorld.GetVoxel(voxelX, voxelY + 1, voxelZ, detailLevel) == Voxel.Empty)
                            sides |= VoxelSides.Top;

                        if (sides != VoxelSides.None)
                        {
                            if (voxelNoiseComponent != null)
                                vox = voxelNoiseComponent.Adjust(vox);

                            LightProvider lightProvider = vox.AllowLightPassthrough ? lightProviderNoShadow : lightProviderShadow;
                            polygonCount += meshBuilder.AddVoxel(sides, chunkVoxelX, (byte)((voxelY - voxelStartY) * renderedVoxelSize), chunkVoxelZ,
                                vox.IgnoreLighting ? (Color4b)vox : lightProvider.GetFinalColor(vox, voxelX, voxelY, voxelZ, detailLevel, blockX, blockY, blockZ, sides));
                            renderedVoxelCount++;
                        }
                    }
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

            float dist = (float)Math.Sqrt(distX * distX + distY * distY + distZ * distZ);
            float distancePerLevel;
            switch (currentVoxelDetalLevelSetting)
            {
                case VoxelDetailLevelDistance.Closest: distancePerLevel = DistClosest; break;
                case VoxelDetailLevelDistance.Close: distancePerLevel = DistClose; break;
                case VoxelDetailLevelDistance.Mid: distancePerLevel = DistMid; break;
                case VoxelDetailLevelDistance.Far: distancePerLevel = DistFar; break;
                default: distancePerLevel = DistFurthest; break;
            }

            for (byte level = (byte)LODLevel.V32; level < (byte)LODLevel.NumOfLevels; level++)
            {
                if (dist <= distancePerLevel)
                    return (LODLevel)level;
                dist -= distancePerLevel;// *(level + 1);
            }
            return LODLevel.V4;
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

            float dist = (float)Math.Sqrt(distX * distX + distY * distY + distZ * distZ);
            float shadowDistance;
            switch (currentShadowDistanceSetting)
            {
                case ShadowDistance.Closest: shadowDistance = DistClosest; break;
                case ShadowDistance.Close: shadowDistance = DistClose; break;
                case ShadowDistance.Mid: shadowDistance = DistMid; break;
                case ShadowDistance.Far: shadowDistance = DistFar; break;
                default: shadowDistance = DistFurthest; break;
            }

            return dist <= shadowDistance ? ShadowType.Nice : ShadowType.None;
        }

        private void ReloadEntityMesh(IEntity entity, VoxelMeshComponent renderData, LODLevel voxelDetailLevel, ShadowType shadowType)
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
        #endregion
    }
}
