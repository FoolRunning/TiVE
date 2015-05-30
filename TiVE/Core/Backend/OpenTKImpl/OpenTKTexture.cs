using System;
using OpenTK.Graphics.OpenGL;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE.Core.Backend.OpenTKImpl
{
    internal class OpenTKTexture : ITexture
    {
        private const int BytesPerPixel = 4;

        private byte[] data;
        private readonly int width;
        private readonly int height;

        public OpenTKTexture(int width, int height, byte[] data)
        {
            if (data != null && data.Length != width * height * BytesPerPixel)
                throw new ArgumentException("Texture data must contain the correct number of bytes", "data");

            this.width = width;
            this.height = height;
            this.data = data;
        }

        ~OpenTKTexture()
        {
            Messages.Assert(Id == 0, "Texture was not properly deleted");
        }

        public void Dispose()
        {
            if (Id != 0)
            {
                GL.DeleteTexture(Id);
                GlUtils.CheckGLErrors();
            }
            Id = 0;
        }

        public int Id { get; private set; }

        public void Initialize()
        {
            if (Id != 0)
                return; // Already initialized

            Id = GL.GenTexture();
            
            Activate();

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.Byte, data);
            GlUtils.CheckGLErrors();
            data = null;
        }

        public void Activate()
        {
            GL.BindTexture(TextureTarget.Texture2D, Id);
            GlUtils.CheckGLErrors();
        }

        public void UpdateTextureData(byte[] newData)
        {
            if (newData.Length != width * height * BytesPerPixel)
                throw new ArgumentException("Updated texture must contain the correct number of bytes", "newData");

            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, PixelFormat.Rgba,
                PixelType.Byte, newData);
            GlUtils.CheckGLErrors();
        }
    }
}
