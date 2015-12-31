using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public enum LightingModelType
    {
        Realistic,
        BrightRealistic,
        Fantasy1,
        Fantasy2
    }

    public interface IGameWorld
    {
        LightingModelType LightingModelType { get; set; }

        /// <summary>
        /// Gets the voxel size of the game world
        /// </summary>
        Vector3i VoxelSize { get; }

        /// <summary>
        /// Gets the size of the game world in blocks
        /// </summary>
        Vector3i BlockSize { get; }

        /// <summary>
        /// Gets/sets the ID of the block in the game world at the specified block location
        /// </summary>
        ushort this[int blockX, int blockY, int blockZ] { get; set; }
    }
}
