using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
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

    /// <summary>
    /// Contains the information about the game world.
    /// </summary>
    public sealed class GameWorld
    {
        private const int BlockTotalVoxelCount = BlockInformation.VoxelSize * BlockInformation.VoxelSize * BlockInformation.VoxelSize;

        private readonly Vector3i voxelSize;
        private readonly Vector3i blockSize;
        private readonly ushort[] blocks;
        private readonly BlockState[] blockStates;

        private uint[] blockVoxels;
        private uint[] blockVoxelsForLighting;

        public GameWorld(int blockSizeX, int blockSizeY, int blockSizeZ)
        {
            LightingModelType = LightingModelType.Realistic;

            blockSize = new Vector3i(blockSizeX, blockSizeY, blockSizeZ);
            voxelSize = new Vector3i(blockSizeX * BlockInformation.VoxelSize, blockSizeY * BlockInformation.VoxelSize, blockSizeZ * BlockInformation.VoxelSize);

            blockStates = new BlockState[blockSizeX * blockSizeY * blockSizeZ];
            blocks = new ushort[blockSizeX * blockSizeY * blockSizeZ];
        }

        [UsedImplicitly]
        public LightingModelType LightingModelType { get; set; }

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
        public ushort this[int blockX, int blockY, int blockZ]
        {
            get { return blocks[GetBlockOffset(blockX, blockY, blockZ)]; }
            set { blocks[GetBlockOffset(blockX, blockY, blockZ)] = value; }
        }

        internal void Initialize(IBlockListInternal blockList)
        {
            blockVoxelsForLighting = new uint[blockList.BlockCount * BlockTotalVoxelCount];
            blockVoxels = new uint[blockList.BlockCount * BlockTotalVoxelCount];
            for (int i = 0; i < blockList.BlockCount; i++)
            {
                BlockInformation block = blockList.AllBlocks[i];
                Array.Copy(block.VoxelsArray, 0, blockVoxels, i * BlockTotalVoxelCount, BlockTotalVoxelCount);
                if (block.Light == null)
                    Array.Copy(block.VoxelsArray, 0, blockVoxelsForLighting, i * BlockTotalVoxelCount, BlockTotalVoxelCount);
            }
        }

        public BlockState GetBlockState(int blockX, int blockY, int blockZ)
        {
            return blockStates[GetBlockOffset(blockX, blockY, blockZ)];
        }

        public void SetBlockState(int blockX, int blockY, int blockZ, BlockState state)
        {
            blockStates[GetBlockOffset(blockX, blockY, blockZ)] = state;
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
            ushort block = blocks[GetBlockOffset(blockX, blockY, blockZ)];
            if (block == 0)
                return 0;

            int blockVoxelX = voxelX % BlockInformation.VoxelSize;
            int blockVoxelY = voxelY % BlockInformation.VoxelSize;
            int blockVoxelZ = voxelZ % BlockInformation.VoxelSize;
            return blockVoxels[GetBlockOffset(block, blockVoxelX, blockVoxelY, blockVoxelZ)];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool VoxelEmptyForLighting(int voxelX, int voxelY, int voxelZ)
        {
            MiscUtils.CheckConstraints(voxelX, voxelY, voxelZ, voxelSize);

            int blockX = voxelX / BlockInformation.VoxelSize;
            int blockY = voxelY / BlockInformation.VoxelSize;
            int blockZ = voxelZ / BlockInformation.VoxelSize;
            ushort block = blocks[GetBlockOffset(blockX, blockY, blockZ)];
            if (block == 0)
                return true;

            int blockVoxelX = voxelX % BlockInformation.VoxelSize;
            int blockVoxelY = voxelY % BlockInformation.VoxelSize;
            int blockVoxelZ = voxelZ % BlockInformation.VoxelSize;
            return blockVoxelsForLighting[GetBlockOffset(block, blockVoxelX, blockVoxelY, blockVoxelZ)] == 0;
        }

        /// <summary>
        /// Voxel transversal algorithm taken from: http://www.cse.chalmers.se/edu/year/2011/course/TDA361_Computer_Graphics/grid.pdf
        /// Modified with small optimizations for TiVE.
        /// </summary>
        internal bool NoVoxelInLine(int x, int y, int z, int endX, int endY, int endZ)
        {
            if (x == endX && y == endY && z == endZ)
                return true;

            int stepX = x > endX ? -1 : 1;
            int stepY = y > endY ? -1 : 1;
            int stepZ = z > endZ ? -1 : 1;

            // Because all voxels in TiVE have a size of 1.0, this simplifies the calculation of the delta considerably.
            // We also don't have to worry about specifically handling a divide-by-zero as .Net makes the result Infinity
            // which works just fine for this algorithm.
            float tStepX = (float)stepX / (endX - x);
            float tStepY = (float)stepY / (endY - y);
            float tStepZ = (float)stepZ / (endZ - z);
            float tMaxX = tStepX;
            float tMaxY = tStepY;
            float tMaxZ = tStepZ;

            //int blockX = x / BlockInformation.VoxelSize;
            //int blockY = y / BlockInformation.VoxelSize;
            //int blockZ = z / BlockInformation.VoxelSize;

            do
            {
                if (tMaxX < tMaxY)
                {
                    if (tMaxX < tMaxZ)
                    {
                        x = x + stepX;
                        tMaxX = tMaxX + tStepX;
                    }
                    else
                    {
                        z = z + stepZ;
                        tMaxZ = tMaxZ + tStepZ;
                    }
                }
                else if (tMaxY < tMaxZ)
                {
                    y = y + stepY;
                    tMaxY = tMaxY + tStepY;
                }
                else
                {
                    z = z + stepZ;
                    tMaxZ = tMaxZ + tStepZ;
                }

                if (x == endX && y == endY && z == endZ)
                    return true;
            }
            while (VoxelEmptyForLighting(x, y, z));

            return false;
        }

        #region Private helper methods
        /// <summary>
        /// Gets the offset into the game world blocks array for the block at the specified location
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlockOffset(int x, int y, int z)
        {
            Vector3i size = blockSize;
            MiscUtils.CheckConstraints(x, y, z, size);
            return (x * size.Z + z) * size.Y + y; // y-axis major for speed
        }

        /// <summary>
        /// Gets the offset into the blocks voxels array
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetBlockOffset(ushort blockIndex, int x, int y, int z)
        {
            Debug.Assert(x >= 0 && x < BlockInformation.VoxelSize);
            Debug.Assert(y >= 0 && y < BlockInformation.VoxelSize);
            Debug.Assert(z >= 0 && z < BlockInformation.VoxelSize);

            return blockIndex * BlockTotalVoxelCount + (z * BlockInformation.VoxelSize + x) * BlockInformation.VoxelSize + y; // y-axis major for speed
        }
        #endregion
    }
}
