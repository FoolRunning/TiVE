using System.IO;
using JetBrains.Annotations;

namespace ProdigalSoftware.TiVEPluginFramework
{
    /// <summary>
    /// Handles the serialization and deserialization of objects that implement the <see cref="ITiVESerializable"/> interface
    /// </summary>
    [PublicAPI]
    public static class TiVESerializer
    {
        internal static ITiVESerializerImpl Implementation;

        /// <summary>
        /// Serializes the specified object to the specifed writer
        /// </summary>
        public static void Serialize(ITiVESerializable obj, BinaryWriter writer)
        {
            Implementation.Serialize(obj, writer);
        }

        /// <summary>
        /// Deserializes the object at the current location in the specified reader and returns it as the specified type
        /// </summary>
        public static T Deserialize<T>(BinaryReader reader) where T : ITiVESerializable
        {
            return Implementation.Deserialize<T>(reader);
        }
    }

    internal interface ITiVESerializerImpl
    {
        void Serialize(ITiVESerializable obj, BinaryWriter writer);

        T Deserialize<T>(BinaryReader reader) where T : ITiVESerializable;
    }
}
