using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework.Lighting
{
    public sealed class PointLight : ILight
    {
        private readonly Color4f brightest;
        private readonly float attenuation;

        public PointLight(Vector3b location, Color4f brightest, float attenuation)
        {
            Location = location;
            this.brightest = brightest;
            this.attenuation = attenuation;
            MaxVoxelDist = LightUtils.GetMaxDistanceFromLight(attenuation);
        }

        public Vector3b Location { get; private set; }

        public float MaxVoxelDist { get; private set; }

        public Color4f GetColorAtDistSquared(float distSquared)
        {
            float lightPercent = 1.0f / (1.0f + attenuation * distSquared);
            return new Color4f(brightest.R * lightPercent, brightest.G * lightPercent, brightest.B * lightPercent, 1.0f);
        }
    }
}
