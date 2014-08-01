using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using OpenTK;
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
        private readonly Queue<GameWorldVoxelChunk> chunkLoadQueue = new Queue<GameWorldVoxelChunk>();
        private readonly List<Thread> chunkCreationThreads = new List<Thread>();
        
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
            chunkMinX = Math.Max(0, Math.Min(gameWorld.XChunkSize, camMinX / GameWorldVoxelChunk.TileSize - 1));
            chunkMaxX = Math.Max(0, Math.Min(gameWorld.XChunkSize, (int)Math.Ceiling(camMaxX / (float)GameWorldVoxelChunk.TileSize) + 1));
            chunkMinY = Math.Max(0, Math.Min(gameWorld.YChunkSize, camMinY / GameWorldVoxelChunk.TileSize - 1));
            chunkMaxY = Math.Max(0, Math.Min(gameWorld.YChunkSize, (int)Math.Ceiling(camMaxY / (float)GameWorldVoxelChunk.TileSize) + 1));
            chunkMaxZ = Math.Max((int)Math.Ceiling(gameWorld.Zsize / (float)GameWorldVoxelChunk.TileSize), 1);

            for (int i = 0; i < loadedChunksList.Count; i++)
            {
                GameWorldVoxelChunk chunk = loadedChunksList[i];
                if (!chunk.IsInside(chunkMinX - 1, chunkMinY - 1, chunkMaxX + 1, chunkMaxY + 1))
                    chunksToDelete.Add(chunk);
            }

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
                            using (new PerformanceLock(chunkLoadQueue))
                                chunkLoadQueue.Enqueue(chunk);
                            loadedChunks.Add(chunk);
                            loadedChunksList.Add(chunk);
                        }
                    }
                }
            }
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

            for (int i = 0; i < loadedChunksList.Count; i++)
            {
                GameWorldVoxelChunk chunk = loadedChunksList[i];
                if (!chunk.IsInitialized)
                    chunk.Initialize();
            }

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
                    Thread.Sleep(1);
                    continue;
                }

                MeshBuilder meshBuilder = meshBuilders.Find(NotLocked);
                if (meshBuilder == null && meshBuilders.Count < 5)
                {
                    meshBuilder = new MeshBuilder(800000, 800000);
                    meshBuilders.Add(meshBuilder);
                }

                if (meshBuilder == null)
                    continue;

                GameWorldVoxelChunk chunk;
                using (new PerformanceLock(chunkLoadQueue))
                {
                    if (chunkLoadQueue.Count == 0)
                        continue; // Check for race condition with multiple threads accessing the queue

                    chunk = chunkLoadQueue.Dequeue();
                }

                if (!chunk.IsDeleted)
                    chunk.Load(meshBuilder);
            }
        }

        private static bool NotLocked(MeshBuilder meshBuilder)
        {
            return !meshBuilder.IsLocked;
        }
    }
}
