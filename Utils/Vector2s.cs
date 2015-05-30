using System.Runtime.InteropServices;

namespace ProdigalSoftware.Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2s
    {
        public readonly short X;
        public readonly short Y;

        public Vector2s(short x, short y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return string.Format("Vector2s({0},{1})", X, Y);
        }
    }
}
