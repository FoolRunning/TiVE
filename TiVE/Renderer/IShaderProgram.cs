using OpenTK;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal enum ShaderType
    {
        Vertex,
        Fragment,
        Geometry,
    }

    internal interface IShaderProgram
    {
        void AddShader(string shaderSource, ShaderType shaderType);

        void AddAttribute(string name);

        void AddKnownUniform(string name);

        void Delete();

        void Bind();

        bool Initialize();

        void SetUniform(string name, ref Matrix4 value);

        void SetUniform(string name, ref Vector3 value);

        void SetUniform(string name, ref Vector4 value);
    }
}
