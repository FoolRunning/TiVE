using System;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Core.Backend
{
    internal enum ShaderType
    {
        Vertex,
        Fragment,
        Geometry,
    }

    internal abstract class ShaderProgram : IDisposable
    {
        private static bool cubifyVoxels;

        static ShaderProgram()
        {
            cubifyVoxels = TiVEController.UserSettings.Get(UserSettings.CubifyVoxelsKey);
            TiVEController.UserSettings.SettingChanged += UserSettings_SettingChanged;
        }

        /// <summary>
        /// Gets a voxel helper for the specified settings
        /// </summary>
        public static string GetShaderName(bool forInstances)
        {
            if (cubifyVoxels)
                return forInstances ? "ShadedInstanced" : "ShadedNonInstanced";

            return forInstances ? "NonShadedInstanced" : "NonShadedNonInstanced";
        }

        public abstract void Dispose();

        public abstract void AddShader(string shaderSource, ShaderType shaderType);

        public abstract void AddAttribute(string name);

        public abstract void AddKnownUniform(string name);

        public abstract void Bind();

        public abstract void Unbind();

        public abstract bool IsInitialized { get; }

        public abstract bool Initialize();

        public abstract void SetUniform(string name, ref Matrix4f value);

        public abstract void SetUniform(string name, ref Vector3f value);

        public abstract void SetUniform(string name, ref Color4f value);

        public abstract void SetUniform(string name, RenderedLight[] value);

        public abstract void SetUniform(string name, int value);

        private static void UserSettings_SettingChanged(string settingName, Setting newValue)
        {
            if (settingName == UserSettings.CubifyVoxelsKey)
                cubifyVoxels = newValue;
        }
    }
}
