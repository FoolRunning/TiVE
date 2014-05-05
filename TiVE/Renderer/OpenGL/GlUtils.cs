using System;
using OpenTK.Graphics.OpenGL;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE.Renderer.OpenGL
{
    internal static class GlUtils
    {
        /// <summary>
        /// Checks for an OpenGL error and spits out the error and stack trace if there is an error.
        /// </summary>
        public static void CheckGLErrors()
        {
#if DEBUG
            ErrorCode error = GL.GetError();
            if (error != ErrorCode.NoError)
                DisplayError(error);
        }

        private static void DisplayError(ErrorCode code)
        {
            Messages.AddWarning("Found OpenGL error: " + code);
            Messages.AddWarning(Environment.StackTrace);
#endif
        }
    }
}
