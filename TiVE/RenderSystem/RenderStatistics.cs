namespace ProdigalSoftware.TiVE.RenderSystem
{
    /// <summary>
    /// Holds information about the current render frame
    /// </summary>
    internal struct RenderStatistics
    {
        /// <summary>
        /// The number of draw calls to the renderer backend
        /// </summary>
        public readonly int DrawCount;
        /// <summary>
        /// The total number of voxels that are represented
        /// </summary>
        public readonly int VoxelCount;
        /// <summary>
        /// The number of voxels that were rendered
        /// </summary>
        public readonly int RenderedVoxelCount;

        /// <summary>
        /// Creates a new RenderStatistics with the specified information
        /// </summary>
        /// <param name="drawCount">The number of draw calls to the renderer backend</param>
        /// <param name="voxelCount">The total number of voxels that are represented</param>
        /// <param name="renderedVoxelCount">The number of voxels that were rendered</param>
        public RenderStatistics(int drawCount, int voxelCount, int renderedVoxelCount)
        {
            DrawCount = drawCount;
            VoxelCount = voxelCount;
            RenderedVoxelCount = renderedVoxelCount;
        }

        /// <summary>
        /// Adds the specified RenderStatistics together and returns the results
        /// </summary>
        public static RenderStatistics operator +(RenderStatistics r1, RenderStatistics r2)
        {
            return new RenderStatistics(r1.DrawCount + r2.DrawCount, r1.VoxelCount + r2.VoxelCount, r1.RenderedVoxelCount + r2.RenderedVoxelCount);
        }
    }
}
