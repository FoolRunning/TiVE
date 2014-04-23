using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.Particles;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;
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

            int drawCount = 0;
            int polygonCount = 0;
            int voxelCount = 0;
            int renderedVoxelCount = 0;
            // Render opaque particles first
            for (int i = 0; i < renderList.Count; i++)
            {
                if (renderList[i].HasTransparency)
                    continue;
                RenderStatistics stats = renderList[i].Render(ref matrixMVP);
                drawCount += stats.DrawCount;
                polygonCount += stats.PolygonCount;
                voxelCount += stats.VoxelCount;
                renderedVoxelCount += stats.RenderedVoxelCount;
            }

            // Render transparent particles last
            for (int i = 0; i < renderList.Count; i++)
            {
                if (!renderList[i].HasTransparency)
                    continue;
                RenderStatistics stats = renderList[i].Render(ref matrixMVP);
                drawCount += stats.DrawCount;
                polygonCount += stats.PolygonCount;
                voxelCount += stats.VoxelCount;
                renderedVoxelCount += stats.RenderedVoxelCount;
            }

            return new RenderStatistics(drawCount, polygonCount, voxelCount, renderedVoxelCount);
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
