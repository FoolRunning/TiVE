using System;
using OpenTK;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.Particles
{
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
            ParticleController upd = systemInfo.Controller;
            upd.BeginUpdate(this, timeSinceLastFrame);

            int aliveParticles = AliveParticles;
            numOfParticlesNeeded += ParticlesPerSecond * timeSinceLastFrame;
            int newParticleCount = Math.Min((int)numOfParticlesNeeded, systemInfo.MaxParticles - aliveParticles);
            numOfParticlesNeeded -= newParticleCount;

            float locX = Location.X;
            float locY = Location.Y;
            float locZ = Location.Z;
            int dataStartIndex = dataIndex;
            for (int pi = 0; pi < aliveParticles + newParticleCount; pi++)
            {
                Particle part = particleList[pi];
                if (part.Time > 0.0f)
                    upd.Update(part, timeSinceLastFrame, locX, locY, locZ);
                else if (newParticleCount > 0)
                {
                    // We need new particles, just re-initialize this one
                    upd.InitializeNew(part, locX, locY, locZ);
                    newParticleCount--;
                    aliveParticles++;
                }
                else
                {
                    // Particle died replace with an existing alive particle
                    int lastAliveIndex = aliveParticles - 1;
                    Particle lastAlive = particleList[lastAliveIndex];
                    particleList[lastAliveIndex] = part;
                    particleList[pi] = lastAlive;
                    part = lastAlive;
                    aliveParticles--;
                    // Just replaced current particle with another one. Need to update it.
                    upd.Update(part, timeSinceLastFrame, locX, locY, locZ);
                }

                locationArray[dataIndex] = new Vector3s((short)part.X, (short)part.Y, (short)part.Z);
                colorArray[dataIndex] = part.Color;
                dataIndex++;
            }

            AliveParticles = dataIndex - dataStartIndex;
        }
    }
}
