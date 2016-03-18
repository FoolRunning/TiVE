﻿using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ProdigalSoftware.TiVEPluginFramework
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3i : ITiVESerializable
    {
        public static readonly Guid ID = new Guid("901BDEE0-B2DD-4E46-9CF8-13E0F309B463");

        public int X;
        public int Y;
        public int Z;

        public Vector3i(BinaryReader reader)
        {
            X = reader.ReadInt32();
            Y = reader.ReadInt32();
            Z = reader.ReadInt32();
        }

        public Vector3i(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Gets the offset into an array based on this vector
        /// </summary>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetArrayOffset(int x, int y, int z)
        {
            MiscUtils.CheckConstraints(x, y, z, this);
            return (x * Z + z) * Y + y; // y-axis major for speed
        }


        public override string ToString()
        {
            return string.Format("Vector3i({0},{1},{2})", X, Y, Z);
        }

        public void SaveTo(BinaryWriter writer)
        {
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
        }
    }
}