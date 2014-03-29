namespace ProdigalSoftware.Utils
{
    public static class MathUtils
    {
        /// <summary>
        /// Returns the power-of-two that is greater than or equal to the specified value
        /// </summary>
        public static int ClosestPow2(this int value)
        {
            int pow = 1;
            while (pow < value)
                pow = pow << 1;
            return pow;
        }
    }
}
