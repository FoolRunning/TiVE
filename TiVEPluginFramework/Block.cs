using System.IO;
using JetBrains.Annotations;

namespace ProdigalSoftware.TiVEPluginFramework
{
    [PublicAPI]
    public abstract class Block : ITiVESerializable
    {
        /// <summary>Number of voxels that make up a block on each axis (must be a power-of-two)</summary>
        public const int VoxelSize = 16;
        /// <summary>Number of bit shifts neccessary on a number to produce the number of voxels on each axis.
        /// This allows us to quickly multiply or divide a value by the voxel size by bitshifting</summary>
        public const int VoxelSizeBitShift = 4;

        public abstract string Name { get; }

        /// <summary>
        /// Gets/sets the voxel at the specified position within the block
        /// </summary>
        public abstract Voxel this[int x, int y, int z] { get; set; }

        public abstract void AddComponent(IBlockComponent component);

        public abstract void RemoveComponent<T>() where T : class, IBlockComponent;

        public abstract bool HasComponent<T>() where T : class, IBlockComponent;

        public abstract T GetComponent<T>() where T : class, IBlockComponent;

        public abstract Block CreateRotated(BlockRotation rotation);

        public abstract void SaveTo(BinaryWriter writer);
    }

    public interface IBlockComponent : ITiVESerializable
    {
    }
}
