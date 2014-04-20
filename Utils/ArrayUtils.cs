using System;

namespace ProdigalSoftware.Utils
{
    public static class ArrayUtils
    {
        public static void ResizeArray<T>(ref T[] array)
        {
            int newSize = array.Length + (array.Length * 2 / 3) + 1;
            Array.Resize(ref array, newSize);
        }
    }
}
