using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace ProdigalSoftware.TiVEPluginFramework
{
    [PublicAPI]
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

        /// <summary>
        /// Throws an ArgumentOutOfRangeException if the specified location is outside the bounds of the specified size.
        /// This method is not compiled into release builds.
        /// </summary>
        [Conditional("DEBUG")]
        public static void CheckConstraints(int x, int y, int z, int size)
        {
            if (x < 0 || x >= size)
                throw new ArgumentOutOfRangeException("x");
            if (y < 0 || y >= size)
                throw new ArgumentOutOfRangeException("y");
            if (z < 0 || z >= size)
                throw new ArgumentOutOfRangeException("z");
        }
    }
}
