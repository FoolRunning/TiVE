using System;
using System.IO;
using JetBrains.Annotations;

namespace ProdigalSoftware.TiVEPluginFramework.Components
{
    /// <summary>
    /// Component for entities that emit particles
    /// </summary>
    [PublicAPI]
    public sealed class ParticleComponent : IBlockComponent, IComponent
    {
        public static readonly Guid ID = new Guid("005EED41-2987-446D-8717-5851DED31749");
        private const byte SerializedFileVersion = 1;

        public readonly string ControllerName;
        public Vector3i Location;
        public bool IsAlive = true;

        public ParticleComponent(BinaryReader reader)
        {
            byte fileVersion = reader.ReadByte();
            if (fileVersion > SerializedFileVersion)
                throw new FileTooNew("ParticleComponent");

            ControllerName = reader.ReadString();
            Location = new Vector3i(reader);
        }

        public ParticleComponent(string controllerName, Vector3i location = new Vector3i())
        {
            ControllerName = controllerName;
            Location = location;
        }

        #region Implementation of ITiVESerializable
        public void SaveTo(BinaryWriter writer)
        {
            writer.Write(SerializedFileVersion);
            writer.Write(ControllerName);
            Location.SaveTo(writer);
        }
        #endregion
    }
}
