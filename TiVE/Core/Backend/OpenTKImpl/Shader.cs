using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE.Core.Backend.OpenTKImpl
{
    internal sealed class Shader : IDisposable
    {
        private readonly string shaderSource;
        private readonly ShaderType shaderType;

        public Shader(string shaderSource, ShaderType shaderType)
        {
            this.shaderSource = shaderSource;
            this.shaderType = shaderType;
        }

        ~Shader()
        {
            Messages.Assert(ShaderId == 0, shaderType + " shader was not properly deleted");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (ShaderId != 0)
                GL.DeleteShader(ShaderId);
            ShaderId = 0;

            GC.SuppressFinalize(this);
        }

        public int ShaderId { get; private set; }

        public bool Compile()
        {
            ShaderId = GL.CreateShader(GLShaderType);

            GL.ShaderSource(ShaderId, shaderSource);
            GL.CompileShader(ShaderId);

            string info = GL.GetShaderInfoLog(ShaderId);

            if (!string.IsNullOrEmpty(info))
                Messages.AddWarning(info);

            int compileResult;
            GL.GetShader(ShaderId, ShaderParameter.CompileStatus, out compileResult);
            if (compileResult != 1)
            {
                Messages.AddWarning(shaderType + " shader compile error!");
                Debug.WriteLine(shaderSource);
                Dispose();
                return false;
            }
            return true;
        }

        private OpenTK.Graphics.OpenGL4.ShaderType GLShaderType
        {
            get
            {
                switch (shaderType)
                {
                    case ShaderType.Vertex: return OpenTK.Graphics.OpenGL4.ShaderType.VertexShader;
                    case ShaderType.Fragment: return OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader;
                    case ShaderType.Geometry: return OpenTK.Graphics.OpenGL4.ShaderType.GeometryShader;
                    default: throw new InvalidOperationException("Unknown shader type: " + shaderType);
                }
            }
        }
    }
}
