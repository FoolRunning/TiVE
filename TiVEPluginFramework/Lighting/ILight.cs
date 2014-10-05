using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework.Lighting
{
    public interface ILight
    {
        float MaxVoxelDist { get; }

        Vector3b Location { get; }

        Color4f GetColorAtDistSquared(float dist);
    }
}
