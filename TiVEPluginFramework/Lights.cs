using System;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public interface ILight
    {
        Vector3b Location { get; }

        float MaxDist { get; }

        Color4b GetColorAtDist(float dist);
    }

    public sealed class PointLight : ILight
    {
        private readonly Color4b brightest;
        private readonly float attenuationLinear;
        private readonly float attenuationExp;

        public PointLight(Vector3b location, Color4b brightest, float attenuationLinear, float attenuationExp)
        {
            Location = location;
            this.brightest = brightest;
            this.attenuationLinear = attenuationLinear;
            this.attenuationExp = attenuationExp;
            MaxDist = LightUtils.GetMaxDistanceFromLight(brightest, attenuationLinear, attenuationExp);
        }

        public Vector3b Location { get; private set; }

        public float MaxDist { get; private set; }

        public Color4b GetColorAtDist(float dist)
        {
            return brightest;
        }
    }

    public static class LightUtils
    {
        /// <summary>
        /// Light distance calculation taken from: http://ogldev.atspace.co.uk/www/tutorial36/tutorial36.html
        /// </summary>
        /// <param name="color"></param>
        /// <param name="attenuationLinear"></param>
        /// <param name="attenuationExp"></param>
        /// <returns></returns>
        public static float GetMaxDistanceFromLight(Color4b color, float attenuationLinear, float attenuationExp)
        {
            float maxChannel = Math.Max(Math.Max(color.R, color.G), color.B);

            return (-attenuationLinear + (float)Math.Sqrt(attenuationLinear * attenuationLinear -
                4.0f * attenuationExp * (attenuationExp - 256.0f * maxChannel))) / (2.0f * attenuationExp);
        }
    }
}
