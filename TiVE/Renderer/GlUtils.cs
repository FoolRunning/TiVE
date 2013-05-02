using System;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;

namespace ProdigalSoftware.TiVE.Renderer
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
