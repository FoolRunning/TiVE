using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using ProdigalSoftware.TiVE.Renderer;

namespace ProdigalSoftware.TiVE.Core.Backend.OpenTKImpl
{
    internal sealed class OpenTKBackend : IControllerBackend
    {
        #region IControllerBackend implementation
        public IKeyboard Keyboard
        {
            get { return new KeyboardImpl(); }
        }

        public IMouse Mouse
        {
            get { return null; }
        }

        public IEnumerable<DisplaySetting> AvailableDisplaySettings
        {
            get 
            {
                return DisplayDevice.Default.AvailableResolutions
                    .Where(r => (r.BitsPerPixel == 24 || r.BitsPerPixel == 32) && r.RefreshRate >= 60)
                    .Select(r => new DisplaySetting(r.Width, r.Height, (int)r.RefreshRate));
            }
        }

        public INativeDisplay CreateNatveDisplay(DisplaySetting displaySetting, FullScreenMode fullscreenMode, int antiAliasAmount, bool vsync)
        {
            OpenTKDisplay nativeDisplay = new OpenTKDisplay(displaySetting, fullscreenMode, vsync, antiAliasAmount);
            nativeDisplay.Visible = true;
            return nativeDisplay;
        }
        
        public void Initialize()
        {
            GL.ClearColor(1.0f, 0.5f, 0.0f, 1.0f);
            //GL.ClearColor(0, 0, 0, 1);

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
            DataValueType dataValueType, bool normalize, bool dynamic) where T : struct
        {
            return new RendererData<T>(data, elementCount, elementsPerVertex, dataType, dataValueType, normalize, dynamic);
        }

        public IShaderProgram CreateShaderProgram()
        {
            return new ShaderProgram();
        }

        public string GetShaderDefinitionFileResourcePath()
        {
            return "ProdigalSoftware.TiVE.Core.Backend.OpenTKImpl.Shaders.Shaders.shad";
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
        private static DrawElementsType GetDrawType(DataValueType dataValueType)
        {
            switch (dataValueType)
            {
                case DataValueType.Byte: return DrawElementsType.UnsignedByte;
                case DataValueType.UShort: return DrawElementsType.UnsignedShort;
                case DataValueType.UInt: return DrawElementsType.UnsignedInt;
                default: throw new InvalidOperationException("Invalid draw type: " + dataValueType);
            }
        }
        #endregion
    }
}
