using System;
using OpenTK;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public sealed class ParticleSystemInformation
    {
        public readonly ParticleController Controller;
        public readonly int ParticlesPerSecond;
        public readonly int MaxParticles;
        public readonly Vector3b Location;

        public ParticleSystemInformation(ParticleController controller, Vector3b location, int particlesPerSecond, int maxParticles)
        {
            Controller = controller;
            Location = location;
            ParticlesPerSecond = particlesPerSecond;
            MaxParticles = maxParticles;
        }
    }

    public interface IParticleSystem
    {
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
