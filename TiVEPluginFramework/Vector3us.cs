using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ProdigalSoftware.TiVEPluginFramework
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3us : ITiVESerializable
    {
        public static readonly Guid ID = new Guid("619F1ABA-43AC-4453-A1A3-FEB648F2D9E2");

        public ushort X;
        public ushort Y;
        public ushort Z;

        public Vector3us(BinaryReader reader)
        {
            X = reader.ReadUInt16();
            Y = reader.ReadUInt16();
            Z = reader.ReadUInt16();
        }

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

        #region Implementation of ITiVESerializable
        public void SaveTo(BinaryWriter writer)
        {
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
        }
        #endregion

        public override string ToString()
        {
            return string.Format("Vector3us({0},{1},{2})", X, Y, Z);
        }
    }
}
