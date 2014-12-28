using System.Runtime.CompilerServices;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework
{
    /// <summary>
    /// Contains the information about the game world.
    /// </summary>
    public sealed class GameWorld
    {
        private readonly Vector3i voxelSize;
        private readonly Vector3i blockSize;
        private readonly Block[] worldBlocks;
        //private readonly BlockList blockList; // TODO: This was only for animated blocks and is not accessible here. Fix this!

        public GameWorld(int blockSizeX, int blockSizeY, int blockSizeZ/*, BlockList blockList*/)
        {
            //this.blockList = blockList;

            blockSize = new Vector3i(blockSizeX, blockSizeY, blockSizeZ);
            voxelSize = new Vector3i(blockSizeX * BlockInformation.VoxelSize, blockSizeY * BlockInformation.VoxelSize, blockSizeZ * BlockInformation.VoxelSize);

            worldBlocks = new Block[blockSizeX * blockSizeY * blockSizeZ];
            for (int i = 0; i < worldBlocks.Length; i++)
                worldBlocks[i] = new Block(BlockInformation.Empty);
        }

        /// <summary>
        /// Gets the voxel size of the game world
        /// </summary>
        public Vector3i VoxelSize
        {
            get { return voxelSize; }
        }

        /// <summary>
        /// Gets the size of the game world in blocks
        /// </summary>
        public Vector3i BlockSize
        {
            get { return blockSize; }
        }

        /// <summary>
        /// Gets/sets the block in the game world at the specified block location
        /// </summary>
        public BlockInformation this[int blockX, int blockY, int blockZ]
        {
            get { return worldBlocks[GetBlockOffset(blockX, blockY, blockZ)].BlockInfo; }
            set { worldBlocks[GetBlockOffset(blockX, blockY, blockZ)].BlockInfo = value ?? BlockInformation.Empty; }
        }

        public BlockState GetBlockState(int blockX, int blockY, int blockZ)
        {
            return worldBlocks[GetBlockOffset(blockX, blockY, blockZ)].State;
        }

        public void SetBlockState(int blockX, int blockY, int blockZ, BlockState state)
        {
            worldBlocks[GetBlockOffset(blockX, blockY, blockZ)].State = state;
        }

        /// <summary>
        /// Gets the voxel in the game world at the specified absolute voxel location
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetVoxel(int voxelX, int voxelY, int voxelZ)
        {
            MiscUtils.CheckConstraints(voxelX, voxelY, voxelZ, voxelSize);

            int blockX = voxelX / BlockInformation.VoxelSize;
            int blockY = voxelY / BlockInformation.VoxelSize;
            int blockZ = voxelZ / BlockInformation.VoxelSize;

            int blockVoxelX = voxelX % BlockInformation.VoxelSize;
            int blockVoxelY = voxelY % BlockInformation.VoxelSize;
            int blockVoxelZ = voxelZ % BlockInformation.VoxelSize;

            BlockInformation block = worldBlocks[GetBlockOffset(blockX, blockY, blockZ)].BlockInfo;
            return /*blockList.BelongsToAnimation(block) ? 0 :*/ block[blockVoxelX, blockVoxelY, blockVoxelZ];
        }

        #region Private helper methods
        /// <summary>
        /// Gets the offset into the game world blocks array for the block at the specified location
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlockOffset(int x, int y, int z)
        {
            MiscUtils.CheckConstraints(x, y, z, blockSize);
            return (x * blockSize.Z + z) * blockSize.Y + y; // y-axis major for speed
        }
        #endregion

        #region Block class
        /// <summary>
        /// Represents one block in the game world
        /// </summary>
        private struct Block
        {
            /// <summary>
            /// Information about the block
            /// </summary>
            public BlockInformation BlockInfo;

            /// <summary>
            /// Information about the state of the block
            /// </summary>
            public BlockState State;

            public Block(BlockInformation blockInfo)
            {
                BlockInfo = blockInfo;
                State = new BlockState();
            }
        }
        #endregion
    }
}
