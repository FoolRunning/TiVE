using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE.Renderer.OpenGL
{
    internal sealed class OpenGLRendererBackend : IRendererBackend
    {
        #region IRendererBackend implementation
        public IDisplay CreateDisplay()
        {
            return new OpenGLDisplay();
        }

        public void Draw(PrimitiveType primitiveType, IVertexDataCollection data)
        {
            data.Bind();

            MeshVertexDataCollection meshData = (MeshVertexDataCollection)data;
            if (meshData.ContainsInstances)
            {
                if (meshData.ContainsIndexes)
                {
                    GL.DrawElementsInstanced(GlPrimitiveType(primitiveType), meshData.IndexCount, 
                        DrawElementsType.UnsignedByte, IntPtr.Zero, meshData.InstanceCount);
                }
                else
                    GL.DrawArraysInstanced(GlPrimitiveType(primitiveType), 0, meshData.VertexCount, meshData.InstanceCount);
            }
            else if (meshData.ContainsIndexes)
                GL.DrawElements(GlPrimitiveType(primitiveType), meshData.IndexCount, DrawElementsType.UnsignedByte, IntPtr.Zero);
            else // Just straight vertex data
                GL.DrawArrays(GlPrimitiveType(primitiveType), 0, meshData.VertexCount);

            GlUtils.CheckGLErrors();
        }

        public IVertexDataCollection CreateVertexDataCollection()
        {
            return new MeshVertexDataCollection();
        }

        public IRendererData CreateData<T>(T[] data, int dataPerVertex, DataType dataType, bool dynamic) where T : struct
        {
            return new RendererData<T>(data, dataPerVertex, dataType, dynamic);
        }

        public IShaderProgram CreateShaderProgram()
        {
            return new ShaderProgram();
        }
        #endregion

        private static OpenTK.Graphics.OpenGL4.PrimitiveType GlPrimitiveType(PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.Lines: return OpenTK.Graphics.OpenGL4.PrimitiveType.Lines;
                case PrimitiveType.Triangles: return OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles;
                case PrimitiveType.Quads: return OpenTK.Graphics.OpenGL4.PrimitiveType.Quads;
                default: return OpenTK.Graphics.OpenGL4.PrimitiveType.Points;
            }
        }

        #region OpenGLDisplay class
        private sealed class OpenGLDisplay : GameWindow, IDisplay
        {
            private GameLogic game;

            /// <summary>
            /// Creates a 1600x1200 window with the specified title.
            /// </summary>
            public OpenGLDisplay()
                : base(1600, 1200, new OpenTK.Graphics.GraphicsMode(new OpenTK.Graphics.ColorFormat(8, 8, 8, 8), 16, 0, 4), "TiVE",
                    GameWindowFlags.Default, DisplayDevice.Default, 3, 5, OpenTK.Graphics.GraphicsContextFlags.ForwardCompatible)
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
                {
                    Exit();
                    return;
                }

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
                base.OnResize(e);
                GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
                game.Resize(Width, Height);
            }

            /// <summary>
            /// Called when it is time to setup the next frame. Add you game logic here.
            /// </summary>
            /// <param name="e">Contains timing information for framerate independent logic.</param>
            protected override void OnUpdateFrame(FrameEventArgs e)
            {
                base.OnUpdateFrame(e);

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
                base.OnRenderFrame(e);

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                RenderStatistics stats = game.Render(e.Time);

                SwapBuffers();
                //GlUtils.CheckGLErrors();
                Title = string.Format("TiVE         VC={0}  RVC={1}  PC = {2}  DC = {3}", 
                    stats.VoxelCount, stats.RenderedVoxelCount, stats.PolygonCount, stats.DrawCount);
            }
        }
        #endregion

        #region VertexDataCollection class
        private sealed class MeshVertexDataCollection : IVertexDataCollection
        {
            private readonly List<IRendererData> buffers = new List<IRendererData>();
            private int vertexArrayId;
            private IRendererData indexBuffer;
            private IRendererData firstVertexBuffer;
            private IRendererData firstInstanceBuffer;

            ~MeshVertexDataCollection()
            {
                Messages.Assert(vertexArrayId == 0, "Data collection was not properly deleted");
            }

            public bool ContainsIndexes
            {
                get { return indexBuffer != null; }
            }

            public bool ContainsInstances
            {
                get { return firstInstanceBuffer != null; }
            }

            public int VertexCount
            {
                get { return firstVertexBuffer != null ? firstVertexBuffer.DataLength : 0; }
            }

            public int IndexCount
            {
                get { return indexBuffer != null ? indexBuffer.DataLength : 0; }
            }

            public int InstanceCount
            {
                get { return firstInstanceBuffer != null ? firstInstanceBuffer.DataLength : 0; }
            }

            public void AddBuffer(IRendererData buffer)
            {
                if (vertexArrayId != 0)
                    throw new InvalidOperationException("Can not add a buffer after the collection has been initialized");

                if (buffers.Contains(buffer))
                    throw new InvalidOperationException("Can not add the same buffer more then once");

                IRendererData existingBuffer = buffers.Find(b => b.DataType == buffer.DataType);
                if (existingBuffer != null && existingBuffer.DataLength != buffer.DataLength)
                    throw new InvalidOperationException("New buffers must contain the same number of data elements as other buffers of the same type");

                if (buffer.DataType == DataType.Index)
                {
                    if (indexBuffer != null)
                        throw new InvalidOperationException("Buffer collection can only contain one index buffer");
                    indexBuffer = buffer;
                }
                else
                {
                    // Cache buffers for speed when getting counts, etc.
                    if (firstVertexBuffer == null && buffer.DataType == DataType.Vertex)
                        firstVertexBuffer = buffer;
                    else if (firstInstanceBuffer == null && buffer.DataType == DataType.Instance)
                        firstInstanceBuffer = buffer;

                    buffers.Add(buffer);
                }
            }

            public void Delete()
            {
                buffers.ForEach(b => b.Delete());
                if (indexBuffer != null)
                    indexBuffer.Delete();

                GL.DeleteVertexArray(vertexArrayId);
                GlUtils.CheckGLErrors();

                vertexArrayId = 0;
            }

            public bool Initialize()
            {
                if (vertexArrayId != 0)
                    return true; // Already initialized

                vertexArrayId = GL.GenVertexArray();
                GL.BindVertexArray(vertexArrayId);

                bool success = true;
                for (int i = 0; i < buffers.Count; i++)
                    success &= buffers[i].Initialize(i);
                if (indexBuffer != null)
                    success &= indexBuffer.Initialize(-1);
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

        #region RendererData class
        private class RendererData<T> : IRendererData where T : struct
        {
            private readonly int floatsPerVertex;
            private readonly DataType dataType;
            private readonly bool dynamic;
            private readonly T[] data;
            private int dataVboId;

            public RendererData(T[] data, int floatsPerVertex, DataType dataType, bool dynamic)
            {
                this.data = data;
                this.floatsPerVertex = floatsPerVertex;
                this.dataType = dataType;
                this.dynamic = dynamic;
            }

            ~RendererData()
            {
                Messages.Assert(dataVboId == 0, "Data buffer was not properly deleted");
            }

            public int DataLength
            {
                get { return data.Length; }
            }

            public DataType DataType 
            {
                get { return dataType; }
            }

            public void Delete()
            {
                GL.DeleteBuffer(dataVboId);
                dataVboId = 0;
            }

            public bool Initialize(int arrayAttrib)
            {
                BufferTarget target = Target;
                if (dataVboId != 0)
                {
                    GL.BindBuffer(target, dataVboId);
                    if (dataType != DataType.Index)
                    {
                        GL.EnableVertexAttribArray(arrayAttrib);
                        GL.VertexAttribPointer(arrayAttrib, floatsPerVertex, VertexAttribPointerType.Float, false, 0, 0);
                    }

                    if (dataType == DataType.Instance)
                        GL.VertexAttribDivisor(arrayAttrib, 1);

                    GlUtils.CheckGLErrors();
                    return dataVboId != 0;
                }

                int sizeInBytes = data.Length * Marshal.SizeOf(typeof(T));

                // Load data into OpenGL
                dataVboId = GL.GenBuffer();
                GL.BindBuffer(target, dataVboId);
                if (dataType != DataType.Index)
                {
                    GL.EnableVertexAttribArray(arrayAttrib);
                    GL.VertexAttribPointer(arrayAttrib, floatsPerVertex, VertexAttribPointerType.Float, false, 0, 0);
                }
                
                if (dataType == DataType.Instance)
                    GL.VertexAttribDivisor(arrayAttrib, 1);

                GL.BufferData(target, new IntPtr(sizeInBytes), data, dynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
                GlUtils.CheckGLErrors();
                return true;
            }

            private BufferTarget Target
            {
                get 
                {
                    switch (dataType)
                    {
                        case DataType.Index: return BufferTarget.ElementArrayBuffer;
                        case DataType.Vertex: 
                        case DataType.Instance:
                            return BufferTarget.ArrayBuffer;
                        default: throw new InvalidOperationException("Unknown buffer type: " + dataType);
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
                Messages.Assert(programId == 0, "Shader program was not properly deleted");
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
                Messages.Assert(ShaderId == 0, "Shader was not properly deleted");
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

                string info = GL.GetShaderInfoLog(ShaderId);

                if (!string.IsNullOrEmpty(info))
                    Messages.AddWarning(info);

                int compileResult;
                GL.GetShader(ShaderId, ShaderParameter.CompileStatus, out compileResult);
                if (compileResult != 1)
                {
                    Messages.AddWarning("Shader compile error!");
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
