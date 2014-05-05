using System;

namespace ProdigalSoftware.Utils
{
    /// <summary>
    /// Various helper methods for dealing with arrays
    /// </summary>
    public static class ArrayUtils
    {
        /// <summary>
        /// Increases the size of the specified array by two thirds of it's current size. This is guaranteed to increase the size of the
        /// specified array by at least one item.
        /// </summary>
        public static void ResizeArray<T>(ref T[] array)
        {
            int addedCount = array.Length > 1 ? (array.Length * 2 / 3) : 1;
            Array.Resize(ref array, array.Length + addedCount);
        }
    }
}
