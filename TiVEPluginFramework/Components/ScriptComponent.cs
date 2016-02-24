using System;
using System.IO;
using JetBrains.Annotations;

namespace ProdigalSoftware.TiVEPluginFramework.Components
{
    /// <summary>
    /// Component for entities that are controled with a Lua script.
    /// </summary>
    [PublicAPI]
    public sealed class ScriptComponent : IComponent
    {
        public static readonly Guid ID = new Guid("2477E7A2-A954-4C16-8413-35E7A6672AE2");
        private const byte SerializedFileVersion = 1;

        #region Internal data
        internal bool Loaded;
        #endregion

        public readonly string ScriptName;

        public ScriptComponent(BinaryReader reader)
        {
            byte fileVersion = reader.ReadByte();
            if (fileVersion > SerializedFileVersion)
                throw new FileTooNewException("ScriptComponent");

            ScriptName = reader.ReadString();
        }

        public ScriptComponent(string scriptName)
        {
            ScriptName = scriptName;
        }

        public void SaveTo(BinaryWriter writer)
        {
            writer.Write(SerializedFileVersion);
            writer.Write(ScriptName);
        }
    }
}
