using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ProdigalSoftware.TiVE.RenderSystem.Lighting;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Particles;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.RenderSystem.Particles
{
    /// <summary>
    /// Represents a single particle emitter. Responsible for updating all particles owned by itself.
    /// </summary>
    internal sealed class ParticleSystem : IParticleSystem
    {
        private static readonly ParticleSorter sorter = new ParticleSorter();
        private readonly ParticleSystemComponent systemInfo;
        private readonly Particle[] particles;
        private float numOfParticlesNeeded;

        public ParticleSystem(ParticleSystemComponent systemInfo)
        {
            this.systemInfo = systemInfo;
            ParticlesPerSecond = systemInfo.ParticlesPerSecond;
            particles = new Particle[systemInfo.MaxParticles];
            for (int i = 0; i < particles.Length; i++)
                particles[i] = new Particle();
        }
        
        public int AliveParticles { get; private set; }

        public Vector3i Location { get; set; }

        public int ParticlesPerSecond { get; set; }

        public bool IsAlive { get; set; }

        public void Reset(Vector3i worldLoc)
        {
            Location = worldLoc;
            for (int i = 0; i < particles.Length; i++)
                particles[i].Time = 0.0f;
        }

        public void Update(float timeSinceLastFrame, Vector3us[] locationArray, Color4b[] colorArray, 
            IGameWorldRenderer renderer, ref int dataIndex)
        {
            Debug.Assert(IsAlive);

            ParticleSystemComponent sysInfo = systemInfo;
            ParticleController upd = sysInfo.Controller;
            upd.BeginUpdate(this, timeSinceLastFrame);

            int aliveParticles = AliveParticles;
            numOfParticlesNeeded += ParticlesPerSecond * timeSinceLastFrame;
            int newParticleCount = Math.Min((int)numOfParticlesNeeded, systemInfo.MaxParticles - aliveParticles);
            numOfParticlesNeeded -= newParticleCount;
            Vector3i worldSize = renderer.GameWorld.VoxelSize;
            LightProvider lightProvider = renderer.LightProvider;
            Particle[] particleList = particles;
            bool isLit = systemInfo.IsLit;
            
            float locX = Location.X;
            float locY = Location.Y;
            float locZ = Location.Z;
            for (int i = 0; i < aliveParticles; i++)
            {
                Particle part = particleList[i];
                if (part.Time > 0.0f)
                {
                    // Normal case - particle is still alive, so just update it
                    upd.Update(part, timeSinceLastFrame, locX, locY, locZ);
                }
                else if (newParticleCount > 0)
                {
                    // Particle died, but we need new particles so just re-initialize this one
                    upd.InitializeNew(part, locX, locY, locZ);
                    newParticleCount--;
                }
                else
                {
                    // Particle died - replace with an existing alive particle
                    int lastAliveIndex = aliveParticles - 1;
                    Particle lastAlive = particleList[lastAliveIndex];
                    particleList[lastAliveIndex] = part;
                    particleList[i] = lastAlive;
                    part = lastAlive;
                    aliveParticles--;
                    // Just replaced current dead particle with an alive one. Need to update it.
                    upd.Update(part, timeSinceLastFrame, locX, locY, locZ);
                }
            }

            // Intialize any new particles that are still needed
            for (int i = 0; i < newParticleCount; i++)
            {
                Particle part = particleList[aliveParticles];
                upd.InitializeNew(part, locX, locY, locZ);
                aliveParticles++;
            }

            if (sysInfo.TransparencyType == TransparencyType.Realistic)
            {
                sorter.CameraLocation = new Vector3i((int)renderer.Camera.Location.X, (int)renderer.Camera.Location.Y, (int)renderer.Camera.Location.Z);
                Array.Sort(particleList, sorter);
            }

            for (int i = 0; i < aliveParticles; i++)
            {
                Particle part = particleList[i];
                if (part.X < 0.0f || part.Y < 0.0f || part.Z < 0.0f)
                    continue;

                ushort partX = (ushort)part.X;
                ushort partY = (ushort)part.Y;
                ushort partZ = (ushort)part.Z;
                locationArray[dataIndex] = new Vector3us(partX, partY, partZ);

                colorArray[dataIndex] = isLit ? CalculateParticleColor(partX, partY, partZ, part.Color, worldSize, lightProvider) : part.Color;
                dataIndex++;
            }

            AliveParticles = aliveParticles;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Color4b CalculateParticleColor(int partX, int partY, int partZ, Color4b color, Vector3i worldSize, LightProvider lightProvider)
        {
            Color3f lightColor;
            if (partX < 0 || partX >= worldSize.X || partY < 0 || partY >= worldSize.Y || partZ < 0 || partZ >= worldSize.Z)
                lightColor = lightProvider.AmbientLight;
            else
                lightColor = lightProvider.GetLightAtFast(partX, partY, partZ);

            return new Color4b((byte)Math.Min(255, (int)(color.R * lightColor.R)), (byte)Math.Min(255, (int)(color.G * lightColor.G)),
                (byte)Math.Min(255, (int)(color.B * lightColor.B)), color.A);
        }

        #region ParticleSorter class
        private class ParticleSorter : IComparer<Particle>
        {
            public Vector3i CameraLocation;

            public int Compare(Particle p1, Particle p2)
            {
                if (p1.Time <= 0.0f && p2.Time <= 0.0f)
                    return 0;

                if (p1.Time <= 0.0f)
                    return 1;

                if (p2.Time <= 0.0f)
                    return -1;

                int p1DistX = (int)p1.X - CameraLocation.X;
                int p1DistY = (int)p1.Y - CameraLocation.Y;
                int p1DistZ = (int)p1.Z - CameraLocation.Z;
                int p1DistSquared = p1DistX * p1DistX + p1DistY * p1DistY + p1DistZ * p1DistZ;

                int p2DistX = (int)p2.X - CameraLocation.X;
                int p2DistY = (int)p2.Y - CameraLocation.Y;
                int p2DistZ = (int)p2.Z - CameraLocation.Z;
                int p2DistSquared = p2DistX * p2DistX + p2DistY * p2DistY + p2DistZ * p2DistZ;
                return p2DistSquared - p1DistSquared;
            }
        }
        #endregion
    }
}
