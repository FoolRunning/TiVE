using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.Particles;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework.Particles;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Resources
{
    internal sealed class ParticleSystemManager : IDisposable
    {
        private const int ParticleFPS = 60;

        private readonly List<ParticleSystemCollection> renderList = new List<ParticleSystemCollection>();
        private readonly List<ParticleSystemCollection> updateList = new List<ParticleSystemCollection>();
        private readonly Dictionary<ParticleSystemInformation, ParticleSystemCollection> particleSystemCollections =
            new Dictionary<ParticleSystemInformation, ParticleSystemCollection>();

        private Thread particleUpdateThread;
        private volatile bool stopThread;

        public void Dispose()
        {
            stopThread = true;
            particleUpdateThread.Join();

            foreach (ParticleSystemCollection systemCollection in particleSystemCollections.Values)
                systemCollection.Dispose();
            particleSystemCollections.Clear();
        }

        public bool Initialize()
        {
            Messages.Print("Starting particle update thread...");

            particleUpdateThread = new Thread(ParticleUpdateLoop);
            particleUpdateThread.IsBackground = true;
            particleUpdateThread.Name = "ParticleUpdate";
            particleUpdateThread.Start();

            Messages.AddDoneText();
            return true;
        }

        public void UpdateCameraPos(int camMinX, int camMaxX, int camMinY, int camMaxY)
        {
            Debug.Assert(Thread.CurrentThread.Name == "Main UI");

            //GameWorld gameWorld = ResourceManager.GameWorldManager.GameWorld;
            //chunkMinX = Math.Max(0, Math.Min(gameWorld.XChunkSize, camMinX / GameWorldVoxelChunk.TileSize - 1));
            //chunkMaxX = Math.Max(0, Math.Min(gameWorld.XChunkSize, (int)Math.Ceiling(camMaxX / (float)GameWorldVoxelChunk.TileSize) + 1));
            //chunkMinY = Math.Max(0, Math.Min(gameWorld.YChunkSize, camMinY / GameWorldVoxelChunk.TileSize - 1));
            //chunkMaxY = Math.Max(0, Math.Min(gameWorld.YChunkSize, (int)Math.Ceiling(camMaxY / (float)GameWorldVoxelChunk.TileSize) + 1));
            //chunkMaxZ = Math.Max((int)Math.Ceiling(gameWorld.Zsize / (float)GameWorldVoxelChunk.TileSize), 1);

            //for (int i = 0; i < loadedChunksList.Count; i++)
            //{
            //    GameWorldVoxelChunk chunk = loadedChunksList[i];
            //    if (!chunk.IsInside(chunkMinX, chunkMinY, chunkMaxX, chunkMaxY))
            //        chunksToDelete.Add(chunk);
            //}

            //for (int chunkZ = chunkMaxZ - 1; chunkZ >= 0; chunkZ--)
            //{
            //    for (int chunkX = chunkMinX; chunkX < chunkMaxX; chunkX++)
            //    {
            //        for (int chunkY = chunkMinY; chunkY < chunkMaxY; chunkY++)
            //        {
            //            GameWorldVoxelChunk chunk = gameWorld.GetChunk(chunkX, chunkY, chunkZ);
            //            if (!loadedChunks.Contains(chunk))
            //            {
            //                chunk.PrepareForLoad();
            //                using (new PerformanceLock(chunkLoadQueue))
            //                    chunkLoadQueue.Enqueue(chunk);
            //                loadedChunks.Add(chunk);
            //                loadedChunksList.Add(chunk);
            //            }
            //        }
            //    }
            //}
        }

        public void AddParticleSystem(ParticleSystem system)
        {
            ParticleSystemCollection collection;
            using (new PerformanceLock(particleSystemCollections))
            {
                if (!particleSystemCollections.TryGetValue(system.SystemInformation, out collection))
                    particleSystemCollections[system.SystemInformation] = collection = new ParticleSystemCollection(system.SystemInformation);
            }
            collection.Add(system);
        }

        public void RemoveParticleSystem(ParticleSystem system)
        {
            ParticleSystemCollection collection;
            using (new PerformanceLock(particleSystemCollections))
                particleSystemCollections.TryGetValue(system.SystemInformation, out collection);
            
            if (collection != null)
                collection.Remove(system);
        }

        public RenderStatistics Render(ref Matrix4 matrixMVP)
        {
            renderList.Clear();
            using (new PerformanceLock(particleSystemCollections))
                renderList.AddRange(particleSystemCollections.Values);

            RenderStatistics stats = new RenderStatistics();
            // Render opaque particles first
            for (int i = 0; i < renderList.Count; i++)
            {
                if (renderList[i].HasTransparency)
                    continue;
                stats += renderList[i].Render(ref matrixMVP);
            }

            // Render transparent particles last
            for (int i = 0; i < renderList.Count; i++)
            {
                if (!renderList[i].HasTransparency)
                    continue;
                stats += renderList[i].Render(ref matrixMVP);
            }

            return stats;
        }

        private void ParticleUpdateLoop()
        {
            float ticksPerSecond = Stopwatch.Frequency;
            long particleUpdateTime = Stopwatch.Frequency / ParticleFPS;
            long lastTime = 0;
            Stopwatch sw = Stopwatch.StartNew();
            while (!stopThread)
            {
                Thread.Sleep(1);
                long newTicks = sw.ElapsedTicks;
                if (newTicks >= lastTime + particleUpdateTime)
                {
                    float timeSinceLastUpdate = (newTicks - lastTime) / ticksPerSecond;
                    
                    updateList.Clear();
                    using (new PerformanceLock(particleSystemCollections))
                        updateList.AddRange(particleSystemCollections.Values);

                    for (int i = 0; i < updateList.Count; i++)
                        updateList[i].UpdateAll(timeSinceLastUpdate);

                    lastTime = newTicks;
                }
            }
            sw.Stop();
        }
    }
}
