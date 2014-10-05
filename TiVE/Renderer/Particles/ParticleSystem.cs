﻿using System;
using System.Runtime.CompilerServices;
using OpenTK;
using ProdigalSoftware.TiVE.Resources;
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
            Location = new Vector3(worldX, worldY, worldZ);
            ParticlesPerSecond = systemInfo.ParticlesPerSecond;
        }

        public ParticleSystemInformation SystemInformation
        {
            get { return systemInfo; }
        }

        public int AliveParticles { get; private set; }

        public Vector3 Location { get; set; }

        public int ParticlesPerSecond { get; set; }

        public void Update(float timeSinceLastFrame, Particle[] particleList, Vector3s[] locationArray, Color4b[] colorArray, ref int dataIndex)
        {
            ParticleSystemInformation sysInfo = systemInfo;
            ParticleController upd = sysInfo.Controller;
            upd.BeginUpdate(this, timeSinceLastFrame);

            int aliveParticles = AliveParticles;
            numOfParticlesNeeded += ParticlesPerSecond * timeSinceLastFrame;
            int newParticleCount = Math.Min((int)numOfParticlesNeeded, systemInfo.MaxParticles - aliveParticles);
            numOfParticlesNeeded -= newParticleCount;

            LightManager lightManager = ResourceManager.LightManager;

            float locX = Location.X;
            float locY = Location.Y;
            float locZ = Location.Z;
            for (int i = 0; i < aliveParticles; i++)
            {
                Particle part = particleList[i];
                if (part.Time > 0.0f)
                    upd.Update(part, timeSinceLastFrame, locX, locY, locZ);
                else if (newParticleCount > 0)
                {
                    // Particle died, but we need new particles so just re-initialize this one
                    upd.InitializeNew(part, locX, locY, locZ);
                    newParticleCount--;
                }
                else
                {
                    // Particle died replace with an existing alive particle
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
                colorArray[dataIndex] = systemInfo.IsLit ? CalculateParticleColor(partX, partY, partZ, part.Color, lightManager) : part.Color;
                dataIndex++;
            }

            for (int i = 0; i < newParticleCount; i++)
            {
                Particle part = particleList[aliveParticles];
                upd.InitializeNew(part, locX, locY, locZ);
                short partX = (short)part.X;
                short partY = (short)part.Y;
                short partZ = (short)part.Z;
                locationArray[dataIndex] = new Vector3s(partX, partY, partZ);
                colorArray[dataIndex] = systemInfo.IsLit ? CalculateParticleColor(partX, partY, partZ, part.Color, lightManager) : part.Color;
                dataIndex++;
                aliveParticles++;
            }

            AliveParticles = aliveParticles;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Color4b CalculateParticleColor(short partX, short partY, short partZ, Color4b color, LightManager lightManager)
        {
            float percentR;
            float percentG;
            float percentB;
            lightManager.GetLightAt(partX, partY, partZ, out percentR, out percentG, out percentB);
            return new Color4b((byte)Math.Min(255, color.R * percentR), (byte)Math.Min(255, color.G * percentG),
                (byte)Math.Min(255, color.B * percentB), 255);
        }
    }
}
