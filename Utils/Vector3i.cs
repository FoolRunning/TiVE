namespace ProdigalSoftware.Utils
{
    public struct Vector3i
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public Vector3i(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return string.Format("Vector3i({0},{1},{2})", X, Y, Z);
        }
    }
}
