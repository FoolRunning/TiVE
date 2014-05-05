using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework.Particles
{
    public sealed class ParticleSystemInformation
    {
        public readonly uint[, ,] ParticleVoxels;
        public readonly ParticleController Controller;
        public readonly int ParticlesPerSecond;
        public readonly int MaxParticles;
        public readonly Vector3b Location;
        public readonly bool TransparentParticles;
        public readonly bool IsLit;

        public ParticleSystemInformation(uint[,,] particleVoxels, ParticleController controller, Vector3b location, 
            int particlesPerSecond, int maxParticles, bool transparentParticles, bool isLit)
        {
            ParticleVoxels = particleVoxels;
            Controller = controller;
            Location = location;
            ParticlesPerSecond = particlesPerSecond;
            MaxParticles = maxParticles;
            TransparentParticles = transparentParticles;
            IsLit = isLit;
        }
    }
}
