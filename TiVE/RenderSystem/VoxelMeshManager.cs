using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVE.RenderSystem.Lighting;
using ProdigalSoftware.TiVE.RenderSystem.Meshes;
using ProdigalSoftware.TiVE.RenderSystem.Voxels;
using ProdigalSoftware.TiVE.RenderSystem.World;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.RenderSystem
{
    #region VoxelDetailLevelDistance enumeration
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
    #endregion

    /// <summary>
    /// Handles creation of polygon meshes from voxel data and manages the creation/disposal of the native mesh resources.
    /// For speed, mesh building is split between a number of threads so that multiple meshes can be built at one time.
    /// </summary>
    internal sealed class VoxelMeshManager : IDisposable
    {
        #region Constants
        private const int VoxelDetailLevelSections = 3; // 16x16x16 = 4096v, 8x8x8 = 512v, 4x4x4 = 64v, not worth going to 2x2x2 = 8v.
        private const int BestVoxelDetailLevel = 0;
        private const int WorstVoxelDetailLevel = VoxelDetailLevelSections - 1;
        private const int TotalMeshBuilders = 35;
        private const int MaxQueueSize = 3000;
        #endregion

        #region Member variables
        private readonly Dictionary<RenderComponent, MeshBuilder> meshesToInitialize = new Dictionary<RenderComponent, MeshBuilder>();
        private readonly List<KeyValuePair<RenderComponent, MeshBuilder>> meshesToInitializeList = new List<KeyValuePair<RenderComponent, MeshBuilder>>();
        private readonly List<IEntity> entitiesToDelete = new List<IEntity>();
        private readonly HashSet<IEntity> loadedEntities = new HashSet<IEntity>();

        private readonly List<Thread> meshCreationThreads = new List<Thread>();
        private readonly EntityLoadQueue entityLoadQueue = new EntityLoadQueue(MaxQueueSize);
        private readonly List<MeshBuilder> meshBuilders;

        private Scene loadedScene;

        private volatile bool endCreationThreads;
        #endregion

        #region Constructor/Dispose
        public VoxelMeshManager(int maxThreads)
        {
            meshBuilders = new List<MeshBuilder>(TotalMeshBuilders);
            for (int i = 0; i < TotalMeshBuilders; i++)
                meshBuilders.Add(new MeshBuilder(1700000, 3500000));

            for (int i = 0; i < maxThreads; i++)
                meshCreationThreads.Add(StartMeshCreateThread(i + 1));

            TiVEController.UserSettings.SettingChanged += UserSettings_SettingChanged;
        }

        public void Dispose()
        {
            Debug.Assert(Thread.CurrentThread.Name == "Main UI");

            TiVEController.UserSettings.SettingChanged -= UserSettings_SettingChanged;

            endCreationThreads = true;
            foreach (Thread thread in meshCreationThreads)
                thread.Join();
            meshCreationThreads.Clear();

            using (new PerformanceLock(entityLoadQueue))
                entityLoadQueue.Clear();

            foreach (IEntity entity in loadedEntities)
                DeleteEntity(entity);
            loadedEntities.Clear();
        }
        #endregion

        public int ChunkLoadCount
        {
            get { return entityLoadQueue.Size; }
        }

        #region Public methods
        /// <summary>
        /// Performs the necessary cleanup when the current scene has changed
        /// </summary>
        public void ChangeScene(Scene newScene)
        {
            // Clean up threads and data for previous scene
            endCreationThreads = true;
            foreach (Thread thread in meshCreationThreads)
                thread.Join();
            using (new PerformanceLock(entityLoadQueue))
                entityLoadQueue.Clear();
            foreach (IEntity entity in loadedEntities)
                entityLoadQueue.Remove(entity);

            // Start threads for new scene
            endCreationThreads = false;
            for (int i = 0; i < meshCreationThreads.Count; i++)
                meshCreationThreads[i] = StartMeshCreateThread(i);

            loadedScene = newScene;
        }

        /// <summary>
        /// Loads meshes for the specified entities. Meshes for any entities that were loaded that are no longer in the specified list will be deleted.
        /// </summary>
        public void LoadMeshesForEntities(HashSet<IEntity> entitiesToRender, CameraComponent cameraData)
        {
            Debug.Assert(Thread.CurrentThread.Name == "Main UI");
            foreach (IEntity entity in loadedEntities)
            {
                if (!entitiesToRender.Contains(entity))
                {
                    entitiesToDelete.Add(entity);
                    DeleteEntity(entity);
                }
            }

            using (new PerformanceLock(entityLoadQueue))
            {
                for (int i = 0; i < entitiesToDelete.Count; i++)
                {
                    entityLoadQueue.Remove(entitiesToDelete[i]);
                    loadedEntities.Remove(entitiesToDelete[i]);
                }
            }
            entitiesToDelete.Clear();

            VoxelDetailLevelDistance currentVoxelDetalLevelSetting = (VoxelDetailLevelDistance)(int)TiVEController.UserSettings.Get(UserSettings.DetailDistanceKey);
            foreach (IEntity entity in entitiesToRender)
            {
                RenderComponent renderData = entity.GetComponent<RenderComponent>();
                if (renderData == null)
                    continue;

                if (!loadedEntities.Contains(entity))
                    LoadEntity(entity, renderData, WorstVoxelDetailLevel); // Initially load at the worst detail level to quickly get something on screen
                else if (renderData.MeshData != null)
                {
                    int perferedDetailLevel = GetPerferedVoxelDetailLevel(renderData, cameraData, currentVoxelDetalLevelSetting);
                    if (renderData.LoadedVoxelDetailLevel != perferedDetailLevel)
                        LoadEntity(entity, renderData, perferedDetailLevel);
                }
            }

            meshesToInitializeList.Clear();
            using (new PerformanceLock(meshesToInitialize))
            {
                meshesToInitializeList.AddRange(meshesToInitialize);
                meshesToInitialize.Clear();
            }

            foreach (KeyValuePair<RenderComponent, MeshBuilder> meshToInitialize in meshesToInitializeList)
            {
                RenderComponent renderData = meshToInitialize.Key;
                MeshBuilder meshBuilder = meshToInitialize.Value;
                using (new PerformanceLock(renderData.SyncLock))
                {
                    if (renderData.MeshData != null)
                        ((IVertexDataCollection)renderData.MeshData).Dispose();

                    if (renderData.PolygonCount == 0)
                        renderData.MeshData = null;
                    else
                    {
                        renderData.MeshData = meshBuilder.GetMesh();
                        ((IVertexDataCollection)renderData.MeshData).Initialize();
                    }

                    meshBuilder.DropMesh(); // Release builder - Must be called after initializing the mesh
                }
            }
        }

        public void ReloadAllEntities()
        {
            foreach (IEntity chunk in loadedEntities)
                ReloadEntity(chunk, WorstVoxelDetailLevel); // TODO: Reload with the correct detail level
        }
        #endregion

        #region Event handlers
        void UserSettings_SettingChanged(string settingName)
        {
            if (settingName == UserSettings.LightingComplexityKey)
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
                    hasItemToLoad = entityLoadQueue.Size > 0;

                if (!hasItemToLoad)
                {
                    Thread.Sleep(1);
                    continue;
                }

                MeshBuilder meshBuilder;
                using (new PerformanceLock(meshBuilders))
                {
                    meshBuilder = meshBuilders.Find(NotLocked);
                    if (meshBuilder != null)
                        meshBuilder.StartNewMesh(); // Found a mesh builder - grab it quick!
                }

                if (meshBuilder == null)
                {
                    Thread.Sleep(1);
                    continue; // No free meshbuilders to use
                }

                IEntity entity;
                int foundEntityDetailLevel;
                using (new PerformanceLock(entityLoadQueue))
                    entity = entityLoadQueue.Dequeue(out foundEntityDetailLevel);

                if (entity == null)
                {
                    // Couldn't find a entity to load. No need to hold onto the mesh builder.
                    meshBuilder.DropMesh();
                    continue;
                }

                RenderComponent renderData = entity.GetComponent<RenderComponent>();
                Debug.Assert(renderData != null);

                if (!renderData.Visible)
                {
                    // Entity became invisible while waiting to be loaded. No need to hold onto the mesh builder.
                    meshBuilder.DropMesh();
                    continue;
                }

                ChunkComponent chunkData = entity.GetComponent<ChunkComponent>();
                if (chunkData != null)
                    LoadChunkMesh(renderData, chunkData, meshBuilder, foundEntityDetailLevel);
                else
                    LoadMesh(entity, renderData, meshBuilder, foundEntityDetailLevel);
            }
        }

        private static bool NotLocked(MeshBuilder meshBuilder)
        {
            return !meshBuilder.IsLocked;
        }
        #endregion

        #region Methods for loading chunk meshes
        private void LoadChunkMesh(RenderComponent renderData, ChunkComponent chunkData, MeshBuilder meshBuilder, int voxelDetailLevel)
        {
            Scene scene = loadedScene; // For thread safety
            renderData.LoadedVoxelDetailLevel = voxelDetailLevel;

            Debug.Assert(meshBuilder.IsLocked);
            Debug.Assert(loadedScene != null);
            //Debug.WriteLine("Started chunk ({0},{1},{2})", chunkStartX, chunkStartY, chunkStartZ);

            GameWorld gameWorld = scene.GameWorld;
            BlockList blockList = scene.BlockList;

            scene.LightProvider.CacheLightsInBlocksForChunk(chunkData);

            int blockStartX = chunkData.ChunkBlockLoc.X;
            int blockEndX = Math.Min((chunkData.ChunkLoc.X + 1) * ChunkComponent.BlockSize, gameWorld.BlockSize.X);
            int blockStartY = chunkData.ChunkBlockLoc.Y;
            int blockEndY = Math.Min((chunkData.ChunkLoc.Y + 1) * ChunkComponent.BlockSize, gameWorld.BlockSize.Y);
            int blockStartZ = chunkData.ChunkBlockLoc.Z;
            int blockEndZ = Math.Min((chunkData.ChunkLoc.Z + 1) * ChunkComponent.BlockSize, gameWorld.BlockSize.Z);

            int voxelStartX = blockStartX * Block.VoxelSize;
            int voxelStartY = blockStartY * Block.VoxelSize;
            int voxelStartZ = blockStartZ * Block.VoxelSize;

            int voxelCount = 0;
            int renderedVoxelCount = 0;
            int polygonCount = 0;
            for (int blockZ = blockEndZ - 1; blockZ >= blockStartZ; blockZ--)
            {
                if (!renderData.Visible)
                    break;

                for (int blockX = blockStartX; blockX < blockEndX; blockX++)
                {
                    for (int blockY = blockStartY; blockY < blockEndY; blockY++)
                    {
                        ushort blockIndex = gameWorld[blockX, blockY, blockZ];
                        if (blockIndex == 0)
                            continue;

                        BlockImpl block = (BlockImpl)blockList[blockIndex];
                        //if (block.TotalVoxels == 0)
                        //    continue;

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

            using (new PerformanceLock(meshesToInitialize))
            using (new PerformanceLock(renderData.SyncLock))
            {
                MeshBuilder existingMeshBuilder;
                if (meshesToInitialize.TryGetValue(renderData, out existingMeshBuilder))
                {
                    // Must have been loading this chunk while waiting to initialize a previous version of this chunk
                    existingMeshBuilder.DropMesh();
                    meshesToInitialize.Remove(renderData);
                }

                if (!renderData.Visible)
                {
                    // Entity became invisible while loading so just release the lock on the mesh builder since we're done with it.
                    meshBuilder.DropMesh();
                }
                else
                {
                    meshesToInitialize.Add(renderData, meshBuilder); // Make sure we don't add it again due to a lock timing issue
                    renderData.PolygonCount = polygonCount;
                    renderData.VoxelCount = voxelCount;
                    renderData.RenderedVoxelCount = renderedVoxelCount;
                }
            }
        }

        private static void CreateMeshForBlockInChunk(BlockImpl block, int blockX, int blockY, int blockZ,
            int voxelStartX, int voxelStartY, int voxelStartZ, int voxelDetailLevel, Scene scene,
            MeshBuilder meshBuilder, ref int polygonCount, ref int renderedVoxelCount)
        {
            GameWorld gameWorld = scene.GameWorld;
            LightProvider lightProvider = scene.LightProvider;
            VoxelMeshHelper meshHelper = VoxelMeshHelper.Get(false);

            int voxelSize = 1 << voxelDetailLevel;
            int maxVoxelX = gameWorld.VoxelSize.X - 1 - voxelSize;
            int maxVoxelY = gameWorld.VoxelSize.Y - 1 - voxelSize;
            int maxVoxelZ = gameWorld.VoxelSize.Z - 1 - voxelSize;
            int maxBlockVoxelSize = Block.VoxelSize - voxelSize;

            bool blockIsLit = !block.HasComponent(UnlitComponent.Instance);
            //for (int bz = BlockInformation.VoxelSize - 1; bz >= 0; bz -= voxelSize)
            for (int bvz = 0; bvz < Block.VoxelSize; bvz += voxelSize)
            {
                int voxelZ = blockZ * Block.VoxelSize + bvz;
                byte chunkVoxelZ = (byte)(voxelZ - voxelStartZ);
                for (int bvx = 0; bvx < Block.VoxelSize; bvx += voxelSize)
                {
                    int voxelX = blockX * Block.VoxelSize + bvx;
                    byte chunkVoxelX = (byte)(voxelX - voxelStartX);
                    for (int bvy = 0; bvy < Block.VoxelSize; bvy += voxelSize)
                    {
                        uint color = voxelSize == 1 ? block.GetVoxelFast(bvx, bvy, bvz) : GetLODVoxelColor(block, bvz, bvx, bvy, voxelSize);
                        if (color == 0)
                            continue;

                        int voxelY = blockY * Block.VoxelSize + bvy;

                        VoxelSides sides = VoxelSides.None;

                        // Check to see if the back side is visible
                        if (bvz >= voxelSize)
                        {
                            if (block.GetVoxelFast(bvx, bvy, bvz - voxelSize) == 0)
                                sides |= VoxelSides.Back;
                        }
                        else if (voxelZ >= voxelSize && gameWorld.GetVoxel(voxelX, voxelY, voxelZ - voxelSize) == 0)
                            sides |= VoxelSides.Back;

                        // Check to see if the front side is visible
                        if (bvz < maxBlockVoxelSize)
                        {
                            if (block.GetVoxelFast(bvx, bvy, bvz + voxelSize) == 0)
                                sides |= VoxelSides.Front;
                        }
                        else if (voxelZ <= maxVoxelZ && gameWorld.GetVoxel(voxelX, voxelY, voxelZ + voxelSize) == 0)
                            sides |= VoxelSides.Front;

                        // Check to see if the left side is visible
                        if (bvx >= voxelSize)
                        {
                            if (block.GetVoxelFast(bvx - voxelSize, bvy, bvz) == 0)
                                sides |= VoxelSides.Left;
                        }
                        else if (voxelX >= voxelSize && gameWorld.GetVoxel(voxelX - voxelSize, voxelY, voxelZ) == 0)
                            sides |= VoxelSides.Left;

                        // Check to see if the right side is visible
                        if (bvx < maxBlockVoxelSize)
                        {
                            if (block.GetVoxelFast(bvx + voxelSize, bvy, bvz) == 0)
                                sides |= VoxelSides.Right;
                        }
                        else if (voxelX <= maxVoxelX && gameWorld.GetVoxel(voxelX + voxelSize, voxelY, voxelZ) == 0)
                            sides |= VoxelSides.Right;

                        // Check to see if the bottom side is visible
                        if (bvy >= voxelSize)
                        {
                            if (block.GetVoxelFast(bvx, bvy - voxelSize, bvz) == 0)
                                sides |= VoxelSides.Bottom;
                        }
                        else if (voxelY >= voxelSize && gameWorld.GetVoxel(voxelX, voxelY - voxelSize, voxelZ) == 0)
                            sides |= VoxelSides.Bottom;

                        // Check to see if the top side is visible
                        if (bvy < maxBlockVoxelSize)
                        {
                            if (block.GetVoxelFast(bvx, bvy + voxelSize, bvz) == 0)
                                sides |= VoxelSides.Top;
                        }
                        else if (voxelY <= maxVoxelY && gameWorld.GetVoxel(voxelX, voxelY + voxelSize, voxelZ) == 0)
                            sides |= VoxelSides.Top;

                        if (sides != VoxelSides.None)
                        {
                            Color3f lightColor = blockIsLit ?
                                lightProvider.GetLightAt(voxelX, voxelY, voxelZ, voxelSize, blockX, blockY, blockZ, sides) : Color3f.White;
                            byte a = (byte)((color >> 24) & 0xFF);
                            byte r = (byte)Math.Min(255, (int)(((color >> 16) & 0xFF) * lightColor.R));
                            byte g = (byte)Math.Min(255, (int)(((color >> 8) & 0xFF) * lightColor.G));
                            byte b = (byte)Math.Min(255, (int)(((color >> 0) & 0xFF) * lightColor.B));

                            polygonCount += meshHelper.AddVoxel(meshBuilder, sides,
                                chunkVoxelX, (byte)(voxelY - voxelStartY), chunkVoxelZ, new Color4b(r, g, b, a), voxelSize);
                            renderedVoxelCount++;
                        }
                    }
                }
            }
        }

        private static uint GetLODVoxelColor(BlockImpl block, int bvz, int bvx, int bvy, int voxelSize)
        {
            uint color;
            int voxelsFound = 0;
            int totalA = 0;
            int totalR = 0;
            int totalG = 0;
            int totalB = 0;
            int maxX = bvx + voxelSize;
            int maxY = bvy + voxelSize;
            int maxZ = bvz + voxelSize;
            //for (int z = bvz; z > bvz - voxelSize; z--)
            for (int z = bvz; z < maxZ; z++)
            {
                for (int x = bvx; x < maxX; x++)
                {
                    for (int y = bvy; y < maxY; y++)
                    {
                        uint otherColor = block.GetVoxelFast(x, y, z);
                        if (otherColor == 0)
                            continue;

                        voxelsFound++;
                        totalA += (int)((otherColor >> 24) & 0xFF);
                        totalR += (int)((otherColor >> 16) & 0xFF);
                        totalG += (int)((otherColor >> 8) & 0xFF);
                        totalB += (int)((otherColor >> 0) & 0xFF);
                    }
                }
            }

            if (voxelsFound == 0) // Prevent divide-by-zero
                color = 0;
            else
            {
                color = (uint)(((totalA / voxelsFound) & 0xFF) << 24 |
                    ((totalR / voxelsFound) & 0xFF) << 16 |
                    ((totalG / voxelsFound) & 0xFF) << 8 |
                    ((totalB / voxelsFound) & 0xFF));
            }
            return color;
        }
        #endregion

        #region Methods for loading entity meshes
        private void LoadMesh(IEntity entity, RenderComponent renderData, MeshBuilder meshBuilder, int voxelDetailLevel)
        {
        }
        #endregion

        #region Other private helper methods
        /// <summary>
        /// Deletes the mesh data and cached render information for the specified entity. This method does nothing if the specified entitiy has
        /// not had a mesh created yet.
        /// </summary>
        private void DeleteEntity(IEntity entity)
        {
            RenderComponent renderData = entity.GetComponent<RenderComponent>();
            using (new PerformanceLock(renderData.SyncLock))
            {
                using (new PerformanceLock(meshesToInitialize))
                {
                    MeshBuilder meshBuilder;
                    meshesToInitialize.TryGetValue(renderData, out meshBuilder);
                    if (meshBuilder != null)
                        meshBuilder.DropMesh();
                    meshesToInitialize.Remove(renderData);
                }

                if (renderData.MeshData != null)
                    ((IVertexDataCollection)renderData.MeshData).Dispose();

                renderData.MeshData = null;
                renderData.Visible = false;
                renderData.LoadedVoxelDetailLevel = -1;
                renderData.PolygonCount = 0;
                renderData.VoxelCount = 0;
                renderData.RenderedVoxelCount = 0;
            }

            ChunkComponent chunkData = entity.GetComponent<ChunkComponent>();
            if (chunkData != null)
                loadedScene.LightProvider.RemoveLightsForChunk(chunkData);
        }

        /// <summary>
        /// Given the specified render data and camera data determine the wanted voxel detail level given the specified user detail setting
        /// </summary>
        private static int GetPerferedVoxelDetailLevel(RenderComponent renderData, CameraComponent cameraData, VoxelDetailLevelDistance currentVoxelDetalLevelSetting)
        {
            Vector3f entityLoc = renderData.Location;
            Vector3f cameraLoc = cameraData.Location;
            float distX = entityLoc.X - cameraLoc.X;
            float distY = entityLoc.Y - cameraLoc.Y;
            float distZ = entityLoc.Z - cameraLoc.Z;

            int dist = (int)Math.Sqrt(distX * distX + distY * distY + distZ * distZ);
            int distancePerLevel;
            switch (currentVoxelDetalLevelSetting)
            {
                case VoxelDetailLevelDistance.Closest: distancePerLevel = 350; break;
                case VoxelDetailLevelDistance.Close: distancePerLevel = 500; break;
                case VoxelDetailLevelDistance.Mid: distancePerLevel = 750; break;
                case VoxelDetailLevelDistance.Far: distancePerLevel = 1000; break;
                default: distancePerLevel = 1500; break;
            }

            for (int i = BestVoxelDetailLevel; i <= WorstVoxelDetailLevel; i++)
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
        private void LoadEntity(IEntity entity, RenderComponent renderData, int voxelDetailLevel)
        {
            using (new PerformanceLock(renderData.SyncLock))
                renderData.Visible = true;
            loadedEntities.Add(entity);
            ReloadEntity(entity, voxelDetailLevel);
        }

        private void ReloadEntity(IEntity entity, int voxelDetailLevel)
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
        #endregion
    }
}
