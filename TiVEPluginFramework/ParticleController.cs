using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public enum TransparencyType
    {
        None = 0,
        Additive = 1,
        Realistic = 2
    }

    /// <summary>
    /// Controls a set of particles for each frame
    /// </summary>
    public abstract class ParticleController
    {
        private readonly TransparencyType transparencyType;
        private readonly int maxParticles;
        private readonly int particlesPerSecond;
        private readonly bool isLit;

        protected ParticleController(int maxParticles, int particlesPerSecond, TransparencyType transparencyType, bool isLit)
        {
            this.maxParticles = maxParticles;
            this.transparencyType = transparencyType;
            this.particlesPerSecond = particlesPerSecond;
            this.isLit = isLit;
        }

        public bool IsLit
        {
            get { return isLit; }
        }

        public TransparencyType TransparencyType
        {
            get { return transparencyType; }
        }

        public int MaxParticles
        {
            get { return maxParticles; }
        }

        public int ParticlesPerSecond
        {
            get { return particlesPerSecond; }
        }

        public abstract Voxel[, ,] ParticleVoxels { get; }

        public abstract void Update(Particle particle, float timeSinceLastUpdate, Vector3i location);

        public abstract void InitializeNew(Particle particle, Vector3i location);

        /// <summary>
        /// Helper method to apply the current velocity of the specified particle to itself.
        /// </summary>
        /// <param name="particle">The <see cref="Particle"/></param>
        /// <param name="timeSinceLastUpdate">The time (in seconds) since the last update</param>
        protected static void ApplyVelocity(Particle particle, float timeSinceLastUpdate)
        {
            particle.X += (particle.VelX * timeSinceLastUpdate);
            particle.Y += (particle.VelY * timeSinceLastUpdate);
            particle.Z += (particle.VelZ * timeSinceLastUpdate);
        }
    }
}
