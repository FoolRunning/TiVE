﻿using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework
{
    [Flags]
    [PublicAPI]
    public enum VoxelSettings : byte
    {
        None = 0x00,
        AllowLightPassthrough = 0x01,
        IgnoreLighting = 0x02,
        SkipVoxelNormalCalc = 0x04
    }

    /// <summary>
    /// Represents a single voxel in TiVE.
    /// </summary>
    [PublicAPI]
    [StructLayout(LayoutKind.Sequential)]
    public struct Voxel
    {
        #region Constants/Member variables
        private const byte SettingsBitMask = 0x0F;

        /// <summary>Represents a voxel location that is not filled in.</summary>
        public static readonly Voxel Empty = new Voxel(0x00, 0x00, 0x00, 0x00, VoxelSettings.AllowLightPassthrough | VoxelSettings.IgnoreLighting);
        /// <summary>Represents a voxel location that is filled in with a solid black color.</summary>
        public static readonly Voxel Black = new Voxel(0x000000F0);
        /// <summary>Represents a voxel location that is filled in with a solid white color.</summary>
        public static readonly Voxel White = new Voxel(0xFFFFFFF0);

        private readonly uint value;
        #endregion

        #region Constructors
        public Voxel(float r, float g, float b, float a = 1.0f, VoxelSettings settings = VoxelSettings.None) : 
            this((byte)Math.Max(0, Math.Min(255, (int)(r * 255))),
            (byte)Math.Max(0, Math.Min(255, (int)(g * 255))),
            (byte)Math.Max(0, Math.Min(255, (int)(b * 255))),
            (byte)Math.Max(0, Math.Min(255, (int)(a * 255))), settings)
        {
        }

        public Voxel(byte r, byte g, byte b, byte a = byte.MaxValue, VoxelSettings settings = VoxelSettings.None)
        {
            value = (uint)(r << 24 | g << 16 | b << 8 | (a & 0xF0) | (byte)settings);
        }

        private Voxel(uint rgbaValue)
        {
            // only 4 bits of percision are available for the alpha so the bottom 4 bits are always assumed to be set on the assumption that
            // an alpha of zero would be considered worthless as the voxel should, then, not be set at all. 
            // So the lowest alpha value that a voxel can have is 15.
            value = rgbaValue & 0xFFFFFFF0;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the red component of the voxel color
        /// </summary>
        public byte R
        {
            get { return (byte)(value >> 24 & 0xFF); }
        }

        /// <summary>
        /// Gets the green component of the voxel color
        /// </summary>
        public byte G
        {
            get { return (byte)(value >> 16 & 0xFF); }
        }

        /// <summary>
        /// Gets the blue component of the voxel color
        /// </summary>
        public byte B
        {
            get { return (byte)(value >> 8 & 0xFF); }
        }

        /// <summary>
        /// Gets the alpha component of the voxel color
        /// </summary>
        public byte A
        {
            get { return (byte)((value & 0xF0) | 0x0F); }
        }

        public VoxelSettings Settings
        {
            get { return (VoxelSettings)(value & SettingsBitMask); }
        }

        public bool AllowLightPassthrough
        {
            get { return (value & (byte)VoxelSettings.AllowLightPassthrough) != 0; }
        }

        public bool IgnoreLighting
        {
            get { return (value & (byte)VoxelSettings.IgnoreLighting) != 0; }
        }

        public bool SkipVoxelNormalCalc
        {
            get { return (value & (byte)VoxelSettings.SkipVoxelNormalCalc) != 0; }
        }
        #endregion

        #region Operator overloads
        public static bool operator ==(Voxel v, uint rgbaValue)
        {
            return (v.value & 0xFFFFFFF0) == (rgbaValue & 0xFFFFFFF0);
        }

        public static bool operator ==(Voxel v1, Voxel v2)
        {
            return v1.value == v2.value;
        }

        public static bool operator !=(Voxel v, uint rgbaValue)
        {
            return (v.value & 0xFFFFFFF0) != (rgbaValue & 0xFFFFFFF0);
        }

        public static bool operator !=(Voxel v1, Voxel v2)
        {
            return v1.value != v2.value;
        }

        public static explicit operator uint(Voxel voxel)
        {
            return voxel.value;
        }

        public static explicit operator Voxel(uint rgbaValue)
        {
            return new Voxel(rgbaValue);
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
