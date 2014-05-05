﻿using OpenTK;

namespace ProdigalSoftware.TiVEPluginFramework.Particles
{
    /// <summary>
    /// Represents a collection of <see cref="Particle"/>s generated by the game engine based off of a <see cref="ParticleSystemInformation"/> definition
    /// </summary>
    public interface IParticleSystem
    {
        /// <summary>
        /// Gets the current number of <see cref="Particle"/>s that are currently alive
        /// </summary>
        int AliveParticles { get; }

        /// <summary>
        /// Gets/sets the current location in the game world of this particle system
        /// </summary>
        Vector3 Location { get; set; }

        /// <summary>
        /// Gets/sets the current number of <see cref="Particle"/>s that should be created each second
        /// </summary>
        int ParticlesPerSecond { get; set; }
    }
}
