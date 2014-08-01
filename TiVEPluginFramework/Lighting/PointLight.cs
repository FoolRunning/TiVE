using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework.Lighting
{
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
}
