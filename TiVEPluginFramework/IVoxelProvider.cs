namespace ProdigalSoftware.TiVEPluginFramework
{
    public interface IVoxelProvider
    {
        /// <summary>
        /// Gets the number of voxels on each axis
        /// </summary>
        Vector3i VoxelCount { get; }

        /// <summary>
        /// Gets the voxel at the specified location
        /// </summary>
        Voxel this[int x, int y, int z] { get; }
    }
}
