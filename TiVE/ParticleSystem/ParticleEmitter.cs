using System;
using System.Collections.Generic;
using ProdigalSoftware.TiVE.RenderSystem.Lighting;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.ParticleSystem
{
    /// <summary>
    /// Represents a single particle emitter. Responsible for updating all particles owned by itself.
    /// </summary>
    internal sealed class ParticleEmitter
    {
        private static readonly ParticleSorter sorter = new ParticleSorter();

        private readonly ParticleController controller;
        private readonly Particle[] particles;
        private float numOfParticlesNeeded;
        private int aliveParticles;

        public ParticleEmitter(ParticleController controller)
        {
            this.controller = controller;
            particles = new Particle[controller.MaxParticles];
            for (int i = 0; i < particles.Length; i++)
                particles[i] = new Particle();
        }

        public Vector3i Location { get; set; }
        
        public bool InUse { get; set; }

        public void Reset()
        {
            for (int i = 0; i < particles.Length; i++)
                particles[i].Time = 0.0f;
        }

        public void UpdateInternal(Vector3i cameraLocation, float timeSinceLastUpdate)
        {
            numOfParticlesNeeded += controller.ParticlesPerSecond * timeSinceLastUpdate;
            int newParticleCount = Math.Min((int)numOfParticlesNeeded, particles.Length - aliveParticles);
            numOfParticlesNeeded -= newParticleCount;

            Vector3i location = Location;
            for (int i = 0; i < aliveParticles; i++)
            {
                Particle part = particles[i];
                if (part.Time > 0.0f)
                {
                    // Normal case - particle is still alive, so just update it
                    controller.Update(part, timeSinceLastUpdate, location);
                }
                else if (newParticleCount > 0)
                {
                    // Particle died, but we need new particles so just re-initialize this one
                    controller.InitializeNew(part, location);
                    newParticleCount--;
                }
                else
                {
                    // Particle died - replace with an existing alive particle
                    int lastAliveIndex = aliveParticles - 1;
                    Particle lastAlive = particles[lastAliveIndex];
                    particles[lastAliveIndex] = part;
                    particles[i] = lastAlive;
                    part = lastAlive;
                    aliveParticles--;
                    // Just replaced current dead particle with an alive one. Need to update it.
                    controller.Update(part, timeSinceLastUpdate, location);
                }
            }

            // Intialize any new particles that are still needed
            for (int i = 0; i < newParticleCount; i++)
            {
                Particle part = particles[aliveParticles];
                controller.InitializeNew(part, location);
                aliveParticles++;
            }

            if (controller.TransparencyType == TransparencyType.Realistic)
            {
                sorter.CameraLocation = cameraLocation;
                Array.Sort(particles, sorter);
            }
        }

        public void AddToArrays(Vector3i worldSize, LightProvider lightProvider, Vector3us[] locationArray, Color4b[] colorArray, ref int dataIndex)
        {
            bool isLit = controller.IsLit;
            for (int i = 0; i < aliveParticles; i++)
            {
                Particle part = particles[i];
                if (part.X < 0.0f || part.Y < 0.0f || part.Z < 0.0f)
                    continue; // Can't be cast to a ushort

                ushort partX = (ushort)part.X;
                ushort partY = (ushort)part.Y;
                ushort partZ = (ushort)part.Z;
                locationArray[dataIndex] = new Vector3us(partX, partY, partZ);
                if (!isLit)
                    colorArray[dataIndex] = part.Color;
                else
                {
                    Color3f lightColor;
                    if (partX >= worldSize.X || partY >= worldSize.Y || partZ >= worldSize.Z)
                        lightColor = lightProvider.AmbientLight;
                    else
                        lightColor = lightProvider.GetLightAtFast(partX, partY, partZ);

                    colorArray[dataIndex] = new Color4b(
                        (byte)Math.Min(255, (int)(part.Color.R * lightColor.R)),
                        (byte)Math.Min(255, (int)(part.Color.G * lightColor.G)),
                        (byte)Math.Min(255, (int)(part.Color.B * lightColor.B)), part.Color.A);
                }
                dataIndex++;
            } 
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
