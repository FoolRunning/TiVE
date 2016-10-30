using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace ProdigalSoftware.TiVEPluginFramework
{
    [PublicAPI]
    public static class MiscUtils
    {
        private const int MaxPowerOfTwo = (1 << 30);

        /// <summary>
        /// Throws an ArgumentOutOfRangeException if the specified location is outside the bounds of the specified size.
        /// This method is not compiled into release builds.
        /// </summary>
        [Conditional("DEBUG")]
        public static void CheckConstraints(int x, int y, int z, Vector3i size)
        {
            if (x < 0 || x >= size.X)
                throw new ArgumentOutOfRangeException(nameof(x));
            if (y < 0 || y >= size.Y)
                throw new ArgumentOutOfRangeException(nameof(y));
            if (z < 0 || z >= size.Z)
                throw new ArgumentOutOfRangeException(nameof(z));
        }

        /// <summary>
        /// Throws an ArgumentOutOfRangeException if the specified location is outside the bounds of the specified size.
        /// This method is not compiled into release builds.
        /// </summary>
        [Conditional("DEBUG")]
        public static void CheckConstraints(int x, int y, int z, int size)
        {
            if (x < 0 || x >= size)
                throw new ArgumentOutOfRangeException(nameof(x));
            if (y < 0 || y >= size)
                throw new ArgumentOutOfRangeException(nameof(y));
            if (z < 0 || z >= size)
                throw new ArgumentOutOfRangeException(nameof(z));
        }

        public static int GetCountOfNonEmptyVoxels(Voxel[] voxels)
        {
            int count = 0;
            for (int i = 0; i < voxels.Length; i++) // For loop for speed
            {
                if (voxels[i] != Voxel.Empty)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Gets the power-of-two that is greater than or equal to the specified value
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="bitShifts">The number of bit shifts required to create the returned value</param>
        public static int GetNearestPowerOfTwo(int value, out int bitShifts)
        {
            if (value <= 0 || value > MaxPowerOfTwo)
                throw new ArgumentOutOfRangeException(nameof(value), "value must be greater than 0 and less than or equal to 2^30");

            int pow2Value = 1;
            bitShifts = 0;
            while (pow2Value < value)
            {
                pow2Value = pow2Value << 1;
                bitShifts++;
            }

            return pow2Value;
        }
    }
}
