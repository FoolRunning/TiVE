using System;
using OpenTK;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public sealed class ParticleSystemInformation
    {
        public readonly uint[, ,] ParticleVoxels;
        public readonly ParticleController Controller;
        public readonly int ParticlesPerSecond;
        public readonly int MaxParticles;
        public readonly Vector3b Location;
        public readonly bool TransparentParticles;

        public ParticleSystemInformation(uint[,,] particleVoxels, ParticleController controller, Vector3b location, 
            int particlesPerSecond, int maxParticles, bool transparentParticles)
        {
            ParticleVoxels = particleVoxels;
            Controller = controller;
            Location = location;
            ParticlesPerSecond = particlesPerSecond;
            MaxParticles = maxParticles;
            TransparentParticles = transparentParticles;
        }
    }

    public interface IParticleSystem
    {
        int AliveParticles { get; }
        Vector3 Location { get; set; }
        int ParticlesPerSecond { get; set; }
    }

    public sealed class Particle
    {
        public float X;
        public float Y;
        public float Z;
        
        public float VelX;
        public float VelY;
        public float VelZ;

        public Color4b Color;

        public float Time;
    }
}
