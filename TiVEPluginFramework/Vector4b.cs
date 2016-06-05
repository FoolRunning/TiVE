using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ProdigalSoftware.TiVEPluginFramework
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector4b : ITiVESerializable
    {
        public static readonly Guid ID = new Guid("F1689872-8334-4553-999E-F161F5AD6ACE");

        public byte X;
        public byte Y;
        public byte Z;
        public byte W;

        public Vector4b(BinaryReader reader)
        {
            X = reader.ReadByte();
            Y = reader.ReadByte();
            Z = reader.ReadByte();
            W = reader.ReadByte();
        }

        public Vector4b(byte x, byte y, byte z, byte w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public override string ToString()
        {
            return string.Format("Vector4b({0},{1},{2},{3})", X, Y, Z, W);
        }

        public void SaveTo(BinaryWriter writer)
        {
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
            writer.Write(W);
        }
    }
}
