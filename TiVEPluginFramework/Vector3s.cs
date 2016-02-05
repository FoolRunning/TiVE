using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ProdigalSoftware.TiVEPluginFramework
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3s : ITiVESerializable
    {
        public static readonly Guid ID = new Guid("52E4033D-C0DE-4DD7-A010-D29AB296CA55");

        public short X;
        public short Y;
        public short Z;

        public Vector3s(BinaryReader reader)
        {
            X = reader.ReadInt16();
            Y = reader.ReadInt16();
            Z = reader.ReadInt16();
        }

        public Vector3s(short x, short y, short z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return string.Format("Vector3s({0},{1},{2})", X, Y, Z);
        }

        public void SaveTo(BinaryWriter writer)
        {
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
        }
    }
}
