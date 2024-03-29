﻿using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ProdigalSoftware.TiVEPluginFramework
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Color4f : ITiVESerializable
    {
        public static readonly Guid ID = new Guid("C0C826F8-B35B-4684-B3E4-ADCB9D9760EE");

        public float R;
        public float G;
        public float B;
        public float A;

        public Color4f(BinaryReader reader)
        {
            R = reader.ReadSingle();
            G = reader.ReadSingle();
            B = reader.ReadSingle();
            A = reader.ReadSingle();
        }
        
        public Color4f(byte r, byte g, byte b, byte a)
        {
            R = r / (float)byte.MaxValue;
            G = g / (float)byte.MaxValue;
            B = b / (float)byte.MaxValue;
            A = a / (float)byte.MaxValue;
        }

        public Color4f(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static explicit operator Color4f(Color4b color)
        {
            return new Color4f(color.R, color.G, color.B, color.A);
        }
        
        public static Color4f operator +(Color4f c1, Color4f c2)
        {
            return new Color4f(c1.R + c2.R, c1.G + c2.G, c1.B + c2.B, c1.A);
        }

        public static Color4f operator *(Color4f c, float value)
        {
            return new Color4f(c.R * value, c.G * value, c.B * value, c.A);
        }

        /// <summary>
        /// Returns a pointer to the first element of the specified instance.
        /// </summary>
        /// <param name="c">The instance.</param>
        /// <returns>A pointer to the first element of v.</returns>
        public static unsafe explicit operator float*(Color4f c)
        {
            return &c.R;
        }

        #region Implementation of ITiVESerializable
        public void SaveTo(BinaryWriter writer)
        {
            writer.Write(R);
            writer.Write(G);
            writer.Write(B);
            writer.Write(A);
        }
        #endregion
    }
}
