using System;

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
    }
}
