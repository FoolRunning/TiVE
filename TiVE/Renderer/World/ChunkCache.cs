using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    internal sealed class ChunkCache
    {
        private readonly GameWorld gameWorld;
        private readonly BlockList blockList;

        private readonly Dictionary<int, Chunk> chunks = new Dictionary<int, Chunk>(300);

        public ChunkCache(GameWorld gameWorld, BlockList blockList)
        {
            this.gameWorld = gameWorld;
            this.blockList = blockList;
        }

        public void CleanUp()
        {
            lock (chunks)
            {
                foreach (Chunk chunk in chunks.Values)
                    chunk.Delete();

                chunks.Clear();
            }
        }

        public Chunk GetOrCreateChunk(int chunkX, int chunkY, int chunkZ)
        {
            int key = GetChunkKey(chunkX, chunkY, chunkZ);
            Chunk chunk;
            lock (chunks)
            {
                if (!chunks.TryGetValue(key, out chunk))
                    chunks[key] = chunk = CreateChunk(chunkX, chunkY, chunkZ);
            }
            return chunk;
        }

        private Chunk CreateChunk(int chunkX, int chunkY, int chunkZ)
        {
            Chunk chunk = new Chunk();
            Chunk.ChunkLoadInfo info = new Chunk.ChunkLoadInfo(gameWorld, blockList, chunkX * Chunk.TileSize, 
                chunkY * Chunk.TileSize, chunkZ * Chunk.TileSize);
            ThreadPool.QueueUserWorkItem(chunk.Load, info);

            return chunk;
        }

        private static int GetChunkKey(int chunkX, int chunkY, int chunkZ)
        {
            return chunkX + (chunkY << 8) + (chunkZ << 16);
        }
    }
}
