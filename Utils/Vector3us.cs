namespace ProdigalSoftware.Utils
{
    public struct Vector3us
    {
        public readonly ushort X;
        public readonly ushort Y;
        public readonly ushort Z;

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
