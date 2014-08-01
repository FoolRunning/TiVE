using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework.Lighting
{
    public interface ILight
    {
        float MaxDist { get; }

        Color4b GetColorAtDist(float dist);
    }
}
