using OpenTK.Graphics.OpenGL;
using System.Diagnostics;
using OpenTK;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal sealed class ShaderProgram
    {
        private readonly string vertexShaderSource;
        private readonly string fragmentShaderSource;
        private readonly string geometryShaderSource;

        private int programId;
        private int matrixMVPLocation;
        private int matrixModelLocation;
        private int matrixViewLocation;
        private int matrixProjectionLocation;

        public ShaderProgram(string vertexShaderSource, string fragmentShaderSource, string geometryShaderSource)
        {
            this.vertexShaderSource = vertexShaderSource;
            this.fragmentShaderSource = fragmentShaderSource;
            this.geometryShaderSource = geometryShaderSource;
        }

        ~ShaderProgram()
        {
            Debug.Assert(programId == 0, "Shader was not properly deleted");
        }

        public void Delete()
        {
            GL.DeleteProgram(programId);
            programId = 0;
        }

        public void Bind()
        {
            GL.UseProgram(programId);
        }

        public bool Initialize()
        {
            programId = GL.CreateProgram();

            int vertexId = GL.CreateShader(ShaderType.VertexShader);
            int fragmentId = GL.CreateShader(ShaderType.FragmentShader);
            int geometryId = 0;

            bool success = CompileShader(vertexId, vertexShaderSource);
            success &= CompileShader(fragmentId, fragmentShaderSource);

            if (!string.IsNullOrEmpty(geometryShaderSource))
            {
                geometryId = GL.CreateShader(ShaderType.GeometryShader);
                success &= CompileShader(geometryId, geometryShaderSource);
            }

            if (success)
            {
                GL.AttachShader(programId, vertexId);
                GL.AttachShader(programId, fragmentId);
                if (geometryId != 0)
                    GL.AttachShader(programId, geometryId);

                GL.BindAttribLocation(programId, 0, "in_Position");
                GL.BindAttribLocation(programId, 1, "in_Color");

                GL.LinkProgram(programId);

                string info;
                GL.GetProgramInfoLog(programId, out info);
                if (!string.IsNullOrEmpty(info))
                    Debug.WriteLine(info);

                int linkResult;
                GL.GetProgram(programId, ProgramParameter.LinkStatus, out linkResult);
                success = (linkResult == 1);
            }

            if (success)
            {
                matrixMVPLocation = GL.GetUniformLocation(programId, "matrix_ModelViewProjection");
                matrixModelLocation = GL.GetUniformLocation(programId, "matrix_Model");
                matrixViewLocation = GL.GetUniformLocation(programId, "matrix_View");
                matrixProjectionLocation = GL.GetUniformLocation(programId, "matrix_Projection");
            }

            if (vertexId != 0)
                GL.DeleteShader(vertexId);
            if (fragmentId != 0)
                GL.DeleteShader(fragmentId);
            if (geometryId != 0)
                GL.DeleteShader(geometryId);
            GlUtils.CheckGLErrors();

            return success;
        }

        public void SetMVPMatrix(ref Matrix4 matrixModel, ref Matrix4 matrixView, ref Matrix4 matrixProjection)
        {
            GL.UniformMatrix4(matrixModelLocation, false, ref matrixModel);
            GL.UniformMatrix4(matrixViewLocation, false, ref matrixView);
            GL.UniformMatrix4(matrixProjectionLocation, false, ref matrixProjection);
        }

        public void SetMVPMatrix(ref Matrix4 matrixMVP)
        {
            GL.UniformMatrix4(matrixMVPLocation, false, ref matrixMVP);
        }

        private static bool CompileShader(int shaderId, string shaderSource)
        {
            GL.ShaderSource(shaderId, shaderSource);
            GL.CompileShader(shaderId);

            string info;
            GL.GetShaderInfoLog(shaderId, out info);
            
            if (!string.IsNullOrEmpty(info))
                Debug.WriteLine(info);

            int compileResult;
            GL.GetShader(shaderId, ShaderParameter.CompileStatus, out compileResult);
            if (compileResult != 1)
            {
                Debug.WriteLine("Compile Error!");
                Debug.WriteLine(shaderSource);
                return false;
            }
            return true;
        }
    }
}
