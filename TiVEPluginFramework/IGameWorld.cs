using JetBrains.Annotations;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public enum LightingModelType
    {
        Realistic,
        BrightRealistic,
        Fantasy1,
        Fantasy2,
        Fantasy3
    }

    [PublicAPI]
    public interface IGameWorld : ITiVESerializable
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
        /// Gets/sets the block in the game world at the specified block location
        /// </summary>
        Block this[int blockX, int blockY, int blockZ] { get; set; }
    }
}
