using System;

namespace ProdigalSoftware.TiVEPluginFramework.Lighting
{
    public static class LightUtils
    {
        private const float MinLightValue = 0.05f; // 5% of the light

        /// <summary>
        /// 
        /// </summary>
        public static float GetMaxDistanceFromLight(float attenuation)
        {
            return (float)Math.Sqrt(1.0 / (attenuation * MinLightValue));
        }
    }
}
