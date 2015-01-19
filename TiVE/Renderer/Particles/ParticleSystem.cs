﻿using System;
using System.Runtime.CompilerServices;
using ProdigalSoftware.TiVE.Renderer.Lighting;
using ProdigalSoftware.TiVEPluginFramework.Particles;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.Particles
{
    /// <summary>
    /// Represents a single particle emitter. Responsible for updating all particles owned by itself.
    /// </summary>
    internal sealed class ParticleSystem : IParticleSystem
    {
        private readonly ParticleSystemInformation systemInfo;
        private float numOfParticlesNeeded;

        public ParticleSystem(ParticleSystemInformation systemInfo, int worldX, int worldY, int worldZ)
        {
            this.systemInfo = systemInfo;
            Location = new Vector3i(worldX, worldY, worldZ);
            ParticlesPerSecond = systemInfo.ParticlesPerSecond;
        }

        public ParticleSystemInformation SystemInformation
        {
            get { return systemInfo; }
        }

        public int AliveParticles { get; private set; }

        public Vector3i Location { get; set; }

        public int ParticlesPerSecond { get; set; }

        public void Update(float timeSinceLastFrame, Particle[] particleList, Vector3s[] locationArray, Color4b[] colorArray, 
            IGameWorldRenderer renderer, ref int dataIndex)
        {
            ParticleSystemInformation sysInfo = systemInfo;
            ParticleController upd = sysInfo.Controller;
            upd.BeginUpdate(this, timeSinceLastFrame);

            int aliveParticles = AliveParticles;
            numOfParticlesNeeded += ParticlesPerSecond * timeSinceLastFrame;
            int newParticleCount = Math.Min((int)numOfParticlesNeeded, systemInfo.MaxParticles - aliveParticles);
            numOfParticlesNeeded -= newParticleCount;
            Vector3i worldSize = renderer.GameWorld.VoxelSize;
            LightProvider lightProvider = renderer.LightProvider;
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

                short partX = (short)part.X;
                short partY = (short)part.Y;
                short partZ = (short)part.Z;
                locationArray[dataIndex] = new Vector3s(partX, partY, partZ);

                colorArray[dataIndex] = isLit ? CalculateParticleColor(partX, partY, partZ, part.Color, worldSize, lightProvider) : part.Color;
                dataIndex++;
            }

            // Intialize any new particles that are still needed
            for (int i = 0; i < newParticleCount; i++)
            {
                Particle part = particleList[aliveParticles];
                upd.InitializeNew(part, locX, locY, locZ);
                
                short partX = (short)part.X;
                short partY = (short)part.Y;
                short partZ = (short)part.Z;
                locationArray[dataIndex] = new Vector3s(partX, partY, partZ);
                colorArray[dataIndex] = isLit ? CalculateParticleColor(partX, partY, partZ, part.Color, worldSize, lightProvider) : part.Color;

                dataIndex++;
                aliveParticles++;
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
                (byte)Math.Min(255, (int)(color.B * lightColor.B)), 255);
        }
    }
}
