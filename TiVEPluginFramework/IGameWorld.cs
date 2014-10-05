using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public interface IGameWorld
    {
        Vector3i BlockSize { get; }

        BlockInformation this[int blockX, int blockY, int blockZ] { get; set; }
    }
}
