using System;
using System.Diagnostics;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public static class MiscUtils
    {
        /// <summary>
        /// Throws an ArgumentOutOfRangeException if the specified location is outside the bounds of the specified size.
        /// This method is not compiled into release builds.
        /// </summary>
        [Conditional("DEBUG")]
        public static void CheckConstraints(int x, int y, int z, Vector3i size)
        {
            if (x < 0 || x >= size.X)
                throw new ArgumentOutOfRangeException("x");
            if (y < 0 || y >= size.Y)
                throw new ArgumentOutOfRangeException("y");
            if (z < 0 || z >= size.Z)
                throw new ArgumentOutOfRangeException("z");
        }
    }
}
