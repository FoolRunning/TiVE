using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.Voxels;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    internal sealed class WorldChunkManager : IDisposable
    {
        private const int MaxChunkUpdatesPerFrame = 7;

        private readonly List<GameWorldVoxelChunk> chunksToDelete = new List<GameWorldVoxelChunk>();
        private readonly HashSet<GameWorldVoxelChunk> loadedChunks = new HashSet<GameWorldVoxelChunk>();
        private readonly List<GameWorldVoxelChunk> loadedChunksList = new List<GameWorldVoxelChunk>(1200);
        private readonly List<Thread> chunkCreationThreads = new List<Thread>();
        private readonly Queue<GameWorldVoxelChunk> chunkLoadQueue = new Queue<GameWorldVoxelChunk>();
        
        private volatile bool endCreationThreads;

        public bool Initialize()
        {
            Messages.Print("Starting chunk creations threads...");

            endCreationThreads = false;
            for (int i = 0; i < 3; i++)
                chunkCreationThreads.Add(StartChunkCreateThread(i + 1));

            Messages.AddDoneText();
            return true;
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

            foreach (GameWorldVoxelChunk chunk in loadedChunksList)
                chunk.Dispose();
            loadedChunksList.Clear();
            loadedChunks.Clear();
        }

        public void UpdateCameraPos(int camMinX, int camMaxX, int camMinY, int camMaxY)
        {
            Debug.Assert(Thread.CurrentThread.Name == "Main UI");

            GameWorld gameWorld = ResourceManager.GameWorldManager.GameWorld;
            int chunkMinX = Math.Max(0, Math.Min(gameWorld.ChunkSize.X, camMinX / GameWorldVoxelChunk.TileSize - 1));
            int chunkMaxX = Math.Max(0, Math.Min(gameWorld.ChunkSize.X, (int)Math.Ceiling(camMaxX / (float)GameWorldVoxelChunk.TileSize) + 1));
            int chunkMinY = Math.Max(0, Math.Min(gameWorld.ChunkSize.Y, camMinY / GameWorldVoxelChunk.TileSize - 1));
            int chunkMaxY = Math.Max(0, Math.Min(gameWorld.ChunkSize.Y, (int)Math.Ceiling(camMaxY / (float)GameWorldVoxelChunk.TileSize) + 1));
            int chunkMaxZ = Math.Max((int)Math.Ceiling(gameWorld.BlockSize.Z / (float)GameWorldVoxelChunk.TileSize), 1);

            for (int i = 0; i < loadedChunksList.Count; i++)
            {
                GameWorldVoxelChunk chunk = loadedChunksList[i];
                if (!chunk.IsInside(chunkMinX, chunkMinY, chunkMaxX, chunkMaxY))
                    chunksToDelete.Add(chunk);
            }

            for (int chunkZ = chunkMaxZ - 1; chunkZ >= 0; chunkZ--)
            {
                for (int chunkX = chunkMinX; chunkX < chunkMaxX; chunkX++)
                {
                    for (int chunkY = chunkMinY; chunkY < chunkMaxY; chunkY++)
                    {
                        //GameWorldVoxelChunk chunk = gameWorld.GetChunk(chunkX, chunkY, chunkZ);
                        //if (!loadedChunks.Contains(chunk))
                        //{
                        //    chunk.PrepareForLoad();
                        //    UpdateChunk(chunk);
                        //    loadedChunks.Add(chunk);
                        //    loadedChunksList.Add(chunk);
                        //}
                    }
                }
            }
        }

        public RenderStatistics Render(ref Matrix4 viewProjectionMatrix, int camMinX, int camMaxX, int camMinY, int camMaxY)
        {
            Debug.Assert(Thread.CurrentThread.Name == "Main UI");

            GameWorld gameWorld = ResourceManager.GameWorldManager.GameWorld;
            int chunkMinX = Math.Max(0, Math.Min(gameWorld.ChunkSize.X, camMinX / GameWorldVoxelChunk.TileSize - 1));
            int chunkMaxX = Math.Max(0, Math.Min(gameWorld.ChunkSize.X, (int)Math.Ceiling(camMaxX / (float)GameWorldVoxelChunk.TileSize) + 1));
            int chunkMinY = Math.Max(0, Math.Min(gameWorld.ChunkSize.Y, camMinY / GameWorldVoxelChunk.TileSize - 1));
            int chunkMaxY = Math.Max(0, Math.Min(gameWorld.ChunkSize.Y, (int)Math.Ceiling(camMaxY / (float)GameWorldVoxelChunk.TileSize) + 1));
            int chunkMaxZ = Math.Max((int)Math.Ceiling(gameWorld.BlockSize.Z / (float)GameWorldVoxelChunk.TileSize), 1);

            for (int i = 0; i < chunksToDelete.Count; i++)
            {
                GameWorldVoxelChunk chunk = chunksToDelete[i];
                loadedChunks.Remove(chunk);
                loadedChunksList.Remove(chunk);
                chunk.Dispose();
            }

            chunksToDelete.Clear();

            int initializedChunkCount = 0;
            //int excessUninitializedChunkCount = 0;
            RenderStatistics stats = new RenderStatistics();
            for (int chunkZ = chunkMaxZ - 1; chunkZ >= 0; chunkZ--)
            {
                for (int chunkX = chunkMinX; chunkX < chunkMaxX; chunkX++)
                {
                    for (int chunkY = chunkMinY; chunkY < chunkMaxY; chunkY++)
                    {
                        GameWorldVoxelChunk chunk = gameWorld.GetChunk(chunkX, chunkY, chunkZ);
                        if (chunk.NeedsInitialization)
                        {
                            if (initializedChunkCount < MaxChunkUpdatesPerFrame)
                            {
                                if (chunk.Initialize())
                                    initializedChunkCount++;
                            }
                            //else
                            //    excessUninitializedChunkCount++;
                        }
                            
                        stats += chunk.Render(ref viewProjectionMatrix);
                    }
                }
            }
            //if (excessUninitializedChunkCount > 0)
            //    Console.WriteLine("Maxed chunk initializations for this frame by " + excessUninitializedChunkCount);

            return stats;
        }

        private void UpdateChunk(GameWorldVoxelChunk chunk)
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
            for (int i = 0; i < 5; i++)
                meshBuilders.Add(new MeshBuilder(500000, 1000000));

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
                    if (bottleneckCount > 100) // 300ms give or take
                    {
                        bottleneckCount = 0;
                        // Too many chunks are waiting to be intialized. Most likely there are chunks that were not properly disposed.
                        Console.WriteLine("Mesh creation bottlenecked!");

                        GameWorld gameWorld = ResourceManager.GameWorldManager.GameWorld;
                        for (int z = 0; z < gameWorld.ChunkSize.Z; z++)
                        {
                            for (int x = 0; x < gameWorld.ChunkSize.X; x++)
                            {
                                for (int y = 0; y < gameWorld.ChunkSize.Y; y++)
                                {
                                    GameWorldVoxelChunk oldChunk = gameWorld.GetChunk(x, y, z);
                                    if (oldChunk.MeshBuilder != null)
                                        chunksToDelete.Add(oldChunk);
                                }
                            }
                        }
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
                chunk.Load(meshBuilder);
            }
        }

        private static bool NotLocked(MeshBuilder meshBuilder)
        {
            return !meshBuilder.IsLocked;
        }
    }
}
