using System;
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

        public static readonly Vector3i Zero = new Vector3i(0, 0, 0);

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

        #region Overrides of ValueType
        public override int GetHashCode()
        {
            throw new NotImplementedException("Vector3i can not be used as a key in a hashed set");
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector3i))
                return false;

            Vector3i other = (Vector3i)obj;
            return X == other.X && Y == other.Y && Z == other.Z;
        }
        #endregion

        public override string ToString()
        {
            return $"Vector3i({X},{Y},{Z})";
        }

        public void SaveTo(BinaryWriter writer)
        {
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
        }

        public static bool operator ==(Vector3i first, Vector3i second)
        {
            return first.X == second.X && first.Y == second.Y && first.Z == second.Z;
        }

        public static bool operator !=(Vector3i first, Vector3i second)
        {
            return first.X != second.X || first.Y != second.Y || first.Z != second.Z;
        }
    }
}
