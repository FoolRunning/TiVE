using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ProdigalSoftware.TiVE.Renderer.Meshes;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    internal enum VoxelDetailLevelDistance
    {
        Closest,
        Close,
        Mid,
        Far,
        Furthest
    }

    internal sealed class WorldChunkManager : IDisposable
    {
        private const int VoxelDetailLevelSections = 3;
        private const int BestVoxelDetailLevel = 0;
        private const int WorstVoxelDetailLevel = VoxelDetailLevelSections - 1;
        private const int MeshBuildersPerThread = 3;

        private readonly List<GameWorldVoxelChunk> chunksToDelete = new List<GameWorldVoxelChunk>();
        private readonly HashSet<GameWorldVoxelChunk> loadedChunks = new HashSet<GameWorldVoxelChunk>();

        private readonly List<Thread> chunkCreationThreads = new List<Thread>();
        private readonly List<Queue<GameWorldVoxelChunk>> chunkLoadQueue = new List<Queue<GameWorldVoxelChunk>>(4);

        private readonly IGameWorldRenderer renderer;
        
        private volatile bool endCreationThreads;

        public WorldChunkManager(IGameWorldRenderer renderer, int maxThreads)
        {
            this.renderer = renderer;

            for (int i = BestVoxelDetailLevel; i <= WorstVoxelDetailLevel; i++)
                chunkLoadQueue.Add(new Queue<GameWorldVoxelChunk>(500));

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

            int distSquared = distX * distX + distY * distY + distZ * distZ;
            int distancePerLevel;
            switch (currentVoxelDetalLevelSetting)
            {
                case VoxelDetailLevelDistance.Closest: distancePerLevel = 300 * 300; break;
                case VoxelDetailLevelDistance.Close: distancePerLevel = 500 * 500; break;
                case VoxelDetailLevelDistance.Mid: distancePerLevel = 650 * 650; break;
                case VoxelDetailLevelDistance.Far: distancePerLevel = 850 * 850; break;
                default: distancePerLevel = 1000 * 1000; break;
            }

            for (int i = BestVoxelDetailLevel; i <= WorstVoxelDetailLevel; i++)
            {
                if (distSquared <= distancePerLevel)
                    return i;
                distSquared -= distancePerLevel;
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
            List<MeshBuilder> meshBuilders = new List<MeshBuilder>();
            for (int i = 0; i < MeshBuildersPerThread; i++)
                meshBuilders.Add(new MeshBuilder(1500000, 2500000));

            int bottleneckCount = 0;
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

                MeshBuilder meshBuilder = meshBuilders.Find(NotLocked);
                if (meshBuilder == null)
                {
                    bottleneckCount++;
                    if (bottleneckCount > 200) // 200ms give or take
                    {
                        bottleneckCount = 0;
                        // Too many chunks are waiting to be intialized. Still not sure how this can happen.
                        Messages.AddWarning("Mesh creation bottlenecked!");
                    }

                    //Console.WriteLine("Mesh creation slowed!");
                    Thread.Sleep(1);
                    continue; // No free meshbuilders to use
                }

                bottleneckCount = 0;

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
                    continue; // Couldn't find a chunk to load or chunk got deleted while waiting to be loaded

                meshBuilder.StartNewMesh();
                chunk.Load(meshBuilder, renderer, foundChunkDetailLevel);
            }
        }

        private static bool NotLocked(MeshBuilder meshBuilder)
        {
            return !meshBuilder.IsLocked;
        }
    }
}
