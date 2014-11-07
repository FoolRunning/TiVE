﻿using System;

namespace ProdigalSoftware.Utils
{
    public struct Color3f
    {
        public readonly float R;
        public readonly float G;
        public readonly float B;

        public Color3f(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }

        public Color3f(byte r, byte g, byte b)
        {
            R = r / (float)byte.MaxValue;
            G = g / (float)byte.MaxValue;
            B = b / (float)byte.MaxValue;
        }

        public float MaxComponent
        {
            get { return Math.Max(R, Math.Max(G, B)); }
        }

        public override string ToString()
        {
            return string.Format("Color3f({0},{1},{2})", R, G, B);
        }

        public static Color3f operator +(Color3f c1, Color3f c2)
        {
            return new Color3f(c1.R + c2.R, c1.G + c2.G, c1.B + c2.B);
        }

        public static Color3f operator *(Color3f c, float value)
        {
            return new Color3f(c.R * value, c.G * value, c.B * value);
        }
    }
}
