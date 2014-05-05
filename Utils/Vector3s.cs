namespace ProdigalSoftware.Utils
{
    public struct Vector3s
    {
        public readonly short X;
        public readonly short Y;
        public readonly short Z;

        public Vector3s(short x, short y, short z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        #region Overrides of ValueType
        public override string ToString()
        {
            return string.Format("Vector3s({0},{1},{2})", X, Y, Z);
        }
        #endregion
    }
}
