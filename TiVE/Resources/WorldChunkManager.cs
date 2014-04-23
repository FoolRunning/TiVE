using System;
using System.Collections.Generic;
using System.Threading;
using ProdigalSoftware.TiVE.Renderer.Voxels;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Resources
{
    internal sealed class WorldChunkManager : IDisposable
    {
        /// <summary>
        /// Distance in world tiles (outside the viewable area) to start removing loaded chunks
        /// </summary>
        private const int ChunkUnloadDistance = 1 * GameWorldVoxelChunk.TileSize;

        private readonly List<Tuple<int, GameWorldVoxelChunk>> chunksToDelete = new List<Tuple<int, GameWorldVoxelChunk>>();
        private readonly Dictionary<int, GameWorldVoxelChunk> chunks = new Dictionary<int, GameWorldVoxelChunk>(1200);
        private readonly Queue<ChunkLoadInfo> chunkLoadQueue = new Queue<ChunkLoadInfo>();
        private readonly List<Thread> chunkCreationThreads = new List<Thread>();
        private volatile bool endCreationThreads;

        public void Dispose()
        {
            endCreationThreads = true;
            foreach (Thread thread in chunkCreationThreads)
                thread.Join();
            chunkCreationThreads.Clear();

            using (new PerformanceLock(chunkLoadQueue))
                chunkLoadQueue.Clear();

            foreach (GameWorldVoxelChunk chunk in chunks.Values)
                chunk.Dispose();
            chunks.Clear();
        }

        public bool Initialize()
        {
            Messages.Print("Starting chunk creations threads...");

            endCreationThreads = false;
            int threadCount = Environment.ProcessorCount > 2 ? 2 : 1;
            for (int i = 0; i < threadCount; i++)
                chunkCreationThreads.Add(StartChunkCreateThread());

            Messages.AddDoneText();
            return true;
        }

        public GameWorldVoxelChunk GetOrCreateChunk(int chunkX, int chunkY, int chunkZ)
        {
            int key = GetChunkKey(chunkX, chunkY, chunkZ);
            GameWorldVoxelChunk chunk;
            if (!chunks.TryGetValue(key, out chunk))
                chunks[key] = chunk = CreateChunk(chunkX, chunkY, chunkZ);
            return chunk;
        }

        public void InitializeChunks()
        {
            foreach (GameWorldVoxelChunk chunk in chunks.Values)
            {
                //if (!chunk.IsInitialized)
                    chunk.Initialize();
            }
        }

        public void CleanupChunksOutside(int startX, int startY, int endX, int endY)
        {
            foreach (KeyValuePair<int, GameWorldVoxelChunk> chunkInfo in chunks)
            {
                if (!chunkInfo.Value.IsInside(startX - ChunkUnloadDistance, startY - ChunkUnloadDistance, endX + ChunkUnloadDistance, endY + ChunkUnloadDistance))
                    chunksToDelete.Add(new Tuple<int, GameWorldVoxelChunk>(chunkInfo.Key, chunkInfo.Value));
            }

            for (int i = 0; i < chunksToDelete.Count; i++)
            {
                Tuple<int, GameWorldVoxelChunk> chunkInfo = chunksToDelete[i];
                chunks.Remove(chunkInfo.Item1);
                chunkInfo.Item2.Dispose();
            }

            chunksToDelete.Clear();
        }

        private GameWorldVoxelChunk CreateChunk(int chunkX, int chunkY, int chunkZ)
        {
            GameWorldVoxelChunk chunk = new GameWorldVoxelChunk(chunkX * GameWorldVoxelChunk.TileSize, chunkY * GameWorldVoxelChunk.TileSize, chunkZ * GameWorldVoxelChunk.TileSize);
            ChunkLoadInfo info = new ChunkLoadInfo(chunk);
            lock(chunkLoadQueue)
                chunkLoadQueue.Enqueue(info);
            return chunk;
        }

        private static int GetChunkKey(int chunkX, int chunkY, int chunkZ)
        {
            return chunkX + (chunkY << 8) + (chunkZ << 16);
        }

        private Thread StartChunkCreateThread()
        {
            Thread thread = new Thread(ChunkCreateLoop);
            thread.IsBackground = true;
            thread.Name = "ChunkLoad";
            thread.Start();
            return thread;
        }

        private void ChunkCreateLoop()
        {
            List<MeshBuilder> meshBuilders = new List<MeshBuilder>();
            while (!endCreationThreads)
            {
                int count;
                using (new PerformanceLock(chunkLoadQueue))
                    count = chunkLoadQueue.Count;

                if (count == 0)
                {
                    Thread.Sleep(2);
                    continue;
                }

                MeshBuilder meshBuilder = meshBuilders.Find(NotLocked);
                if (meshBuilder == null && meshBuilders.Count < 5)
                {
                    meshBuilder = new MeshBuilder(200000, 400000);
                    meshBuilders.Add(meshBuilder);
                }

                ChunkLoadInfo chunkInfo;
                using (new PerformanceLock(chunkLoadQueue))
                {
                    if (meshBuilder == null || chunkLoadQueue.Count == 0)
                        continue; // Check for race condition with multiple threads accessing the queue

                    chunkInfo = chunkLoadQueue.Dequeue();
                }

                if (!chunkInfo.Chunk.IsDeleted)
                    chunkInfo.Load(meshBuilder);
            }
        }

        private static bool NotLocked(MeshBuilder meshBuilder)
        {
            return !meshBuilder.IsLocked;
        }
    }
}
