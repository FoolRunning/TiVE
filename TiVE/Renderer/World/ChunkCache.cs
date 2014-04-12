using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading;
using ProdigalSoftware.TiVE.Renderer.Voxels;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    internal sealed class ChunkLoadInfo
    {
        private readonly GameWorldVoxelChunk chunk;

        public ChunkLoadInfo(GameWorldVoxelChunk chunk, GameWorld gameWorld, BlockList blockList)
        {
            this.chunk = chunk;
            GameWorld = gameWorld;
            BlockList = blockList;
        }

        public GameWorld GameWorld { get; private set; }
        public BlockList BlockList { get; private set; }

        public GameWorldVoxelChunk Chunk
        {
            get { return chunk; }
        }

        public void Load(MeshBuilder meshBuilder)
        {
            chunk.Load(this, meshBuilder);
        }
    }

    internal sealed class ChunkCache : IDisposable
    {
        private const int ChunkCacheSize = 4000 / GameWorldVoxelChunk.TileSize;
        /// <summary>
        /// Distance in world tiles (outside the viewable area) to start removing loaded chunks
        /// </summary>
        private const int ChunkUnloadDistance = 4 * GameWorldVoxelChunk.TileSize;

        private readonly GameWorld gameWorld;
        private readonly BlockList blockList;

        private readonly List<Tuple<int, GameWorldVoxelChunk>> chunksToDelete = new List<Tuple<int, GameWorldVoxelChunk>>();
        private readonly Dictionary<int, GameWorldVoxelChunk> chunks = new Dictionary<int, GameWorldVoxelChunk>(1200);
        private readonly Queue<ChunkLoadInfo> chunkLoadQueue = new Queue<ChunkLoadInfo>();

        public ChunkCache(GameWorld gameWorld, BlockList blockList)
        {
            this.gameWorld = gameWorld;
            this.blockList = blockList;

            int coreCount = new ManagementObjectSearcher("Select * from Win32_Processor").Get()
                .Cast<ManagementBaseObject>().Sum(item => int.Parse(item["NumberOfCores"].ToString()));

            for (int i = 0; i < Math.Max(coreCount - 1, 1); i++)
                StartChunkCreateThread();
        }

        public void Dispose()
        {
            foreach (GameWorldVoxelChunk chunk in chunks.Values)
                chunk.Dispose();

            chunks.Clear();
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
                if (chunks.Count - chunksToDelete.Count < ChunkCacheSize)
                    break;

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
            ChunkLoadInfo info = new ChunkLoadInfo(chunk, gameWorld, blockList);
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
            Thread chunkCreationThread = new Thread(ChunkCreateLoop);
            chunkCreationThread.IsBackground = true;
            chunkCreationThread.Name = "ChunkLoad";
            chunkCreationThread.Start();
        }

        private void ChunkCreateLoop()
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

                MeshBuilder meshBuilder = meshBuilders.Find(NotLocked);
                if (meshBuilder == null && meshBuilders.Count < 5)
                {
                    meshBuilder = new MeshBuilder(200000, 400000);
                    meshBuilders.Add(meshBuilder);
                    Debug.WriteLine("Meshbuilder count: " + meshBuilders.Count);
                }

                ChunkLoadInfo chunkInfo;
                lock (chunkLoadQueue)
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
