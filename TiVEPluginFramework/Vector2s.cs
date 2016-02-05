using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ProdigalSoftware.TiVEPluginFramework
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2s : ITiVESerializable
    {
        public static readonly Guid ID = new Guid("BA85727A-6579-4F60-B11B-38B93723A3A4");

        public short X;
        public short Y;

        public Vector2s(BinaryReader reader)
        {
            X = reader.ReadInt16();
            Y = reader.ReadInt16();
        }

        public Vector2s(short x, short y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return string.Format("Vector2s({0},{1})", X, Y);
        }

        public void SaveTo(BinaryWriter writer)
        {
            writer.Write(X);
            writer.Write(Y);
        }
    }
}
