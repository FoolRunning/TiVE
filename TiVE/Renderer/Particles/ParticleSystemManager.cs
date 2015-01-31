using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Particles;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.Particles
{
    internal sealed class ParticleSystemManager : IDisposable
    {
        private const int ParticleFPS = 60;

        private readonly List<ParticleSystemCollection> renderList = new List<ParticleSystemCollection>();
        private readonly List<ParticleSystemCollection> updateList = new List<ParticleSystemCollection>();
        private readonly Dictionary<ParticleSystemInformation, ParticleSystemCollection> particleSystemCollections =
            new Dictionary<ParticleSystemInformation, ParticleSystemCollection>();

        private readonly HashSet<RunningParticleSystem> systemsToRender = new HashSet<RunningParticleSystem>();
        private readonly HashSet<RunningParticleSystem> runningSystems = new HashSet<RunningParticleSystem>();
        private readonly List<RunningParticleSystem> systemsToDelete = new List<RunningParticleSystem>();
        private readonly IGameWorldRenderer renderer;
        private readonly Thread particleUpdateThread;
        private volatile bool stopThread;

        public ParticleSystemManager(IGameWorldRenderer renderer)
        {
            this.renderer = renderer;
            if (TiVEController.UserSettings.Get(UserSettings.UseThreadedParticlesKey))
            {
                particleUpdateThread = new Thread(ParticleUpdateLoop);
                particleUpdateThread.Priority = ThreadPriority.Normal;
                particleUpdateThread.IsBackground = true;
                particleUpdateThread.Name = "ParticleUpdate";
                particleUpdateThread.Start();
            }
        }

        public void Dispose()
        {
            stopThread = true;
            if (particleUpdateThread != null && particleUpdateThread.IsAlive)
                particleUpdateThread.Join();

            foreach (ParticleSystemCollection systemCollection in particleSystemCollections.Values)
                systemCollection.Dispose();
            particleSystemCollections.Clear();
        }

        public void UpdateCameraPos(HashSet<GameWorldVoxelChunk> chunksToRender)
        {
            Debug.Assert(Thread.CurrentThread.Name == "Main UI");

            GameWorld gameWorld = renderer.GameWorld;
            systemsToRender.Clear();
            foreach (GameWorldVoxelChunk chunk in chunksToRender)
            {
                int blockStartX = chunk.ChunkBlockLocation.X;
                int blockStartY = chunk.ChunkBlockLocation.Y;
                int blockStartZ = chunk.ChunkBlockLocation.Z;
                int blockLimitX = chunk.ChunkBlockLocation.X + GameWorldVoxelChunk.BlockSize;
                int blockLimitY = chunk.ChunkBlockLocation.Y + GameWorldVoxelChunk.BlockSize;
                int blockLimitZ = chunk.ChunkBlockLocation.Z + GameWorldVoxelChunk.BlockSize;

                for (int blockZ = blockStartZ; blockZ < blockLimitZ; blockZ++)
                {
                    for (int blockX = blockStartX; blockX < blockLimitX; blockX++)
                    {
                        for (int blockY = blockStartY; blockY < blockLimitY; blockY++)
                        {
                            ParticleSystemInformation particleInfo = gameWorld[blockX, blockY, blockZ].ParticleSystem;
                            RunningParticleSystem runningParticleSystem = new RunningParticleSystem(blockX, blockY, blockZ);
                            systemsToRender.Add(runningParticleSystem);
                            if (particleInfo != null && !runningSystems.Contains(runningParticleSystem))
                            {
                                RunningParticleSystem newSystem = new RunningParticleSystem(blockX, blockY, blockZ, particleInfo);
                                runningSystems.Add(newSystem);
                                AddParticleSystem(newSystem);
                            }
                        }
                    }
                }
            }

            foreach (RunningParticleSystem runningSystem in runningSystems)
            {
                if (!systemsToRender.Contains(runningSystem))
                    systemsToDelete.Add(runningSystem);
            }

            for (int i = 0; i < systemsToDelete.Count; i++)
            {
                RunningParticleSystem runningSystem = systemsToDelete[i];
                runningSystems.Remove(runningSystem);
                RemoveParticleSystem(runningSystem);
            }
            systemsToDelete.Clear();
        }

        public void UpdateParticles(float timeSinceLastUpdate)
        {
            foreach (ParticleSystemCollection collection in particleSystemCollections.Values)
                collection.UpdateAll(renderer, timeSinceLastUpdate);
        }

        public RenderStatistics Render(ShaderManager shaderManager, ref Matrix4 matrixMVP)
        {
            Debug.Assert(Thread.CurrentThread.Name == "Main UI");

            renderList.Clear();
            using (new PerformanceLock(particleSystemCollections))
                renderList.AddRange(particleSystemCollections.Values);

            RenderStatistics stats = new RenderStatistics();
            // Render opaque particles first
            foreach (ParticleSystemCollection system in renderList.Where(s => s.TransparencyType == TransparencyType.None))
                stats += system.Render(shaderManager, ref matrixMVP);

            // Render transparent particles last
            foreach (ParticleSystemCollection system in renderList.Where(s => s.TransparencyType == TransparencyType.Additive))
                stats += system.Render(shaderManager, ref matrixMVP);

            foreach (ParticleSystemCollection system in renderList.Where(s => s.TransparencyType == TransparencyType.Realistic))
                stats += system.Render(shaderManager, ref matrixMVP);
 
            return stats;
        }

        private void AddParticleSystem(RunningParticleSystem system)
        {
            ParticleSystemCollection collection;
            using (new PerformanceLock(particleSystemCollections))
            {
                if (!particleSystemCollections.TryGetValue(system.SystemInfo, out collection))
                    particleSystemCollections[system.SystemInfo] = collection = new ParticleSystemCollection(system.SystemInfo);
            }
            collection.Add(system);
        }

        private void RemoveParticleSystem(RunningParticleSystem system)
        {
            ParticleSystemCollection collection;
            using (new PerformanceLock(particleSystemCollections))
                particleSystemCollections.TryGetValue(system.SystemInfo, out collection);

            if (collection != null)
                collection.Remove(system);
        }

        private void ParticleUpdateLoop()
        {
            float ticksPerSecond = Stopwatch.Frequency;
            long particleUpdateTime = Stopwatch.Frequency / ParticleFPS;
            long lastTime = 0;
            Stopwatch sw = Stopwatch.StartNew();
            while (!stopThread)
            {
                long newTicks = sw.ElapsedTicks;
                if (newTicks >= lastTime + particleUpdateTime)
                {
                    float timeSinceLastUpdate = (newTicks - lastTime) / ticksPerSecond;
                    if (timeSinceLastUpdate > 0.1f)
                        timeSinceLastUpdate = 0.1f;

                    lastTime = newTicks;

                    updateList.Clear();
                    using (new PerformanceLock(particleSystemCollections))
                        updateList.AddRange(particleSystemCollections.Values);

                    for (int i = 0; i < updateList.Count; i++)
                        updateList[i].UpdateAll(renderer, timeSinceLastUpdate);
                }
                else if (lastTime + particleUpdateTime - TiVEController.MaxTicksForSleep > newTicks)
                    Thread.Sleep(1);
            }
            sw.Stop();
        }
    }
}
