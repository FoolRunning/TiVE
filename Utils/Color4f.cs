using System;

namespace ProdigalSoftware.Utils
{
    public struct Color4f
    {
        public readonly float R;
        public readonly float G;
        public readonly float B;
        public readonly float A;

        public Color4f(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public Color4f(byte r, byte g, byte b, byte a)
        {
            R = r / (float)byte.MaxValue;
            G = g / (float)byte.MaxValue;
            B = b / (float)byte.MaxValue;
            A = a / (float)byte.MaxValue;
        }

        public float MaxComponent
        {
            get { return Math.Max(R, Math.Max(G, B)); }
        }

        public override string ToString()
        {
            return string.Format("Color4f({0},{1},{2},{3})", R, G, B, A);
        }
    }
}
