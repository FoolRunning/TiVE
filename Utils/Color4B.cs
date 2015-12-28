using System;
using System.Runtime.InteropServices;

namespace ProdigalSoftware.Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Color4b
    {
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;
        public readonly byte A;

        public Color4b(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public Color4b(float r, float g, float b, float a)
        {
            R = (byte)Math.Max(0, Math.Min(255, (int)(r * 255)));
            G = (byte)Math.Max(0, Math.Min(255, (int)(g * 255)));
            B = (byte)Math.Max(0, Math.Min(255, (int)(b * 255)));
            A = (byte)Math.Max(0, Math.Min(255, (int)(a * 255)));
        }

        public uint ToArgb()
        {
            return (uint)(A << 24 | R << 16 | G << 8 | B << 0);
        }

        public static explicit operator Color4b(Color4f color)
        {
            return new Color4b(color.R, color.G, color.B, color.A);
        }

        public static explicit operator Color4b(Color3f color)
        {
            return new Color4b(color.R, color.G, color.B, 1.0f);
        }

        public static Color4b operator +(Color4b c, Color4b c2)
        {
            return new Color4b((byte)Math.Min(255, c.R + c2.R), (byte)Math.Min(255, c.G + c2.G), 
                (byte)Math.Min(255, c.B + c2.B), (byte)Math.Min(255, c.A + c2.A));
        }

        public static Color4b operator +(Color4b c, int value)
        {
            return new Color4b((byte)Math.Min(255, c.R + value), (byte)Math.Min(255, c.G + value),
                (byte)Math.Min(255, c.B + value), c.A);
        }

        public static Color4b operator -(Color4b c, int value)
        {
            return new Color4b((byte)Math.Max(0, c.R - value), (byte)Math.Max(0, c.G - value),
                (byte)Math.Max(0, c.B - value), c.A);
        }

        public static Color4b operator *(Color4b c, float value)
        {
            return new Color4b((byte)Math.Min(255, (int)(c.R * value)), (byte)Math.Min(255, (int)(c.G * value)), 
                (byte)Math.Min(255, (int)(c.B * value)), c.A);
        }

        public static Color4b operator *(Color4b c1, Color4b c2)
        {
            return new Color4b((byte)(c1.R * c2.R / 255), (byte)(c1.G * c2.G / 255), 
                (byte)(c1.B * c2.B / 255), (byte)(c1.A * c2.A / 255));
        }

        public override string ToString()
        {
            return string.Format("Color4b({0},{1},{2},{3})", R, G, B, A);
        }
    }
}
