using JetBrains.Annotations;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework
{
    /// <summary>
    /// Represents a single voxel in TiVE.
    /// </summary>
    public struct Voxel
    {
        /// <summary>Represents a voxel location that is not filled in.</summary>
        [PublicAPI] public static readonly Voxel Empty = new Voxel(0x00000000);
        /// <summary>Represents a voxel location that is filled in with a solid black color.</summary>
        [PublicAPI]
        public static readonly Voxel Black = new Voxel(0x000000FF);
        /// <summary>Represents a voxel location that is filled in with a solid white color.</summary>
        [PublicAPI]
        public static readonly Voxel White = new Voxel(0xFFFFFFFF);

        private readonly uint value;

        #region Constructors
        public Voxel(byte r, byte g, byte b, byte a = byte.MaxValue)
        {
            value = (uint)(r << 24 | g << 16 | b << 8 | a);
        }

        private Voxel(uint value)
        {
            this.value = value;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the red component of the voxel color
        /// </summary>
        [PublicAPI]
        public byte R
        {
            get { return (byte)(value >> 24 & 0xFF); }
        }

        /// <summary>
        /// Gets the green component of the voxel color
        /// </summary>
        [PublicAPI]
        public byte G
        {
            get { return (byte)(value >> 16 & 0xFF); }
        }

        /// <summary>
        /// Gets the blue component of the voxel color
        /// </summary>
        [PublicAPI]
        public byte B
        {
            get { return (byte)(value >> 8 & 0xFF); }
        }

        /// <summary>
        /// Gets the alpha component of the voxel color
        /// </summary>
        [PublicAPI]
        public byte A
        {
            get { return (byte)(value & 0xFF); }
        }
        #endregion

        #region Operator overloads
        public static bool operator ==(Voxel v, uint value)
        {
            return v.value == value;
        }

        public static bool operator ==(Voxel v1, Voxel v2)
        {
            return v1.value == v2.value;
        }
        
        public static bool operator !=(Voxel v, uint value)
        {
            return v.value != value;
        }

        public static bool operator !=(Voxel v1, Voxel v2)
        {
            return v1.value != v2.value;
        }

        public static explicit operator uint(Voxel voxel)
        {
            return voxel.value;
        }

        public static explicit operator Voxel(uint value)
        {
            return new Voxel(value);
        }

        public static explicit operator Color4b(Voxel voxel)
        {
            return new Color4b(voxel.R, voxel.G, voxel.B, voxel.A);
        }

        public static explicit operator Voxel(Color4b color)
        {
            return new Voxel(color.R, color.G, color.B, color.A);
        }
        #endregion

        #region Public methods
        [PublicAPI]
        public bool Equals(Voxel other)
        {
            return value == other.value;
        }
        #endregion

        #region Overrides of Object
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is Voxel && Equals((Voxel)obj);
        }

        public override int GetHashCode()
        {
            return (int)value;
        }

        public override string ToString()
        {
            return string.Format("Voxel({0},{1},{2},{3})", R, G, B, A);
        }
        #endregion
    }
}
