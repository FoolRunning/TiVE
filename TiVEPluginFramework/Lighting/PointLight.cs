using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework.Lighting
{
    public sealed class PointLight : ILight
    {
        public PointLight(Vector3b location, Color4f color, float attenuation)
        {
            Location = location;
            Color = color;
            Attenuation = attenuation;
        }

        public Vector3b Location { get; private set; }

        public float Attenuation { get; private set; }

        public Color4f Color { get; private set; }
    }
}
