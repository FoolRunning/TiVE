using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.OpenGL
{
    internal sealed class OpenGLRendererBackend : IRendererBackend
    {
        #region IRendererBackend implementation
        public INativeWindow CreateNatveWindow(int width, int height, FullScreenMode fullscreenMode, int antiAliasAmount, bool vsync)
        {
            OpenGLDisplay nativeWindow = new OpenGLDisplay(width, height, fullscreenMode, vsync, antiAliasAmount);
            nativeWindow.Visible = true;
            return nativeWindow;
        }
        
        public void Initialize()
        {
            GL.ClearColor(0, 0, 0, 1);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Cw);

            SetBlendMode(BlendMode.Realistic);

            GlUtils.CheckGLErrors();
        }

        public void WindowResized(Rectangle newClientBounds)
        {
            GL.Viewport(newClientBounds.X, newClientBounds.Y, newClientBounds.Width, newClientBounds.Height);
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

        public void BeforeRenderFrame()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
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

        public string GetShaderDefinitionFileResourcePath()
        {
            return "ProdigalSoftware.TiVE.Renderer.OpenGL.Shaders.Shaders.shad";
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
                    throw new ArgumentException("Unknown blend mode: " + mode);
            }
        }

        public void DisableDepthWriting()
        {
            GL.DepthMask(false);
        }

        public void EnableDepthWriting()
        {
            GL.DepthMask(true);
        }
        #endregion

        #region Private helper methods
        /// <summary>
        /// Gets the OpenTK primitive type for the specified TiVE primitive type
        /// </summary>
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

        /// <summary>
        /// Gets a OpenTK draw type for the specified value type
        /// </summary>
        private static DrawElementsType GetDrawType(ValueType valueType)
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
        private sealed class OpenGLDisplay : GameWindow, INativeWindow
        {
            /// <summary>
            /// Creates a window
            /// </summary>
            public OpenGLDisplay(int width, int height, FullScreenMode fullScreenMode, bool vsync, int antiAliasAmount)
                : base(width, height, new GraphicsMode(32, 16, 0, antiAliasAmount), "TiVE", 
                    fullScreenMode == FullScreenMode.FullScreen ? GameWindowFlags.Fullscreen : GameWindowFlags.Default, 
                    DisplayDevice.Default, 3, 1, GraphicsContextFlags.ForwardCompatible)
            {
                if (fullScreenMode == FullScreenMode.WindowFullScreen)
                {
                    Width = DisplayDevice.Default.Width;
                    Height = DisplayDevice.Default.Height;
                    WindowBorder = WindowBorder.Hidden;
                    WindowState = WindowState.Fullscreen;
                }
                else if (fullScreenMode == FullScreenMode.FullScreen)
                {
                    // TODO: Abstract the resolution option and let the user choose the resolution
                    DisplayResolution resolution = DisplayDevice.Default.SelectResolution(width, height, 32, 0.0f);
                    DisplayDevice.Default.ChangeResolution(resolution);

                    // This seems to be needed when multiple monitors are present and the chosen resolution would push a 
                    // centered window onto the other monitor when the resolution is changed.
                    X = Y = 0;

                    // Not sure why, but some times when switching to fullscreen, the window will get the
                    // size of a secondary monitor. Reset the size just in case.
                    Width = resolution.Width;
                    Height = resolution.Height;
                }
                else // Windowed
                {
                    WindowBorder = WindowBorder.Resizable;
                    WindowState = WindowState.Normal;
                }

                VSync = vsync ? VSyncMode.On : VSyncMode.Off;

                Closing += OpenGLDisplay_Closing;
                Resize += OpenGLDisplay_Resize;
            }

            public event Action<Rectangle> WindowResized;
            public event EventHandler WindowClosing;

            public Rectangle ClientBounds
            {
                get { return ClientRectangle; }
            }

            public string WindowTitle
            {
                set { Title = value; }
            }

            public IKeyboard KeyboardImplementation
            {
                get { return new KeyboardImpl(Keyboard); }
            }

            public void CloseWindow()
            {
                Exit();

                DisplayDevice.Default.RestoreResolution();
            }

            public void ProcessNativeEvents()
            {
                ProcessEvents();
            }

            public void UpdateDisplayContents()
            {
                GlUtils.CheckGLErrors();
                SwapBuffers();
            }

            void OpenGLDisplay_Closing(object sender, System.ComponentModel.CancelEventArgs e)
            {
                // Although this looks weird, we need to cancel the disposing of the OpenGL context, but we will still exit after firing the event
                e.Cancel = true;

                if (WindowClosing != null)
                    WindowClosing(this, EventArgs.Empty);
            }

            void OpenGLDisplay_Resize(object sender, EventArgs e)
            {
                if (WindowResized != null)
                    WindowResized(ClientRectangle);
            }
        }
        #endregion

        #region KeyboardImpl class
        private class KeyboardImpl : IKeyboard
        {
            private readonly KeyboardDevice device;

            public KeyboardImpl(KeyboardDevice device)
            {
                this.device = device;
            }

            public bool IsKeyPressed(Keys key)
            {
                return device[(Key)(uint)key];
            }
        }
        #endregion

        #region VertexDataCollection class
        private sealed class MeshVertexDataCollection : IVertexDataCollection
        {
            private readonly List<IRendererData> buffers = new List<IRendererData>();
            private volatile bool deleted;
            private volatile bool initialized;
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
                using (new PerformanceLock(buffers))
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

            public bool IsInitialized 
            {
                get { return initialized || deleted; }
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

            public bool Initialize()
            {
                if (vertexArrayId != 0)
                    return true; // Already initialized

                bool success = true;
                using (new PerformanceLock(buffers))
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
            private int allocatedDataElementCount;
            private readonly int elementsPerVertex;
            private readonly DataType dataType;
            private readonly ValueType valueType;
            private readonly bool normalize;
            private readonly bool dynamic;
            private int dataVboId;

            public RendererData(T[] data, int elementCount, int elementsPerVertex, DataType dataType, ValueType valueType, bool normalize, bool dynamic)
            {
                this.data = data;
                allocatedDataElementCount = data.Length;
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
                if (dataVboId != 0)
                {
                    GL.DeleteBuffer(dataVboId);
                    GlUtils.CheckGLErrors();
                }
                dataVboId = 0;
                data = null;
                GC.SuppressFinalize(this);
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

                int sizeInBytes = (elementCount > 0 ? elementCount : allocatedDataElementCount) * Marshal.SizeOf(typeof(T));

                // Load data into OpenGL
                dataVboId = GL.GenBuffer();
                Bind(target, arrayAttrib);
                T[] dataVal = elementCount > 0 ? data : null;
                GL.BufferData(target, new IntPtr(sizeInBytes), dataVal, dynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
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
                if (elementCount > allocatedDataElementCount)
                    allocatedDataElementCount = elementCount;

                //int totalSizeInBytes = allocatedDataElementCount * Marshal.SizeOf(typeof(T2));
                GL.BufferData(target, new IntPtr(sizeInBytes), (T2[])null, BufferUsageHint.StreamDraw);
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

                if (programId != 0)
                    GL.DeleteProgram(programId);
                programId = 0;

                GC.SuppressFinalize(this);
            }

            public bool IsInitialized
            {
                get { return programId != 0; }
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

            public void Unbind()
            {
                GL.UseProgram(0);
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
    }
}
