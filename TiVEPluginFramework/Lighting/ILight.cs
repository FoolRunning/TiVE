using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework.Lighting
{
    public interface ILight
    {
        Vector3b Location { get; }

        Color4f Color { get; }

        float Attenuation { get; }
    }
}
