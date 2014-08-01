using System;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework.Lighting
{
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
