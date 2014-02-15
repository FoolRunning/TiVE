using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace ProdigalSoftware.TiVE.Renderer.OpenGL
{
    internal sealed class OpenGLRendererBackend : IRendererBackend
    {
        #region IRendererBackend implementation
        public IDisplay CreateDisplay()
        {
            return new OpenGLDisplay();
        }

        public void Draw(PrimitiveType primitiveType, IVertexDataCollection vertexes)
        {
            OpenTK.Graphics.OpenGL4.PrimitiveType glPrimitiveType;
            switch (primitiveType)
            {
                case PrimitiveType.Lines: glPrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType.Lines; break;
                case PrimitiveType.Triangles: glPrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles; break;
                case PrimitiveType.Quads: glPrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType.Quads; break;
                default: glPrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType.Points; break;
            }

            vertexes.Bind();
            GL.DrawArrays(glPrimitiveType, 0, vertexes.VertexCount);
        }

        public IVertexDataCollection CreateVertexDataCollection()
        {
            return new MeshVertexDataCollection();
        }

        public IVertexData CreateVertexData<T>(T[] data, int dataPerVertex, BufferType bufferType, bool dynamic) where T : struct
        {
            return new VertexData<T>(data, dataPerVertex, bufferType, dynamic);
        }

        public IShaderProgram CreateShaderProgram()
        {
            return new ShaderProgram();
        }
        #endregion

        #region OpenGLDisplay class
        private sealed class OpenGLDisplay : GameWindow, IDisplay
        {
            private GameLogic game;

            /// <summary>
            /// Creates a 1600x1200 window with the specified title.
            /// </summary>
            public OpenGLDisplay()
                : base(1600, 1200, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 16, 0, 4), "TiVE",
                    GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
            {
                VSync = VSyncMode.Off;
            }

            public void RunMainLoop(GameLogic newGame)
            {
                game = newGame;
                Run(120, 60);
            }

            /// <summary>
            /// 
            /// </summary>
            protected override void OnLoad(EventArgs eArgs)
            {
                base.OnLoad(eArgs);

                if (!game.Initialize())
                    Exit();

                GL.ClearColor(0.1f, 0.1f, 0.1f, 0.0f);

                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(DepthFunction.Less);

                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Back);
                GL.FrontFace(FrontFaceDirection.Cw);

                GL.Enable(EnableCap.Blend);
                //GL.Enable(EnableCap.AlphaTest);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

                GlUtils.CheckGLErrors();
            }

            /// <summary>
            /// Called when your window is resized. Set your viewport here. It is also
            /// a good place to set up your projection matrix (which probably changes
            /// along when the aspect ratio of your window).
            /// </summary>
            /// <param name="e">Not used.</param>
            protected override void OnResize(EventArgs e)
            {
                GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
                game.Resize(Width, Height);
            }

            /// <summary>
            /// Called when it is time to setup the next frame. Add you game logic here.
            /// </summary>
            /// <param name="e">Contains timing information for framerate independent logic.</param>
            protected override void OnUpdateFrame(FrameEventArgs e)
            {
                if (!game.UpdateFrame(e.Time, Keyboard))
                    Exit();
            }

            protected override void OnUnload(EventArgs e)
            {
                game.Cleanup();
                base.OnUnload(e);
            }

            /// <summary>
            /// Called when it is time to render the next frame. Add your rendering code here.
            /// </summary>
            /// <param name="e">Contains timing information.</param>
            protected override void OnRenderFrame(FrameEventArgs e)
            {
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                RenderStatistics stats = game.Render(e.Time);

                SwapBuffers();
                //GlUtils.CheckGLErrors();
                Title = string.Format("TiVE         Draw count = {0}     Polygon count = {1}", stats.DrawCount, stats.PolygonCount);
            }
        }
        #endregion

        #region VertexDataCollection class
        private sealed class MeshVertexDataCollection : IVertexDataCollection
        {
            private readonly List<IVertexData> buffers = new List<IVertexData>();
            private int vertexArrayId;

            ~MeshVertexDataCollection()
            {
                Debug.Assert(vertexArrayId == 0);
            }

            /// <summary>
            /// The number of vertexes in each buffer
            /// </summary>
            public int VertexCount
            {
                get { return (buffers.Count > 0) ? buffers[0].VertexCount : 0; }
            }

            public void AddBuffer(IVertexData buffer)
            {
                if (buffers.Contains(buffer))
                    throw new ArgumentException("Can not add the same buffer more then once");

                if (buffers.Count > 0 && !buffers.TrueForAll(b => b.VertexCount == buffer.VertexCount))
                    throw new ArgumentException("New buffers must contain the same number of vertexes as other buffers", "buffer");

                buffers.Add(buffer);
            }

            public void Delete()
            {
                buffers.ForEach(b => b.Delete());

                GL.DeleteVertexArrays(1, ref vertexArrayId);
                GlUtils.CheckGLErrors();

                vertexArrayId = 0;
            }

            public bool Initialize()
            {
                GL.GenVertexArrays(1, out vertexArrayId);
                GL.BindVertexArray(vertexArrayId);

                bool success = true;
                for (int i = 0; i < buffers.Count; i++)
                    success &= buffers[i].Initialize(i);
                return success;
            }

            public void Bind()
            {
                // ENHANCE: Create state management and only change the array attributes if needed
                GL.BindVertexArray(vertexArrayId);

                // Enable/disable array attributes
                for (int i = 0; i < buffers.Count; i++)
                    GL.EnableVertexAttribArray(i);
            }
        }
        #endregion

        #region VertexData class
        private sealed class VertexData<T> : IVertexData where T : struct
        {
            private readonly int floatsPerVertex;
            private readonly int vertexCount;
            private readonly BufferType bufferType;
            private readonly bool dynamic;
            private int dataVboId;
            private T[] data;

            public VertexData(T[] data, int floatsPerVertex, BufferType bufferType, bool dynamic)
            {
                this.data = data;
                this.floatsPerVertex = floatsPerVertex;
                this.bufferType = bufferType;
                this.dynamic = dynamic;
                vertexCount = data.Length;
            }

            ~VertexData()
            {
                Debug.Assert(dataVboId == 0, "Data buffer was not properly deleted");
            }

            /// <summary>
            /// Gets the number of vertexes in this buffer
            /// </summary>
            public int VertexCount
            {
                get { return vertexCount; }
            }

            public void Delete()
            {
                GL.DeleteBuffers(1, ref dataVboId);
                dataVboId = 0;
            }

            public bool Initialize(int arrayAttrib)
            {
                if (data == null)
                {
                    Debug.Fail("Data buffers can not be reinitialized");
                    return false;
                }

                int sizeInBytes = data.Length * Marshal.SizeOf(typeof(T));
                BufferTarget target = Target;

                // Load data into OpenGL
                GL.GenBuffers(1, out dataVboId);
                GL.BindBuffer(target, dataVboId);
                GL.BufferData(target, new IntPtr(sizeInBytes), data, dynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(arrayAttrib);
                GL.VertexAttribPointer(arrayAttrib, floatsPerVertex, VertexAttribPointerType.Float, false, 0, 0);
                GlUtils.CheckGLErrors();

                data = null; // Allow garbage collection
                return true;
            }

            private BufferTarget Target
            {
                get 
                {
                    switch (bufferType)
                    {
                        case BufferType.Data: return BufferTarget.ArrayBuffer;
                        case BufferType.Indexes: return BufferTarget.ElementArrayBuffer;
                        default: throw new InvalidOperationException("Unknown buffer type: " + bufferType);
                    }
                }
            }
        }
        #endregion

        #region ShaderProgram class
        private sealed class ShaderProgram : IShaderProgram
        {
            private readonly List<Shader> shaders = new List<Shader>();
            private readonly Dictionary<string, int> uniformLocations = new Dictionary<string, int>();
            private readonly List<string> attributes = new List<string>();

            private int programId;

            ~ShaderProgram()
            {
                Debug.Assert(programId == 0, "Shader was not properly deleted");
            }

            public void AddShader(string shaderSource, ShaderType shaderType)
            {
                shaders.Add(new Shader(shaderSource, shaderType));
            }

            public void AddAttribute(string name)
            {
                attributes.Add(name);
            }

            public void AddKnownUniform(string name)
            {
                uniformLocations.Add(name, -1);
            }

            public void Delete()
            {
                // Shaders are deleted after attempting to compile

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

                bool success = true;
                shaders.ForEach(s => success &= s.Compile());

                if (success)
                {
                    shaders.ForEach(s => GL.AttachShader(programId, s.ShaderId));

                    for (int i = 0; i < attributes.Count; i++)
                        GL.BindAttribLocation(programId, i, attributes[i]);

                    GL.LinkProgram(programId);

                    string info;
                    GL.GetProgramInfoLog(programId, out info);
                    if (!string.IsNullOrEmpty(info))
                        Debug.WriteLine(info);

                    int linkResult;
                    GL.GetProgram(programId, GetProgramParameterName.LinkStatus, out linkResult);
                    success = (linkResult == 1);
                }

                if (success)
                {
                    foreach (string uniform in uniformLocations.Keys.ToList()) // Make copy of the keys so we can change the dictionary
                        uniformLocations[uniform] = GL.GetUniformLocation(programId, uniform);
                }

                shaders.ForEach(s => s.Delete());
                GlUtils.CheckGLErrors();

                return success;
            }

            public void SetUniform(string name, ref Matrix4 value)
            {
                GL.UniformMatrix4(uniformLocations[name], false, ref value);
                
            }

            public void SetUniform(string name, ref Vector3 value)
            {
                GL.Uniform3(uniformLocations[name], ref value);
            }

            public void SetUniform(string name, ref Vector4 value)
            {
                GL.Uniform4(uniformLocations[name], ref value);
            }
        }
        #endregion

        #region Shader class
        private sealed class Shader
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
                Debug.Assert(ShaderId == 0);
            }

            public int ShaderId { get; private set; }

            public void Delete()
            {
                if (ShaderId != 0)
                    GL.DeleteShader(ShaderId);
                ShaderId = 0;
            }

            public bool Compile()
            {
                ShaderId = GL.CreateShader(GLShaderType);

                GL.ShaderSource(ShaderId, shaderSource);
                GL.CompileShader(ShaderId);

                string info;
                GL.GetShaderInfoLog(ShaderId, out info);

                // ENHANCE: output to the message list instead of output
                if (!string.IsNullOrEmpty(info))
                    Debug.WriteLine(info);

                int compileResult;
                GL.GetShader(ShaderId, ShaderParameter.CompileStatus, out compileResult);
                if (compileResult != 1)
                {
                    Debug.WriteLine("Compile Error!");
                    Debug.WriteLine(shaderSource);
                    Delete();
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
        #endregion
    }
}
