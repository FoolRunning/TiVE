﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private const int VoxelDetailLevelSections = 3; // 16x16x16 = 4096v, 8x8x8 = 512v, 4x4x4 = 64v, not worth going to 2x2x2 = 8v.
        private const int BestVoxelDetailLevel = 0;
        private const int WorstVoxelDetailLevel = VoxelDetailLevelSections - 1;
        private const int TotalMeshBuilders = 20;
        private const int MaxQueueSize = 2000;

        private readonly List<GameWorldVoxelChunk> chunksToDelete = new List<GameWorldVoxelChunk>();
        private readonly HashSet<GameWorldVoxelChunk> loadedChunks = new HashSet<GameWorldVoxelChunk>();

        private readonly List<Thread> chunkCreationThreads = new List<Thread>();
        private readonly ChunkLoadQueue chunkLoadQueue = new ChunkLoadQueue(MaxQueueSize);
        private readonly List<MeshBuilder> meshBuilders;

        private readonly IGameWorldRenderer renderer;
        
        private volatile bool endCreationThreads;

        public WorldChunkManager(IGameWorldRenderer renderer, int maxThreads)
        {
            this.renderer = renderer;

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

        public void CleanUpChunks()
        {
            Debug.Assert(Thread.CurrentThread.Name == "Main UI");

            using (new PerformanceLock(chunkLoadQueue))
            {
                for (int i = 0; i < chunksToDelete.Count; i++)
                    chunkLoadQueue.Remove(chunksToDelete[i]);
            }

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
                case VoxelDetailLevelDistance.Closest: distancePerLevel = 300; break;
                case VoxelDetailLevelDistance.Close: distancePerLevel = 450; break;
                case VoxelDetailLevelDistance.Mid: distancePerLevel = 600; break;
                case VoxelDetailLevelDistance.Far: distancePerLevel = 750; break;
                default: distancePerLevel = 900; break;
            }

            for (int i = BestVoxelDetailLevel; i <= WorstVoxelDetailLevel; i++)
            {
                if (dist <= distancePerLevel)
                    return i;
                dist -= distancePerLevel * (i + 1);
            }
            return WorstVoxelDetailLevel;
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
                if (!chunkLoadQueue.Contains(chunk, voxelDetailLevel))
                    chunkLoadQueue.Enqueue(chunk, voxelDetailLevel);
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
                    hasItemToLoad = chunkLoadQueue.Size > 0;

                if (!hasItemToLoad)
                {
                    Thread.Sleep(1);
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
                    continue; // No free meshbuilders to use

                GameWorldVoxelChunk chunk;
                int foundChunkDetailLevel;
                using (new PerformanceLock(chunkLoadQueue))
                    chunk = chunkLoadQueue.Dequeue(out foundChunkDetailLevel);

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
