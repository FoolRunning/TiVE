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

        public static bool AreEqual<T>(T[] a1, T[] a2)
        {
            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++)
            {
                if (!a1[i].Equals(a2[i]))
                    return false;
            }
            return true;
        }
    }
}
