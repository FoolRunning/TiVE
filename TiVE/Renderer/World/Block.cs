using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    /// <summary>
    /// Represents a block of the game world. Each block is made up of a 16x16x16 voxel cube.
    /// See <see cref="GameWorld"/> for how blocks are used.
    /// </summary>
    internal class Block_Old : IndexedVoxelGroup
    {
        public Block_Old(BlockInformation blockInfo) : base(blockInfo.Voxels)
        {
            BlockInfo = blockInfo;
        }

        public BlockInformation BlockInfo { get; private set; }
    }
}
