using System;
using OpenTK.Graphics.OpenGL;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal sealed class Mesh
    {
        private const int VertexAttribArray = 0;
        private const int ColorAttribArray = 1;
        private const int TextureAttribArray = 2;
        private const int NormalAttribArray = 3;

        private readonly BeginMode primitiveType;
        private readonly float[] vertexData;
        private readonly byte[] colorData;
        private readonly float[] textureData;
        private readonly float[] normalData;
        private readonly uint[] indexData;
        private int vertexArrayId;
        private int vertexVboId;
        private int colorVboId;
        private int textureVboId;
        private int normalVboId;
        private int indexVboId;

        public Mesh(float[] vertexData, byte[] colorData, float[] textureData, float[] normalData, uint[] indexData, BeginMode primitiveType)
        {
            this.vertexData = vertexData;
            this.colorData = colorData;
            this.textureData = textureData;
            this.normalData = normalData;
            this.indexData = indexData;

            this.primitiveType = primitiveType;
        }

        public void Delete()
        {
            GL.DeleteBuffers(1, ref vertexVboId);
            if (colorVboId != 0)
                GL.DeleteBuffers(1, ref colorVboId);
            if (textureVboId != 0)
                GL.DeleteBuffers(1, ref textureVboId);
            if (normalVboId != 0)
                GL.DeleteBuffers(1, ref normalVboId);

            GL.DeleteBuffers(1, ref indexVboId);
            GL.DeleteVertexArrays(1, ref vertexArrayId);
            GlUtils.CheckGLErrors();
        }

        public bool Initialize()
        {
            GL.GenVertexArrays(1, out vertexArrayId);
            GL.BindVertexArray(vertexArrayId);

            GL.GenBuffers(1, out vertexVboId);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexVboId);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(vertexData.Length * sizeof(float)), vertexData, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(VertexAttribArray);
            GL.VertexAttribPointer(VertexAttribArray, 3, VertexAttribPointerType.Float, false, 0, 0);

            if (colorData != null && colorData.Length > 0)
            {
                GL.GenBuffers(1, out colorVboId);
                GL.BindBuffer(BufferTarget.ArrayBuffer, colorVboId);
                GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(colorData.Length), colorData, BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(ColorAttribArray);
                GL.VertexAttribPointer(ColorAttribArray, 4, VertexAttribPointerType.UnsignedByte, true, 0, 0);
            }
            
            if (textureData != null && textureData.Length > 0)
            {
                GL.GenBuffers(1, out textureVboId);
                GL.BindBuffer(BufferTarget.ArrayBuffer, textureVboId);
                GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(textureData.Length * sizeof(float)), textureData, BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(TextureAttribArray);
                GL.VertexAttribPointer(TextureAttribArray, 2, VertexAttribPointerType.Float, false, 0, 0);
            }
            
            if (normalData != null && normalData.Length > 0)
            {
                GL.GenBuffers(1, out normalVboId);
                GL.BindBuffer(BufferTarget.ArrayBuffer, normalVboId);
                GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(normalData.Length * sizeof(float)), normalData, BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(NormalAttribArray);
                GL.VertexAttribPointer(NormalAttribArray, 3, VertexAttribPointerType.Float, false, 0, 0);
            }

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
            
            if (colorData != null && colorData.Length > 0)
                GL.EnableVertexAttribArray(ColorAttribArray);
            else
                GL.DisableVertexAttribArray(ColorAttribArray);

            if (textureData != null && textureData.Length > 0)
                GL.EnableVertexAttribArray(TextureAttribArray);
            else
                GL.DisableVertexAttribArray(TextureAttribArray);
            if (normalData != null && normalData.Length > 0)
                GL.EnableVertexAttribArray(NormalAttribArray);
            else
                GL.DisableVertexAttribArray(NormalAttribArray);

            GL.BindVertexArray(vertexArrayId);
            GL.DrawElements(primitiveType, indexData.Length, DrawElementsType.UnsignedInt, 0);
        }
    }
}
