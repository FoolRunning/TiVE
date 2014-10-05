namespace ProdigalSoftware.Utils
{
    public struct Vector3b
    {
        public readonly byte X;
        public readonly byte Y;
        public readonly byte Z;

        public Vector3b(byte x, byte y, byte z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return string.Format("Vector3b({0},{1},{2})", X, Y, Z);
        }
    }
}
