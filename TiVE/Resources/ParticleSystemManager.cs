using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.Particles;
using ProdigalSoftware.TiVE.Renderer.Voxels;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;
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

        private readonly HashSet<SystemInfo> runningSystems = new HashSet<SystemInfo>();
        private readonly List<SystemInfo> systemsToDelete = new List<SystemInfo>();
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
            particleUpdateThread.Priority = ThreadPriority.BelowNormal;
            particleUpdateThread.IsBackground = true;
            particleUpdateThread.Name = "ParticleUpdate";
            particleUpdateThread.Start();

            Messages.AddDoneText();
            return true;
        }

        public void UpdateCameraPos(int camMinX, int camMaxX, int camMinY, int camMaxY)
        {
            Debug.Assert(Thread.CurrentThread.Name == "Main UI");

            camMinY = Math.Max(camMinY - GameWorldVoxelChunk.TileSize, 0);

            foreach (SystemInfo runningSystem in runningSystems)
            {
                if (runningSystem.X < camMinX || runningSystem.X >= camMaxX ||
                    runningSystem.Y < camMinY || runningSystem.Y >= camMaxY)
                {
                    systemsToDelete.Add(runningSystem);
                }
            }

            for (int i = 0; i < systemsToDelete.Count; i++)
            {
                SystemInfo runningSystem = systemsToDelete[i];
                runningSystems.Remove(runningSystem);
                RemoveParticleSystem(runningSystem.System);
            }
            systemsToDelete.Clear();

            GameWorld gameWorld = ResourceManager.GameWorldManager.GameWorld;
            for (int z = 0; z < gameWorld.BlockSize.Z; z++)
            {
                for (int x = camMinX; x < camMaxX; x++)
                {
                    for (int y = camMinY; y < camMaxY; y++)
                    {
                        BlockInformation block = gameWorld[x, y, z];
                        ParticleSystemInformation particleInfo = block.ParticleSystem;
                        if (particleInfo != null && !runningSystems.Contains(new SystemInfo(x, y, z)))
                        {
                            Vector3b loc = particleInfo.Location;
                            ParticleSystem system = new ParticleSystem(particleInfo, (x * BlockInformation.BlockSize) + loc.X,
                                (y * BlockInformation.BlockSize) + loc.Y, (z * BlockInformation.BlockSize) + loc.Z);
                            runningSystems.Add(new SystemInfo(x, y, z, system));
                            AddParticleSystem(system);
                        }
                    }
                }
            }
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
                    if (timeSinceLastUpdate > 0.1f)
                        timeSinceLastUpdate = 0.1f;

                    lastTime = newTicks;

                    updateList.Clear();
                    using (new PerformanceLock(particleSystemCollections))
                        updateList.AddRange(particleSystemCollections.Values);

                    for (int i = 0; i < updateList.Count; i++)
                        updateList[i].UpdateAll(timeSinceLastUpdate);
                }
            }
            sw.Stop();
        }

        #region SystemInfo struct
        private struct SystemInfo
        {
            public readonly int X;
            public readonly int Y;
            public readonly int Z;
            public readonly ParticleSystem System;

            public SystemInfo(int x, int y, int z)
            {
                X = x;
                Y = y;
                Z = z;
                System = null;
            }

            public SystemInfo(int x, int y, int z, ParticleSystem system)
            {
                X = x;
                Y = y;
                Z = z;
                System = system;
            }

            public override bool Equals(object obj)
            {
                SystemInfo other = (SystemInfo)obj;
                return other.X == X && other.Y == Y && other.Z == Z;
            }

            public override int GetHashCode()
            {
                return X << 20 ^ Y << 10 ^ Z;
            }
        }
        #endregion
    }
}
