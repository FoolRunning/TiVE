using System;
using System.Runtime.InteropServices;

namespace ProdigalSoftware.Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3us
    {
        public readonly ushort X;
        public readonly ushort Y;
        public readonly ushort Z;

        public Vector3us(int x, int y, int z)
        {
            if (x < 0 || x > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("x");
            if (y < 0 || y > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("y");
            if (z < 0 || z > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("z");

            X = (ushort)x;
            Y = (ushort)y;
            Z = (ushort)z;
        }

        public Vector3us(ushort x, ushort y, ushort z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return string.Format("Vector3us({0},{1},{2})", X, Y, Z);
        }
    }
}
