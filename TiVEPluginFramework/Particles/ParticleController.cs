namespace ProdigalSoftware.TiVEPluginFramework.Particles
{
    /// <summary>
    /// Controls the particles in a <see cref="IParticleSystem"/> for each frame
    /// </summary>
    public abstract class ParticleController
    {
        /// <summary>
        /// Called right before the specified <see cref="IParticleSystem"/> is to be updated by this controller.
        /// </summary>
        /// <param name="particleSystem">The <see cref="IParticleSystem"/> that is to be updated</param>
        /// <param name="timeSinceLastUpdate">The time (in seconds) since the last update.</param>
        /// <returns><c>True</c> if the specified <see cref="IParticleSystem"/> should be updated and drawn; <c>false</c> to skip the 
        /// update and the drawing of the specified <see cref="IParticleSystem"/></returns>
        public abstract bool BeginUpdate(IParticleSystem particleSystem, float timeSinceLastUpdate);

        /// <summary>
        /// Called to update the specified <see cref="Particle"/>
        /// </summary>
        /// <param name="particle">The <see cref="Particle"/> to update</param>
        /// <param name="timeSinceLastUpdate">The time (in seconds) since the last update.</param>
        /// <param name="systemX">The location on the x-axis of the <see cref="IParticleSystem"/> that is being updated</param>
        /// <param name="systemY">The location on the y-axis of the <see cref="IParticleSystem"/> that is being updated</param>
        /// <param name="systemZ">The location on the z-axis of the <see cref="IParticleSystem"/> that is being updated</param>
        public abstract void Update(Particle particle, float timeSinceLastUpdate, float systemX, float systemY, float systemZ);

        /// <summary>
        /// Called to initialize the specified <see cref="Particle"/> for the first time.
        /// </summary>
        /// <param name="particle">The <see cref="Particle"/> to initialize</param>
        /// <param name="systemX">The location on the x-axis of the <see cref="IParticleSystem"/> that is being updated</param>
        /// <param name="systemY">The location on the y-axis of the <see cref="IParticleSystem"/> that is being updated</param>
        /// <param name="systemZ">The location on the z-axis of the <see cref="IParticleSystem"/> that is being updated</param>
        public abstract void InitializeNew(Particle particle, float systemX, float systemY, float systemZ);

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
