using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ProdigalSoftware.TiVEPluginFramework
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2i : ITiVESerializable
    {
        public static readonly Guid ID = new Guid("ED6BDBDA-F1F6-4347-893D-F3BBE7583DB6");

        public int X;
        public int Y;

        public Vector2i(BinaryReader reader)
        {
            X = reader.ReadInt32();
            Y = reader.ReadInt32();
        }

        public Vector2i(int x, int y)
        {
            X = x;
            Y = y;
        }
        
        public override string ToString()
        {
            return string.Format("Vector2i({0},{1})", X, Y);
        }

        public void SaveTo(BinaryWriter writer)
        {
            writer.Write(X);
            writer.Write(Y);
        }
    }

}
