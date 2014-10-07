using System;

namespace ProdigalSoftware.TiVEPluginFramework.Lighting
{
    public static class LightUtils
    {
        private const float MinLightValue = 0.01f; // 0.003f (0.3%) produces the best result as that is less then a single light value's worth

        /// <summary>
        /// 
        /// </summary>
        public static float GetMaxDistanceFromLight(float attenuation)
        {
            return (float)Math.Sqrt(1.0 / (attenuation * MinLightValue));
        }
    }
}
