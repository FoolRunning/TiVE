using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using OpenTK;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVE.Renderer;
using ProdigalSoftware.TiVE.Renderer.Voxels;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Resources
{
    internal sealed class WorldChunkManager : IDisposable
    {
        private readonly List<GameWorldVoxelChunk> chunksToDelete = new List<GameWorldVoxelChunk>();
        private readonly HashSet<GameWorldVoxelChunk> loadedChunks = new HashSet<GameWorldVoxelChunk>();
        private readonly List<GameWorldVoxelChunk> loadedChunksList = new List<GameWorldVoxelChunk>(1200);
        private readonly List<Thread> chunkCreationThreads = new List<Thread>();
        private readonly Queue<GameWorldVoxelChunk> chunkLoadQueue = new Queue<GameWorldVoxelChunk>();
        
        private readonly bool useInstancing;
        private IRendererData voxelInstanceLocationData;
        private IRendererData voxelInstanceColorData;
        private int polysPerVoxel;

        private volatile bool endCreationThreads;

        private int chunkMinX;
        private int chunkMaxX;
        private int chunkMinY;
        private int chunkMaxY;
        private int chunkMaxZ;

        public WorldChunkManager(bool useInstancing)
        {
            this.useInstancing = useInstancing;
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

            if (voxelInstanceLocationData != null)
            {
                voxelInstanceLocationData.Unlock();
                voxelInstanceLocationData.Dispose();
            }

            if (voxelInstanceColorData != null)
            {
                voxelInstanceColorData.Unlock();
                voxelInstanceColorData.Dispose();
            }
        }

        public bool Initialize()
        {
            Messages.Print("Starting chunk creations threads...");

            if (useInstancing)
            {
                MeshBuilder voxelInstanceBuilder = new MeshBuilder(30, 0);
                polysPerVoxel = SimpleVoxelGroup.CreateVoxel(voxelInstanceBuilder, VoxelSides.All, 0, 0, 0, 255, 255, 255, 255);
                voxelInstanceLocationData = voxelInstanceBuilder.GetLocationData();
                voxelInstanceLocationData.Lock();
                voxelInstanceColorData = voxelInstanceBuilder.GetColorData();
                voxelInstanceColorData.Lock();
            }

            endCreationThreads = false;
            int threadCount = Environment.ProcessorCount > 2 ? 2 : 1;
            for (int i = 0; i < threadCount; i++)
                chunkCreationThreads.Add(StartChunkCreateThread(i + 1));

            Messages.AddDoneText();
            return true;
        }

        public void UpdateCameraPos(int camMinX, int camMaxX, int camMinY, int camMaxY)
        {
            Debug.Assert(Thread.CurrentThread.Name == "Main UI");

            GameWorld gameWorld = ResourceManager.GameWorldManager.GameWorld;
            chunkMinX = Math.Max(0, Math.Min(gameWorld.ChunkSizeX, camMinX / GameWorldVoxelChunk.TileSize - 1));
            chunkMaxX = Math.Max(0, Math.Min(gameWorld.ChunkSizeX, (int)Math.Ceiling(camMaxX / (float)GameWorldVoxelChunk.TileSize) + 1));
            chunkMinY = Math.Max(0, Math.Min(gameWorld.ChunkSizeY, camMinY / GameWorldVoxelChunk.TileSize - 1));
            chunkMaxY = Math.Max(0, Math.Min(gameWorld.ChunkSizeY, (int)Math.Ceiling(camMaxY / (float)GameWorldVoxelChunk.TileSize) + 1));
            chunkMaxZ = Math.Max((int)Math.Ceiling(gameWorld.BlockSizeZ / (float)GameWorldVoxelChunk.TileSize), 1);

            for (int i = 0; i < loadedChunksList.Count; i++)
            {
                GameWorldVoxelChunk chunk = loadedChunksList[i];
                if (!chunk.IsInside(chunkMinX, chunkMinY, chunkMaxX, chunkMaxY))
                    chunksToDelete.Add(chunk);
            }

            BlockList blockList = ResourceManager.BlockListManager.BlockList;
            //for (int chunkZ = chunkMaxZ - 1; chunkZ >= 0; chunkZ--)
            //{
            //    for (int chunkX = chunkMinX; chunkX < chunkMaxX; chunkX++)
            //    {
            //        for (int chunkY = chunkMinY; chunkY < chunkMaxY; chunkY++)
            //        {
            //            GameWorldVoxelChunk chunk = gameWorld.GetChunk(chunkX, chunkY, chunkZ);
            //            if (loadedChunks.Contains(chunk) && UpdateBlockAnimationsInChunk(chunkX, chunkY, chunkZ, gameWorld, blockList))
            //                UpdateChunk(chunk);
            //        }
            //    }
            //}

            for (int chunkZ = chunkMaxZ - 1; chunkZ >= 0; chunkZ--)
            {
                for (int chunkX = chunkMinX; chunkX < chunkMaxX; chunkX++)
                {
                    for (int chunkY = chunkMinY; chunkY < chunkMaxY; chunkY++)
                    {
                        GameWorldVoxelChunk chunk = gameWorld.GetChunk(chunkX, chunkY, chunkZ);
                        if (!loadedChunks.Contains(chunk))
                        {
                            chunk.PrepareForLoad();
                            UpdateChunk(chunk);
                            loadedChunks.Add(chunk);
                            loadedChunksList.Add(chunk);
                        }
                        else if (UpdateBlockAnimationsInChunk(chunkX, chunkY, chunkZ, gameWorld, blockList))
                            UpdateChunk(chunk);
                    }
                }
            }
        }

        private static bool UpdateBlockAnimationsInChunk(int chunkX, int chunkY, int chunkZ, GameWorld gameWorld, BlockList blockList)
        {
            int worldStartX = chunkX * GameWorldVoxelChunk.TileSize;
            int worldStartY = chunkY * GameWorldVoxelChunk.TileSize;
            int worldStartZ = chunkZ * GameWorldVoxelChunk.TileSize;

            int worldEndX = Math.Min(gameWorld.BlockSizeX, worldStartX + GameWorldVoxelChunk.TileSize);
            int worldEndY = Math.Min(gameWorld.BlockSizeY, worldStartY + GameWorldVoxelChunk.TileSize);
            int worldEndZ = Math.Min(gameWorld.BlockSizeZ, worldStartZ + GameWorldVoxelChunk.TileSize);

            bool changedChunk = false;
            for (int z = worldStartZ; z < worldEndZ; z++)
            {
                for (int x = worldStartX; x < worldEndX; x++)
                {
                    for (int y = worldStartY; y < worldEndY; y++)
                    {
                        BlockInformation newBlock = blockList.NextFrameFor(gameWorld[x, y, z]);
                        if (newBlock != null)
                        {
                            gameWorld[x, y, z] = newBlock;
                            changedChunk = true;
                        }
                    }
                }
            }

            return changedChunk;
        }

        public RenderStatistics Render(ref Matrix4 viewProjectionMatrix)
        {
            Debug.Assert(Thread.CurrentThread.Name == "Main UI");

            for (int i = 0; i < chunksToDelete.Count; i++)
            {
                GameWorldVoxelChunk chunk = chunksToDelete[i];
                loadedChunks.Remove(chunk);
                loadedChunksList.Remove(chunk);
                chunk.Dispose();
            }

            chunksToDelete.Clear();

            RenderStatistics stats = new RenderStatistics();
            GameWorld gameWorld = ResourceManager.GameWorldManager.GameWorld;
            for (int chunkZ = chunkMaxZ - 1; chunkZ >= 0; chunkZ--)
            {
                for (int chunkX = chunkMinX; chunkX < chunkMaxX; chunkX++)
                {
                    for (int chunkY = chunkMinY; chunkY < chunkMaxY; chunkY++)
                        stats += gameWorld.GetChunk(chunkX, chunkY, chunkZ).Render(ref viewProjectionMatrix);
                }
            }

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
            while (!endCreationThreads)
            {
                int count;
                using (new PerformanceLock(chunkLoadQueue))
                    count = chunkLoadQueue.Count;

                if (count == 0)
                {
                    Thread.Sleep(3);
                    continue;
                }

                MeshBuilder meshBuilder;
                GameWorldVoxelChunk chunk;

                using (new PerformanceLock(chunkLoadQueue))
                {
                    if (chunkLoadQueue.Count == 0)
                        continue; // Check for race condition with multiple threads accessing the queue

                    meshBuilder = meshBuilders.Find(NotLocked);
                    if (meshBuilder == null && meshBuilders.Count < 15)
                    {
                        meshBuilder = new MeshBuilder(800000, 800000);
                        meshBuilders.Add(meshBuilder);
                        Debug.WriteLine(meshBuilders.Count);
                    }

                    if (meshBuilder == null)
                        continue; // No free meshbuilders to use

                    chunk = chunkLoadQueue.Dequeue();
                    if (chunk.IsDeleted)
                        continue; // Chunk got deleted while waiting to be loaded

                    meshBuilder.StartNewMesh();
                }

                chunk.Load(meshBuilder);
            }
        }

        private static bool NotLocked(MeshBuilder meshBuilder)
        {
            return !meshBuilder.IsLocked;
        }
    }
}
