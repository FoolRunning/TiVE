namespace ProdigalSoftware.TiVEPluginFramework
{
    public abstract class Block
    {
        /// <summary>Number of voxels that make up a block on each axis</summary>
        public const int VoxelSize = 16;

        public abstract string BlockName { get; }

        /// <summary>
        /// Gets/sets the voxel at the specified position within the block
        /// </summary>
        public abstract uint this[int x, int y, int z] { get; set; }

        public abstract void AddComponent(IBlockComponent component);

        public abstract void RemoveComponent<T>() where T : IBlockComponent;

        public abstract bool HasComponent<T>() where T : IBlockComponent;

        public abstract T GetComponent<T>() where T : IBlockComponent;

        public abstract Block CreateRotated(BlockRotation rotation);
    }

    public interface IBlockComponent
    {
    }
}
