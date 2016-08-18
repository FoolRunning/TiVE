using System;
using System.IO;
using JetBrains.Annotations;

namespace ProdigalSoftware.TiVEPluginFramework
{
    /// <summary>
    /// Interface for objects that can be serialized by TiVE.
    /// In addition to implementing this interface, objects must have a public or internal constructor that takes (as it's only argument) 
    /// a <see cref="BinaryReader"/> object in order to deserialize and a public static field named ID of type <see cref="Guid"/>.
    /// </summary>
    [PublicAPI]
    public interface ITiVESerializable
    {
        /// <summary>
        /// Saves the object to the specified writer
        /// </summary>
        [Pure]
        void SaveTo(BinaryWriter writer);
    }
}
