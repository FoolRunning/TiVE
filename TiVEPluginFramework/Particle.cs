using JetBrains.Annotations;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework
{
    /// <summary>
    /// Represents a single particle
    /// </summary>
    public sealed class Particle
    {
        /// <summary>Location of this particle on the x-axis</summary>
        [UsedImplicitly] public float X;
        /// <summary>Location of this particle on the y-axis</summary>
        [UsedImplicitly] public float Y;
        /// <summary>Location of this particle on the z-axis</summary>
        [UsedImplicitly] public float Z;
        /// <summary>Current velocity of this particle on the x-axis</summary>
        [UsedImplicitly] public float VelX;
        /// <summary>Current velocity of this particle on the y-axis</summary>
        [UsedImplicitly] public float VelY;
        /// <summary>Current velocity of this particle on the z-axis</summary>
        [UsedImplicitly] public float VelZ;
        /// <summary>Current color of this particle</summary>
        [UsedImplicitly] public Color4b Color;
        /// <summary>
        /// Used to determine when a particle is dead. A value of 0.0 or less will result in the particle being killed.
        /// Typically a <see cref="ParticleController"/> will set this value to the amount of time the particle has left to live.
        /// </summary>
        [UsedImplicitly] public float Time;
    }
}
