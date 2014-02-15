using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;

namespace ProdigalSoftware.TiVE.Renderer.OpenGL
{
    internal static class GlUtils
    {
        public static void CheckGLErrors()
        {
            ErrorCode error = GL.GetError();
            if (error != ErrorCode.NoError)
                DisplayError(error);
        }

        private static void DisplayError(ErrorCode code)
        {
            Debug.WriteLine("Found OpenGL error: " + code);
            Debug.WriteLine(Environment.StackTrace);
        }
    }
}
