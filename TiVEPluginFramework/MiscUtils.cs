using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace ProdigalSoftware.TiVEPluginFramework
{
    [PublicAPI]
    public static class MiscUtils
    {
        private const int MaxPowerOfTwo = (1 << 30);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FastAbs(int value)
        {
            return value >= 0 ? value : -value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FastSign(int value)
        {
            return value == 0 ? 0 : (value < 0 ? -1 : 1);
        }
    }
}
