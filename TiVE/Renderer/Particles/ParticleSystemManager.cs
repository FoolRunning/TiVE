using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.World;
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

        private readonly HashSet<SystemInfo> runningSystems = new HashSet<SystemInfo>();
        private readonly List<SystemInfo> systemsToDelete = new List<SystemInfo>();
        private readonly GameWorld gameWorld;
        private readonly Thread particleUpdateThread;
        private volatile bool stopThread;

        public ParticleSystemManager(GameWorld gameWorld)
        {
            this.gameWorld = gameWorld;
            particleUpdateThread = new Thread(ParticleUpdateLoop);
            particleUpdateThread.Priority = ThreadPriority.BelowNormal;
            particleUpdateThread.IsBackground = true;
            particleUpdateThread.Name = "ParticleUpdate";
            particleUpdateThread.Start();
        }

        public void Dispose()
        {
            stopThread = true;
            if (particleUpdateThread.IsAlive)
                particleUpdateThread.Join();

            foreach (ParticleSystemCollection systemCollection in particleSystemCollections.Values)
                systemCollection.Dispose();
            particleSystemCollections.Clear();
        }

        public void UpdateCameraPos(HashSet<GameWorldVoxelChunk> chunksToRender)
        {
            Debug.Assert(Thread.CurrentThread.Name == "Main UI");

            // TODO: Implement this again based on the new chunk renderer

            //foreach (SystemInfo runningSystem in runningSystems)
            //{
            //    if (runningSystem.X < camMinX || runningSystem.X >= camMaxX ||
            //        runningSystem.Y < camMinY || runningSystem.Y >= camMaxY)
            //    {
            //        systemsToDelete.Add(runningSystem);
            //    }
            //}

            for (int i = 0; i < systemsToDelete.Count; i++)
            {
                SystemInfo runningSystem = systemsToDelete[i];
                runningSystems.Remove(runningSystem);
                RemoveParticleSystem(runningSystem.System);
            }
            systemsToDelete.Clear();

            //for (int z = 0; z < gameWorld.BlockSize.Z; z++)
            //{
            //    for (int x = camMinX; x < camMaxX; x++)
            //    {
            //        for (int y = camMinY; y < camMaxY; y++)
            //        {
            //            ParticleSystemInformation particleInfo = gameWorld[x, y, z].ParticleSystem;
            //            if (particleInfo != null && !runningSystems.Contains(new SystemInfo(x, y, z)))
            //            {
            //                Vector3b loc = particleInfo.Location;
            //                ParticleSystem system = new ParticleSystem(particleInfo, (x * BlockInformation.VoxelSize) + loc.X,
            //                    (y * BlockInformation.VoxelSize) + loc.Y, (z * BlockInformation.VoxelSize) + loc.Z);
            //                runningSystems.Add(new SystemInfo(x, y, z, system));
            //                AddParticleSystem(system);
            //            }
            //        }
            //    }
            //}
        }

        public RenderStatistics Render(ShaderManager shaderManager, ref Matrix4 matrixMVP)
        {
            renderList.Clear();
            using (new PerformanceLock(particleSystemCollections))
                renderList.AddRange(particleSystemCollections.Values);

            RenderStatistics stats = new RenderStatistics();
            // Render opaque particles first
            foreach (ParticleSystemCollection system in renderList)
            {
                if (!system.HasTransparency)
                    stats += system.Render(shaderManager, ref matrixMVP);
            }

            // Render transparent particles last
            foreach (ParticleSystemCollection system in renderList)
            {
                if (system.HasTransparency)
                    stats += system.Render(shaderManager, ref matrixMVP);
            }

            return stats;
        }

        private void AddParticleSystem(ParticleSystem system)
        {
            ParticleSystemCollection collection;
            using (new PerformanceLock(particleSystemCollections))
            {
                if (!particleSystemCollections.TryGetValue(system.SystemInformation, out collection))
                    particleSystemCollections[system.SystemInformation] = collection = new ParticleSystemCollection(system.SystemInformation);
            }
            collection.Add(system);
        }

        private void RemoveParticleSystem(ParticleSystem system)
        {
            ParticleSystemCollection collection;
            using (new PerformanceLock(particleSystemCollections))
                particleSystemCollections.TryGetValue(system.SystemInformation, out collection);

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
                        updateList[i].UpdateAll(gameWorld, timeSinceLastUpdate);
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
                // 12 bits for x and y and 8 bits for z (Enough for unique hashes for each block of a 4096x4096x256 world)
                return ((X & 0xFFF) << 20) ^ ((Y & 0xFFF) << 8) ^ (Z & 0xFF);
            }
        }
        #endregion
    }
}
