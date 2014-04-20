using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public abstract class ParticleController
    {
        public abstract bool BeginUpdate(IParticleSystem particleSystem, float timeSinceLastFrame);

        public abstract void Update(Particle particle, float timeSinceLastFrame, float systemX, float systemY, float systemZ);

        public abstract void InitializeNew(Particle particle, float startX, float startY, float startZ);

        protected static void ApplyVelocity(Particle particle, float timeSinceLastFrame)
        {
            particle.X += (particle.VelX * timeSinceLastFrame);
            particle.Y += (particle.VelY * timeSinceLastFrame);
            particle.Z += (particle.VelZ * timeSinceLastFrame);
        }
    }
}
