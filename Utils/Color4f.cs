namespace ProdigalSoftware.Utils
{
    public struct Color4f
    {
        public readonly float R;
        public readonly float G;
        public readonly float B;
        public readonly float A;

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
    }
}
