using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    /// <summary>
    /// Represents a block of the game world. Each block is made up of a 20x20x20 voxel cube.
    /// See <see cref="GameWorld"/> for how blocks are used.
    /// </summary>
    internal class Block : VoxelGroup
    {
        public Block(BlockInformation blockInfo) : base(blockInfo.Voxels)
        {
            //for (int x = 0; x < BlockInformation.BlockSize; x++)
            //{
            //    for (int y = 0; y < BlockInformation.BlockSize; y++)
            //    {
            //        for (int z = 0; z < BlockInformation.BlockSize; z++)
            //        {
            //            uint voxel = blockInfo.Voxels[x, y, z];
            //            SetVoxel(x, y, z, voxel);
            //        }
            //    }
            //}

            BlockInfo = blockInfo;
        }

        public BlockInformation BlockInfo { get; private set; }
    }
}
