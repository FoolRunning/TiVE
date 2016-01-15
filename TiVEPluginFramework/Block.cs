using JetBrains.Annotations;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public abstract class Block
    {
        /// <summary>Number of voxels that make up a block on each axis (must be a power-of-two)</summary>
        [PublicAPI]
        public const int VoxelSize = 16;
        /// <summary>Number of bit shifts neccessary on a number to produce the number of voxels on each axis.
        /// This allows us to quickly multiply or divide a value by the voxel size by bitshifting</summary>
        [PublicAPI]
        public const int VoxelSizeBitShift = 4;

        [PublicAPI]
        public abstract string Name { get; }

        /// <summary>
        /// Gets/sets the voxel at the specified position within the block
        /// </summary>
        [PublicAPI]
        public abstract Voxel this[int x, int y, int z] { get; set; }

        [PublicAPI]
        public abstract void AddComponent(IBlockComponent component);

        [PublicAPI]
        public abstract void RemoveComponent<T>() where T : class, IBlockComponent;

        [PublicAPI]
        public abstract bool HasComponent(IBlockComponent component);

        [PublicAPI]
        public abstract bool HasComponent<T>() where T : class, IBlockComponent;

        [PublicAPI]
        public abstract T GetComponent<T>() where T : class, IBlockComponent;

        [PublicAPI]
        public abstract Block CreateRotated(BlockRotation rotation);
    }

    public interface IBlockComponent
    {
    }
}
