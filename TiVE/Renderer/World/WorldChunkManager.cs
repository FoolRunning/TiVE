using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ProdigalSoftware.TiVE.Renderer.Meshes;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    internal enum VoxelDetailLevelDistance
    {
        Closest = 0,
        Close = 1,
        Mid = 2,
        Far = 3,
        Furthest = 4
    }

    internal sealed class WorldChunkManager : IDisposable
    {
        private const int VoxelDetailLevelSections = 4;
        private const int BestVoxelDetailLevel = 0;
        private const int WorstVoxelDetailLevel = VoxelDetailLevelSections - 1;
        private const int TotalMeshBuilders = 10;

        private readonly List<GameWorldVoxelChunk> chunksToDelete = new List<GameWorldVoxelChunk>();
        private readonly HashSet<GameWorldVoxelChunk> loadedChunks = new HashSet<GameWorldVoxelChunk>();

        private readonly List<Thread> chunkCreationThreads = new List<Thread>();
        private readonly List<Queue<GameWorldVoxelChunk>> chunkLoadQueue;
        private readonly List<MeshBuilder> meshBuilders;

        private readonly IGameWorldRenderer renderer;
        
        private volatile bool endCreationThreads;

        public WorldChunkManager(IGameWorldRenderer renderer, int maxThreads)
        {
            this.renderer = renderer;

            chunkLoadQueue = new List<Queue<GameWorldVoxelChunk>>(VoxelDetailLevelSections); 
            for (int i = BestVoxelDetailLevel; i <= WorstVoxelDetailLevel; i++)
                chunkLoadQueue.Add(new Queue<GameWorldVoxelChunk>(300));

            meshBuilders = new List<MeshBuilder>(TotalMeshBuilders);
            for (int i = 0; i < TotalMeshBuilders; i++)
                meshBuilders.Add(new MeshBuilder(1500000, 2000000));

            for (int i = 0; i < maxThreads; i++)
                chunkCreationThreads.Add(StartChunkCreateThread(i + 1));
        }

        public void Dispose()
        {
            Debug.Assert(Thread.CurrentThread.Name == "Main UI");

            endCreationThreads = true;
            foreach (Thread thread in chunkCreationThreads)
                thread.Join();
            chunkCreationThreads.Clear();

            using (new PerformanceLock(chunkLoadQueue))
                chunkLoadQueue.Clear();

            foreach (GameWorldVoxelChunk chunk in loadedChunks)
                chunk.Dispose();
            loadedChunks.Clear();
        }

        public void Update(HashSet<GameWorldVoxelChunk> chunksToRender, Camera camera)
        {
            Debug.Assert(Thread.CurrentThread.Name == "Main UI");

            foreach (GameWorldVoxelChunk chunk in loadedChunks)
            {
                if (!chunksToRender.Contains(chunk))
                    chunksToDelete.Add(chunk);
            }

            VoxelDetailLevelDistance currentVoxelDetalLevelSetting = (VoxelDetailLevelDistance)(int)TiVEController.UserSettings.Get(UserSettings.DetailDistanceKey);
            foreach (GameWorldVoxelChunk chunk in chunksToRender)
            {
                if (!loadedChunks.Contains(chunk))
                    LoadChunk(chunk, WorstVoxelDetailLevel); // Initially load at the worst detail level
                else if (chunk.IsLoaded)
                {
                    int perferedDetailLevel = GetPerferedVoxelDetailLevel(chunk, camera, currentVoxelDetalLevelSetting);
                    if (chunk.LoadedVoxelDetailLevel != perferedDetailLevel)
                        LoadChunk(chunk, perferedDetailLevel);
                }
            }
        }
        
        private static int GetPerferedVoxelDetailLevel(GameWorldVoxelChunk chunk, Camera camera, VoxelDetailLevelDistance currentVoxelDetalLevelSetting)
        {
            Vector3i chunkLoc = chunk.ChunkVoxelLocation;
            Vector3i cameraLoc = new Vector3i((int)camera.Location.X, (int)camera.Location.Y, (int)camera.Location.Z);
            int distX = chunkLoc.X - cameraLoc.X;
            int distY = chunkLoc.Y - cameraLoc.Y;
            int distZ = chunkLoc.Z - cameraLoc.Z;

            int dist = (int)Math.Sqrt(distX * distX + distY * distY + distZ * distZ);
            int distancePerLevel;
            switch (currentVoxelDetalLevelSetting)
            {
                case VoxelDetailLevelDistance.Closest: distancePerLevel = 150; break;
                case VoxelDetailLevelDistance.Close: distancePerLevel = 250; break;
                case VoxelDetailLevelDistance.Mid: distancePerLevel = 400; break;
                case VoxelDetailLevelDistance.Far: distancePerLevel = 500; break;
                default: distancePerLevel = 600; break;
            }

            for (int i = BestVoxelDetailLevel; i <= WorstVoxelDetailLevel; i++)
            {
                if (dist <= distancePerLevel)
                    return i;
                dist -= distancePerLevel * (i + 1);
            }
            return WorstVoxelDetailLevel;
        }

        public void CleanUpChunks()
        {
            Debug.Assert(Thread.CurrentThread.Name == "Main UI");

            foreach (GameWorldVoxelChunk chunk in chunksToDelete)
            {
                loadedChunks.Remove(chunk);
                chunk.Dispose();
            }

            chunksToDelete.Clear();
        }

        public void ReloadAllChunks()
        {
            foreach (GameWorldVoxelChunk chunk in loadedChunks)
                ReloadChunk(chunk, WorstVoxelDetailLevel); // TODO: Reload with the correct detail level
        }

        private void LoadChunk(GameWorldVoxelChunk chunk, int voxelDetailLevel)
        {
            chunk.PrepareForLoad();
            ReloadChunk(chunk, voxelDetailLevel);
            loadedChunks.Add(chunk);
        }

        private void ReloadChunk(GameWorldVoxelChunk chunk, int voxelDetailLevel)
        {
            if (chunk == null)
                throw new ArgumentNullException("chunk");

            using (new PerformanceLock(chunkLoadQueue))
            {
                Queue<GameWorldVoxelChunk> queue = chunkLoadQueue[voxelDetailLevel];
                if (!queue.Contains(chunk))
                    queue.Enqueue(chunk);
            }
        }

        private Thread StartChunkCreateThread(int num)
        {
            Thread thread = new Thread(ChunkCreateLoop);
            thread.IsBackground = true;
            thread.Name = "ChunkLoad" + num;
            thread.Start();
            return thread;
        }

        private void ChunkCreateLoop()
        {
            while (!endCreationThreads)
            {
                bool hasItemToLoad;
                using (new PerformanceLock(chunkLoadQueue))
                    hasItemToLoad = chunkLoadQueue.SelectMany(q => q).Any();

                if (!hasItemToLoad)
                {
                    Thread.Sleep(5);
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

                GameWorldVoxelChunk chunk = null;
                int foundChunkDetailLevel = WorstVoxelDetailLevel;
                using (new PerformanceLock(chunkLoadQueue))
                {
                    for (int i = chunkLoadQueue.Count - 1; i >= 0; i--)
                    {
                        if (chunkLoadQueue[i].Count > 0)
                        {
                            chunk = chunkLoadQueue[i].Dequeue();
                            foundChunkDetailLevel = i;
                            break;
                        }
                    }
                }

                if (chunk == null || chunk.IsDeleted)
                {
                    // Couldn't find a chunk to load or chunk got deleted while waiting to be loaded. No need to hold onto the mesh builder.
                    meshBuilder.DropMesh();
                    continue; 
                }

                chunk.Load(meshBuilder, renderer, foundChunkDetailLevel);
            }
        }

        private static bool NotLocked(MeshBuilder meshBuilder)
        {
            return !meshBuilder.IsLocked;
        }
    }
}
