using System;
using OpenTK.Graphics.OpenGL;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE.Core.Backend.OpenTKImpl
{
    internal class OpenTKTexture : ITexture
    {
        private readonly int width;
        private readonly int height;

        private int textureID;

        public OpenTKTexture(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        ~OpenTKTexture()
        {
            Messages.Assert(textureID == 0, "Texture was not properly deleted");
        }

        public void Dispose()
        {
            if (textureID != 0)
            {
                GL.DeleteTexture(textureID);
                GlUtils.CheckGLErrors();
            }
        }

        public int Id
        {
            get { return textureID; }
        }

        public void Initialize()
        {
            if (textureID != 0)
                return; // Already initialized

            textureID = GL.GenTexture();
            
            Activate();

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, 
                PixelType.Byte, IntPtr.Zero);
            GlUtils.CheckGLErrors();
        }

        public void Activate()
        {
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GlUtils.CheckGLErrors();
        }

        public void UpdateTextureData(byte[] newData)
        {
            if (newData.Length != width * height * 4)
                throw new ArgumentException("Updated texture must contain the correct number of bytes", "newData");

            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, PixelFormat.Rgba,
                PixelType.Byte, newData);
            GlUtils.CheckGLErrors();
        }
    }
}
