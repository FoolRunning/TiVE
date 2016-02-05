using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ProdigalSoftware.TiVEPluginFramework
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3b : ITiVESerializable
    {
        public static readonly Guid ID = new Guid("94DFCFE8-B209-474D-9DF4-13EBB9EE3456");

        public byte X;
        public byte Y;
        public byte Z;

        public Vector3b(BinaryReader reader)
        {
            X = reader.ReadByte();
            Y = reader.ReadByte();
            Z = reader.ReadByte();
        }

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

        public void SaveTo(BinaryWriter writer)
        {
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
        }
    }
}
