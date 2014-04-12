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
                        GetDrawType(meshData.IndexType), IntPtr.Zero, meshData.InstanceCount);
                }
                else
                    GL.DrawArraysInstanced(GlPrimitiveType(primitiveType), 0, meshData.VertexCount, meshData.InstanceCount);
            }
            else if (meshData.ContainsIndexes)
                GL.DrawElements(GlPrimitiveType(primitiveType), meshData.IndexCount, GetDrawType(meshData.IndexType), IntPtr.Zero);
            else // Just straight vertex data
                GL.DrawArrays(GlPrimitiveType(primitiveType), 0, meshData.VertexCount);

            GlUtils.CheckGLErrors();
        }

        public IVertexDataCollection CreateVertexDataCollection()
        {
            return new MeshVertexDataCollection();
        }

        public IRendererData CreateData<T>(T[] data, int elementCount, int elementsPerVertex, DataType dataType, 
            ValueType valueType, bool normalize, bool dynamic) where T : struct
        {
            return new RendererData<T>(data, elementCount, elementsPerVertex, dataType, valueType, normalize, dynamic);
        }

        public IShaderProgram CreateShaderProgram()
        {
            return new ShaderProgram();
        }

        public void SetBlendMode(BlendMode mode)
        {
            switch (mode)
            {
                case BlendMode.None: 
                    GL.Disable(EnableCap.Blend);
                    break;
                case BlendMode.Realistic:
                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                    break;
                case BlendMode.Additive:
                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
                    break;
                default:
                    throw new ArgumentException("Unknown blend mode: " + mode, "mode");
            }
        }

        public void DisableDepthWriting()
        {
            GL.Disable(EnableCap.DepthTest);
        }

        public void EnableDepthWriting()
        {
            GL.Enable(EnableCap.DepthTest);
        }
        #endregion

        #region Private helper methods
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

        private DrawElementsType GetDrawType(ValueType valueType)
        {
            switch (valueType)
            {
                case ValueType.Byte: return DrawElementsType.UnsignedByte;
                case ValueType.UShort: return DrawElementsType.UnsignedShort;
                case ValueType.UInt: return DrawElementsType.UnsignedInt;
                default: throw new InvalidOperationException("Invalid draw type: " + valueType);
            }
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
                : base(1600, 1200, new OpenTK.Graphics.GraphicsMode(new OpenTK.Graphics.ColorFormat(8, 8, 8, 8), 16, 0, 4), "TiVE",
                    GameWindowFlags.Default, DisplayDevice.Default, 3, 5, OpenTK.Graphics.GraphicsContextFlags.ForwardCompatible)
            {
                VSync = VSyncMode.On;
            }

            public void RunMainLoop(GameLogic newGame)
            {
                game = newGame;
                Run(60, 60);
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

                GL.Disable(EnableCap.Blend);

                GlUtils.CheckGLErrors();
            }

            /// <summary>
            /// Called when your window is resized. Set your viewport here. It is also
            /// a good place to set up your projection matrix (which probably changes
            /// along when the aspect ratio of your window).
            /// </summary>
            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
                game.Resize(Width, Height);
            }

            private readonly Stopwatch sw = new Stopwatch();
            private double lastPrintTime;

            private StatHelper renderTime = new StatHelper();
            private StatHelper updateTime = new StatHelper();
            
            /// <summary>
            /// Called when it is time to setup the next frame. Add you game logic here.
            /// </summary>
            /// <param name="e">Contains timing information for framerate independent logic.</param>
            protected override void OnUpdateFrame(FrameEventArgs e)
            {
                sw.Restart();
                base.OnUpdateFrame(e);

                if (!game.UpdateFrame((float)e.Time, Keyboard))
                    Exit();
                sw.Stop();

                updateTime.AddData(sw.ElapsedTicks * 1000f / Stopwatch.Frequency);

                lastPrintTime += e.Time;
                if (lastPrintTime > 0.25)
                {
                    lastPrintTime -= 0.25;
                    updateTime.UpdateDisplayedTime();
                    renderTime.UpdateDisplayedTime();
                }
            }

            protected override void OnUnload(EventArgs e)
            {
                game.Dispose();
                base.OnUnload(e);
            }

            /// <summary>
            /// Called when it is time to render the next frame. Add your rendering code here.
            /// </summary>
            /// <param name="e">Contains timing information.</param>
            protected override void OnRenderFrame(FrameEventArgs e)
            {
                base.OnRenderFrame(e);

                sw.Restart();
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                RenderStatistics stats = game.Render((float)e.Time);
                sw.Stop();

                renderTime.AddData(sw.ElapsedTicks * 1000f / Stopwatch.Frequency);

                SwapBuffers();
                //GlUtils.CheckGLErrors();
                Title = string.Format("TiVE         Update={5}   Render={4}   Voxels={0}  Rendered={1}  Polys={2}  Draws={3}",
                    stats.VoxelCount, stats.RenderedVoxelCount, stats.PolygonCount, stats.DrawCount, renderTime.DisplayedTime, updateTime.DisplayedTime);
            }
        }
        #endregion

        #region VertexDataCollection class
        private sealed class MeshVertexDataCollection : IVertexDataCollection
        {
            private readonly List<IRendererData> buffers = new List<IRendererData>();
            private bool deleted;
            private int vertexArrayId;
            private IRendererData indexBuffer;
            private IRendererData firstVertexBuffer;
            private IRendererData firstInstanceBuffer;

            ~MeshVertexDataCollection()
            {
                Messages.Assert(vertexArrayId == 0, "Data collection was not properly deleted");
            }

            public void Dispose()
            {
                lock (buffers)
                {
                    buffers.ForEach(b => b.Dispose());
                    if (indexBuffer != null)
                        indexBuffer.Dispose();

                    if (vertexArrayId != 0)
                    {
                        GL.DeleteVertexArray(vertexArrayId);
                        GlUtils.CheckGLErrors();
                    }

                    vertexArrayId = 0;
                    deleted = true;
                }

                GC.SuppressFinalize(this);
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

            public ValueType IndexType 
            {
                get { return indexBuffer != null ? indexBuffer.ValueType : ValueType.Float; }
            }

            public int InstanceCount
            {
                get { return firstInstanceBuffer != null ? firstInstanceBuffer.DataLength : 0; }
            }

            private bool initialized;

            public bool IsInitialized 
            {
                get 
                {
                    lock (buffers)
                        return initialized || deleted;
                }
            }
            //public bool IsInitialized
            //{
            //    get 
            //    {
            //        lock (buffers)
            //            return vertexArrayId != 0;
            //    }
            //}

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

            public bool Initialize()
            {
                if (vertexArrayId != 0)
                    return true; // Already initialized

                bool success = true;
                lock (buffers)
                {
                    vertexArrayId = GL.GenVertexArray();
                    GL.BindVertexArray(vertexArrayId);

                    for (int i = 0; i < buffers.Count; i++)
                        success &= buffers[i].Initialize(i);
                    if (indexBuffer != null)
                        success &= indexBuffer.Initialize(-1);
                    initialized = success;
                }
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
            private T[] data;
            private int elementCount;
            private readonly int elementsPerVertex;
            private readonly DataType dataType;
            private readonly ValueType valueType;
            private readonly bool normalize;
            private readonly bool dynamic;
            private int dataVboId;
            private bool locked;

            public RendererData(T[] data, int elementCount, int elementsPerVertex, DataType dataType, ValueType valueType, bool normalize, bool dynamic)
            {
                this.data = data;
                this.elementCount = elementCount;
                this.elementsPerVertex = elementsPerVertex;
                this.dataType = dataType;
                this.valueType = valueType;
                this.normalize = normalize;
                this.dynamic = dynamic;
            }

            ~RendererData()
            {
                Messages.Assert(dataVboId == 0, "Data buffer was not properly deleted");
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                if (!locked)
                {
                    if (dataVboId != 0)
                    {
                        GL.DeleteBuffer(dataVboId);
                        GlUtils.CheckGLErrors();
                    }
                    dataVboId = 0;
                    data = null;
                    GC.SuppressFinalize(this);
                }
            }

            public int DataLength
            {
                get { return elementCount; }
            }

            public int ElementsPerVertex
            {
                get { return elementsPerVertex; }
            }

            public DataType DataType 
            {
                get { return dataType; }
            }

            public ValueType ValueType
            {
                get { return valueType; }
            }

            public bool Initialize(int arrayAttrib)
            {
                BufferTarget target = Target;
                if (dataVboId != 0)
                {
                    Bind(target, arrayAttrib);
                    return dataVboId != 0;
                }

                if (data == null)
                    throw new InvalidOperationException("Buffers can not be re-initialized");

                int sizeInBytes = elementCount * Marshal.SizeOf(typeof(T));

                // Load data into OpenGL
                dataVboId = GL.GenBuffer();
                Bind(target, arrayAttrib);
                GL.BufferData(target, new IntPtr(sizeInBytes), data, dynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
                GlUtils.CheckGLErrors();
                data = null;
                return true;
            }

            /// <summary>
            /// Changes the data in this buffer to the specified new data. This is an error if the buffer is not dynamic.
            /// </summary>
            public void UpdateData<T2>(T2[] newData, int elementsToUpdate) where T2 : struct 
            {
                if (!dynamic)
                    throw new InvalidOperationException("Can not update the contents of a static buffer");

                BufferTarget target = Target;
                GL.BindBuffer(target, dataVboId);

                elementCount = elementsToUpdate;
                int sizeInBytes = elementsToUpdate * Marshal.SizeOf(typeof(T2));
                //GL.MapBufferRange(target, new IntPtr(0), new IntPtr(sizeInBytes), BufferAccessMask.MapWriteBit);
                GL.BufferSubData(target, new IntPtr(0), new IntPtr(sizeInBytes), newData);
                GlUtils.CheckGLErrors();
            }

            /// <summary>
            /// 
            /// </summary>
            public IntPtr MapData(int elementsToMap)
            {
                BufferTarget target = Target;
                GL.BindBuffer(target, dataVboId);

                elementCount = elementsToMap; 
                int sizeInBytes = elementsToMap * Marshal.SizeOf(typeof(T));
                IntPtr result = GL.MapBufferRange(Target, new IntPtr(0), new IntPtr(sizeInBytes), 
                    BufferAccessMask.MapWriteBit | BufferAccessMask.MapInvalidateBufferBit);
                GlUtils.CheckGLErrors();

                return result;
            }

            /// <summary>
            /// 
            /// </summary>
            public void UnmapData()
            {
                BufferTarget target = Target;
                GL.BindBuffer(target, dataVboId);
                GL.UnmapBuffer(target);
                GlUtils.CheckGLErrors();
            }

            public void Lock()
            {
                locked = true;
            }

            public void Unlock()
            {
                locked = false;
            }

            private void Bind(BufferTarget target, int arrayAttrib)
            {
                GL.BindBuffer(target, dataVboId);
                if (dataType != DataType.Index)
                {
                    GL.EnableVertexAttribArray(arrayAttrib);
                    GL.VertexAttribPointer(arrayAttrib, elementsPerVertex, PointerType, normalize, 0, 0);
                }

                if (dataType == DataType.Instance)
                    GL.VertexAttribDivisor(arrayAttrib, 1);

                GlUtils.CheckGLErrors();
            }

            private VertexAttribPointerType PointerType 
            {
                get 
                {
                    switch (valueType)
                    {
                        case ValueType.Byte: return VertexAttribPointerType.UnsignedByte;
                        case ValueType.Short: return VertexAttribPointerType.Short;
                        case ValueType.UShort: return VertexAttribPointerType.UnsignedShort;
                        case ValueType.Int: return VertexAttribPointerType.Int;
                        case ValueType.UInt: return VertexAttribPointerType.UnsignedInt;
                        case ValueType.Float: return VertexAttribPointerType.Float;
                        default: throw new InvalidOperationException("Unknown value type: " + valueType);
                    }
                }
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

            public void Dispose()
            {
                // Shaders are deleted after attempting to compile

                GL.DeleteProgram(programId);
                programId = 0;

                GC.SuppressFinalize(this);
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

                shaders.ForEach(s => s.Dispose());
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
        private sealed class Shader : IDisposable
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
        #endregion

        private sealed class StatHelper
        {
            private float totalTime;
            private int dataCount;

            public float DisplayedTime { get; private set; }

            public void AddData(float data)
            {
                totalTime += data;
                dataCount++;
            }

            public void UpdateDisplayedTime()
            {
                DisplayedTime = totalTime / Math.Max(dataCount, 1);
                totalTime = 0;
                dataCount = 0;
            }
        }
    }
}
