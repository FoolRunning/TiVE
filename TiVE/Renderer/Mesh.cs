using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal sealed class Mesh
    {
        private readonly int sizeOfColor4InBytes = Marshal.SizeOf(new Color4());

        private const int VertexAttribArray = 0;
        private const int ColorAttribArray = 1;

        private readonly BeginMode primitiveType;
        private readonly Vector3[] vertexData;
        private readonly Color4[] colorData;
        private readonly uint[] indexData;
        private int vertexArrayId;
        private int vertexVboId;
        private int colorVboId;
        private int indexVboId;

        public Mesh(Vector3[] vertexData, Color4[] colorData, uint[] indexData, BeginMode primitiveType)
        {
            this.vertexData = vertexData;
            this.colorData = colorData;
            this.indexData = indexData;
            this.primitiveType = primitiveType;
        }

        ~Mesh()
        {
            Debug.Assert(vertexVboId == 0);
            Debug.Assert(colorVboId == 0);
            Debug.Assert(indexVboId == 0);
        }

        public void Delete()
        {
            GL.DeleteBuffers(1, ref vertexVboId);
            GL.DeleteBuffers(1, ref colorVboId);
            GL.DeleteBuffers(1, ref indexVboId);
            GL.DeleteVertexArrays(1, ref vertexArrayId);
            GlUtils.CheckGLErrors();

            vertexVboId = colorVboId = indexVboId = 0;
        }

        public bool Initialize()
        {
            GL.GenVertexArrays(1, out vertexArrayId);
            GL.BindVertexArray(vertexArrayId);

            // Load location data
            GL.GenBuffers(1, out vertexVboId);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexVboId);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(vertexData.Length * Vector3.SizeInBytes), vertexData, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(VertexAttribArray);
            GL.VertexAttribPointer(VertexAttribArray, 3, VertexAttribPointerType.Float, false, 0, 0);

            // Load color data
            GL.GenBuffers(1, out colorVboId);
            GL.BindBuffer(BufferTarget.ArrayBuffer, colorVboId);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(colorData.Length * sizeOfColor4InBytes), colorData, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(ColorAttribArray);
            GL.VertexAttribPointer(ColorAttribArray, 4, VertexAttribPointerType.Float, false, 0, 0);

            // Load index data
            GL.GenBuffers(1, out indexVboId);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexVboId);
            GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(indexData.Length * sizeof(uint)), indexData, BufferUsageHint.StaticDraw);

            GlUtils.CheckGLErrors();
            return true;
        }

        public void Draw()
        {
            if (indexData.Length == 0)
                return;

            // ENHANCE: Create state management and only change the array attributes if needed

            // Enable/disable array attributes
            GL.EnableVertexAttribArray(VertexAttribArray);
            GL.EnableVertexAttribArray(ColorAttribArray);

            GL.BindVertexArray(vertexArrayId);
            GL.DrawElements(primitiveType, indexData.Length, DrawElementsType.UnsignedInt, 0);
        }
    }
}
