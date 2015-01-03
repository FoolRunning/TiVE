using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework.Lighting
{
    public sealed class PointLight : ILight
    {
        public PointLight(Vector3b location, Color3f color, float lightBlockDist)
        {
            Location = location;
            Color = color;
            LightBlockDist = lightBlockDist;
        }

        public Vector3b Location { get; private set; }

        public float LightBlockDist { get; private set; }

        public Color3f Color { get; private set; }
    }
}
