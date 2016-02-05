using System;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Core.Backend
{
    internal enum ShaderType
    {
        Vertex,
        Fragment,
        Geometry,
    }

    internal interface IShaderProgram : IDisposable
    {
        void AddShader(string shaderSource, ShaderType shaderType);

        void AddAttribute(string name);

        void AddKnownUniform(string name);

        void Bind();

        void Unbind();

        bool IsInitialized { get; }

        bool Initialize();

        void SetUniform(string name, ref Matrix4f value);

        void SetUniform(string name, ref Vector3f value);

        void SetUniform(string name, ref Color4f value);
    }
}
