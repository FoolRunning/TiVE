//#define USE_INSTANCED_RENDERING
#define USE_INDEXED_RENDERING
using System;
using System.Collections.Generic;
using System.Threading;
using ProdigalSoftware.TiVE.Renderer.Meshes;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    internal sealed class ChunkCache : IDisposable
    {
        private const int ChunkCacheSize = 2000 / Chunk.TileSize;
        /// <summary>
        /// Distance in world tiles (outside the viewable area) to start removing loaded chunks
        /// </summary>
        private const int ChunkUnloadDistance = 2 * Chunk.TileSize;

        private readonly GameWorld gameWorld;
        private readonly BlockList blockList;

        private readonly List<Tuple<int, Chunk>> chunksToDelete = new List<Tuple<int, Chunk>>();
        private readonly Dictionary<int, Chunk> chunks = new Dictionary<int, Chunk>(1200);
        private readonly Queue<Chunk.ChunkLoadInfo> chunkLoadQueue = new Queue<Chunk.ChunkLoadInfo>();

        public ChunkCache(GameWorld gameWorld, BlockList blockList)
        {
            this.gameWorld = gameWorld;
            this.blockList = blockList;

            StartChunkCreateThread();
            //StartChunkCreateThread();
        }

        public void Dispose()
        {
            foreach (Chunk chunk in chunks.Values)
                chunk.Dispose();

            chunks.Clear();
        }

        public Chunk GetOrCreateChunk(int chunkX, int chunkY, int chunkZ)
        {
            int key = GetChunkKey(chunkX, chunkY, chunkZ);
            Chunk chunk;
            if (!chunks.TryGetValue(key, out chunk))
                chunks[key] = chunk = CreateChunk(chunkX, chunkY, chunkZ);
            return chunk;
        }

        public void InitializeChunks()
        {
            foreach (Chunk chunk in chunks.Values)
            {
                //if (!chunk.IsInitialized)
                    chunk.Initialize();
            }
        }

        public void CleanupChunksOutside(int startX, int startY, int endX, int endY)
        {
            foreach (KeyValuePair<int, Chunk> chunkInfo in chunks)
            {
                if (chunks.Count - chunksToDelete.Count < ChunkCacheSize)
                    break;

                if (!chunkInfo.Value.IsInside(startX - ChunkUnloadDistance, startY - ChunkUnloadDistance, endX + ChunkUnloadDistance, endY + ChunkUnloadDistance))
                    chunksToDelete.Add(new Tuple<int, Chunk>(chunkInfo.Key, chunkInfo.Value));
            }

            for (int i = 0; i < chunksToDelete.Count; i++)
            {
                Tuple<int, Chunk> chunkInfo = chunksToDelete[i];
                chunks.Remove(chunkInfo.Item1);
                chunkInfo.Item2.Dispose();
            }

            chunksToDelete.Clear();
        }

        private Chunk CreateChunk(int chunkX, int chunkY, int chunkZ)
        {
            Chunk chunk = new Chunk(chunkX * Chunk.TileSize, chunkY * Chunk.TileSize, chunkZ * Chunk.TileSize);
            Chunk.ChunkLoadInfo info = new Chunk.ChunkLoadInfo(chunk, gameWorld, blockList);
            lock(chunkLoadQueue)
                chunkLoadQueue.Enqueue(info);
            return chunk;
        }

        private static int GetChunkKey(int chunkX, int chunkY, int chunkZ)
        {
            return chunkX + (chunkY << 8) + (chunkZ << 16);
        }

        private void StartChunkCreateThread()
        {
            Thread chunkCreationThread = new Thread(() =>
            {
                List<MeshBuilder> meshBuilders = new List<MeshBuilder>();
                while (true)
                {
                    int count;
                    lock (chunkLoadQueue)
                        count = chunkLoadQueue.Count;

                    if (count == 0)
                    {
                        Thread.Sleep(2);
                        continue;
                    }

                    Chunk.ChunkLoadInfo chunkInfo;
                    lock (chunkLoadQueue)
                    {
                        if (chunkLoadQueue.Count == 0)
                            continue; // Check for race condition with multiple threads accessing the queue

                        chunkInfo = chunkLoadQueue.Dequeue();
                    }
                    MeshBuilder meshBuilder = meshBuilders.Find(mb => !mb.IsLocked);
                    if (meshBuilder == null)
                    {
                        meshBuilder = new MeshBuilder(200000, 400000);
                        meshBuilders.Add(meshBuilder);
                    }
                    if (!chunkInfo.Chunk.IsDeleted)
                        chunkInfo.Load(meshBuilder);
                }
            });

            chunkCreationThread.IsBackground = true;
            chunkCreationThread.Name = "ChunkLoad";
            chunkCreationThread.Start();
        }
    }
}
