using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ProdigalSoftware.TiVE.Renderer.Meshes;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    internal sealed class WorldChunkManager : IDisposable
    {
        private const int MaxChunkUpdatesPerFrame = 7;
        private const int MeshBuildersPerThread = 5;

        private readonly List<GameWorldVoxelChunk> chunksToDelete = new List<GameWorldVoxelChunk>();
        private readonly HashSet<GameWorldVoxelChunk> loadedChunks = new HashSet<GameWorldVoxelChunk>();

        private readonly List<Thread> chunkCreationThreads = new List<Thread>();
        private readonly Queue<GameWorldVoxelChunk> chunkLoadQueue = new Queue<GameWorldVoxelChunk>();

        private readonly IGameWorldRenderer renderer;
        
        private volatile bool endCreationThreads;

        public WorldChunkManager(IGameWorldRenderer renderer, int maxThreads)
        {
            this.renderer = renderer;

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

        public void Update(HashSet<GameWorldVoxelChunk> chunksToRender)
        {
            Debug.Assert(Thread.CurrentThread.Name == "FrameUpdate");

            foreach (GameWorldVoxelChunk chunk in chunksToRender)
            {
                if (!loadedChunks.Contains(chunk))
                    LoadChunk(chunk);
            }

            foreach (GameWorldVoxelChunk chunk in loadedChunks)
            {
                if (!chunksToRender.Contains(chunk))
                    chunksToDelete.Add(chunk);
            }
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
                ReloadChunk(chunk);
        }

        private void LoadChunk(GameWorldVoxelChunk chunk)
        {
            chunk.PrepareForLoad();
            ReloadChunk(chunk);
            loadedChunks.Add(chunk);
        }

        private void ReloadChunk(GameWorldVoxelChunk chunk)
        {
            if (chunk == null)
                throw new ArgumentNullException("chunk");

            using (new PerformanceLock(chunkLoadQueue))
            {
                if (!chunkLoadQueue.Contains(chunk))
                    chunkLoadQueue.Enqueue(chunk);
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
                meshBuilders.Add(new MeshBuilder(4000000, 4000000));

            int bottleneckCount = 0;
            while (!endCreationThreads)
            {
                Thread.Sleep(3);

                int count;
                using (new PerformanceLock(chunkLoadQueue))
                    count = chunkLoadQueue.Count;

                if (count == 0)
                    continue;

                MeshBuilder meshBuilder = meshBuilders.Find(NotLocked);
                if (meshBuilder == null)
                {
                    bottleneckCount++;
                    if (bottleneckCount > 50) // 150ms give or take
                    {
                        bottleneckCount = 0;
                        // Too many chunks are waiting to be intialized. Still not sure how this can happen.
                        Messages.AddWarning("Mesh creation bottlenecked!");
                    }

                    continue; // No free meshbuilders to use
                }

                bottleneckCount = 0;

                GameWorldVoxelChunk chunk;
                using (new PerformanceLock(chunkLoadQueue))
                {
                    if (chunkLoadQueue.Count == 0)
                        continue; // Check for race condition with multiple threads accessing the queue

                    chunk = chunkLoadQueue.Dequeue();
                    if (chunk.IsDeleted)
                        continue; // Chunk got deleted while waiting to be loaded
                }

                meshBuilder.StartNewMesh();
                chunk.Load(meshBuilder, renderer);
            }
        }

        private static bool NotLocked(MeshBuilder meshBuilder)
        {
            return !meshBuilder.IsLocked;
        }
    }
}
