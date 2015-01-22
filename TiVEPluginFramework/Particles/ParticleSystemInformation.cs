using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework.Particles
{
    public enum TransparencyType
    {
        None,
        Realistic,
        Additive
    }

    public sealed class ParticleSystemInformation
    {
        public readonly uint[, ,] ParticleVoxels;
        public readonly ParticleController Controller;
        public readonly int ParticlesPerSecond;
        public readonly int MaxParticles;
        public readonly Vector3b Location;
        public readonly TransparencyType TransparencyType;
        public readonly bool IsLit;

        public ParticleSystemInformation(uint[,,] particleVoxels, ParticleController controller, Vector3b location,
            int particlesPerSecond, int maxParticles, TransparencyType transparencyType, bool isLit)
        {
            ParticleVoxels = particleVoxels;
            Controller = controller;
            Location = location;
            ParticlesPerSecond = particlesPerSecond;
            MaxParticles = maxParticles;
            TransparencyType = transparencyType;
            IsLit = isLit;
        }
    }
}
