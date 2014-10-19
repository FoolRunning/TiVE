using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework.Lighting
{
    public sealed class PointLight : ILight
    {
        public PointLight(Vector3b location, Color3f color, float attenuation)
        {
            Location = location;
            Color = color;
            Attenuation = attenuation;
        }

        public Vector3b Location { get; private set; }

        public float Attenuation { get; private set; }

        public Color3f Color { get; private set; }
    }
}
