using System;
using System.Runtime.InteropServices;

namespace ProdigalSoftware.Utils
{
    /// <summary>
    /// Contains various helper methods for doing mathematical calculations
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// Returns the power-of-two that is greater than or equal to the specified value
        /// </summary>
        /// <exception cref="ArgumentException">If the value is greater then 2^30</exception>
        public static int ClosestPow2(int value)
        {
            if (value > 1 << 30)
                throw new ArgumentException("Can not get the nearest power-of-two for value larger than 2^30");

            int pow = 1;
            while (pow < value)
                pow = pow << 1;
            return pow;
        }
    
        public static int NumberOfSetBits(this int i)
        {
            // Code taken from: http://stackoverflow.com/a/109025/4953232
            i = i - ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }

        public static float FastSqrt(float val)
        {
            FloatIntUnion u;
            u.tmp = 0;
            float xhalf = 0.5f * val;
            u.f = val;
            u.tmp = 0x5f375a86 - (u.tmp >> 1);
            return u.f * (1.5f - xhalf * u.f * u.f) * val;
        }

        /// <summary>
        /// Returns an approximation of the inverse square root of left number.
        /// </summary>
        /// <returns>An approximation of the inverse square root of the specified number, with an upper error bound of 0.001</returns>
        /// <remarks>
        /// This is an improved implementation of the the method known as Carmack's inverse square root
        /// which is found in the Quake III source code. This implementation comes from
        /// http://www.codemaestro.com/reviews/review00000105.html. For the history of this method, see
        /// http://www.beyond3d.com/content/articles/8/
        /// </remarks>
        public static float InverseSqrtFast(float val)
        {
            FloatIntUnion u;
            u.tmp = 0;
            float xhalf = 0.5f * val;
            u.f = val;                              // Read bits as integer.
            u.tmp = 0x5f375a86 - (u.tmp >> 1);      // Make an initial guess for Newton-Raphson approximation
            val = u.f;                              // Convert bits back to float
            return val * (1.5f - xhalf * val * val); // Perform left single Newton-Raphson step.
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct FloatIntUnion
        {
            [FieldOffset(0)]
            public float f;

            [FieldOffset(0)]
            public int tmp;
        }
    }
}
