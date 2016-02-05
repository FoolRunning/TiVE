using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.RenderSystem.World
{
    /// <summary>
    /// Contains the information about the game world.
    /// </summary>
    internal sealed class GameWorld : IGameWorld
    {
        private const int BlockTotalVoxelCount = Block.VoxelSize * Block.VoxelSize * Block.VoxelSize;

        private readonly Vector3i voxelSize;
        private readonly Vector3i blockSize;
        private readonly ushort[] blocks;
        private readonly BlockState[] blockStates;
        private readonly Dictionary<ushort, string> blockIdToName = new Dictionary<ushort, string>();

        private Voxel[] blockVoxels;
        private bool[] blockVoxelsEmptyForLighting;
        private bool[] blockLightPassThrough;

        public GameWorld(int blockSizeX, int blockSizeY, int blockSizeZ)
        {
            LightingModelType = LightingModelType.Realistic;

            blockSize = new Vector3i(blockSizeX, blockSizeY, blockSizeZ);
            voxelSize = new Vector3i(blockSizeX * Block.VoxelSize, blockSizeY * Block.VoxelSize, blockSizeZ * Block.VoxelSize);

            blockStates = new BlockState[blockSizeX * blockSizeY * blockSizeZ];
            blocks = new ushort[blockSizeX * blockSizeY * blockSizeZ];
        }

        #region Implementation of IGameWorld
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
            get { return blocks[blockSize.GetArrayOffset(blockX, blockY, blockZ)]; }
            set { blocks[blockSize.GetArrayOffset(blockX, blockY, blockZ)] = value; }
        }
        #endregion

        #region Implementation of ITiVESerializable
        public void SaveTo(BinaryWriter writer)
        {
            
        }
        #endregion

        public void Initialize(BlockList blockList)
        {
            blockVoxelsEmptyForLighting = new bool[blockList.BlockCount * BlockTotalVoxelCount];
            blockVoxels = new Voxel[blockList.BlockCount * BlockTotalVoxelCount];
            blockLightPassThrough = new bool[blockList.BlockCount];
            for (int blockIndex = 0; blockIndex < blockList.BlockCount; blockIndex++)
            {
                BlockImpl block = blockList.AllBlocks[blockIndex];

                blockLightPassThrough[blockIndex] = (blockIndex == 0 || block.HasComponent<LightPassthroughComponent>());
                Array.Copy(block.VoxelsArray, 0, blockVoxels, blockIndex * BlockTotalVoxelCount, BlockTotalVoxelCount);

                int offset = blockIndex * BlockTotalVoxelCount;
                for (int i = 0; i < BlockTotalVoxelCount; i++)
                    blockVoxelsEmptyForLighting[offset + i] = (blockIndex == 0 || block.VoxelsArray[i].AllowLightPassthrough);
            }
        }

        public BlockState GetBlockState(int blockX, int blockY, int blockZ)
        {
            return blockStates[blockSize.GetArrayOffset(blockX, blockY, blockZ)];
        }

        public void SetBlockState(int blockX, int blockY, int blockZ, BlockState state)
        {
            blockStates[blockSize.GetArrayOffset(blockX, blockY, blockZ)] = state;
        }

        /// <summary>
        /// Gets the voxel in the game world at the specified absolute voxel location
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Voxel GetVoxel(int voxelX, int voxelY, int voxelZ)
        {
            MiscUtils.CheckConstraints(voxelX, voxelY, voxelZ, voxelSize);

            int blockX = voxelX >> Block.VoxelSizeBitShift;
            int blockY = voxelY >> Block.VoxelSizeBitShift;
            int blockZ = voxelZ >> Block.VoxelSizeBitShift;
            ushort block = blocks[blockSize.GetArrayOffset(blockX, blockY, blockZ)];
            if (block == 0)
                return Voxel.Empty;

            int blockVoxelX = voxelX % Block.VoxelSize;
            int blockVoxelY = voxelY % Block.VoxelSize;
            int blockVoxelZ = voxelZ % Block.VoxelSize;
            return blockVoxels[GetBlockVoxelsOffset(block, blockVoxelX, blockVoxelY, blockVoxelZ)];
        }

        /// <summary>
        /// Voxel transversal algorithm taken from: http://www.cse.chalmers.se/edu/year/2011/course/TDA361_Computer_Graphics/grid.pdf
        /// Modified with optimizations for TiVE.
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        public bool NoVoxelInLine(int x, int y, int z, int endX, int endY, int endZ)
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
            int blockX = x >> Block.VoxelSizeBitShift;
            int blockY = y >> Block.VoxelSizeBitShift;
            int blockZ = z >> Block.VoxelSizeBitShift;
            int blockVoxelX = x % Block.VoxelSize;
            int blockVoxelY = y % Block.VoxelSize;
            int blockVoxelZ = z % Block.VoxelSize;
            
            ushort[] blocksLocal = blocks;
            Vector3i blockSizeLocal = blockSize;
            bool[] blockVoxelsEmptyForLightingLocal = blockVoxelsEmptyForLighting;

            for (; ;)
            {
                if (tMaxX < tMaxY)
                {
                    if (tMaxX < tMaxZ)
                    {
                        x = x + stepX;
                        tMaxX = tMaxX + tStepX;
                        blockX = x >> Block.VoxelSizeBitShift;
                        blockVoxelX = x % Block.VoxelSize;
                    }
                    else
                    {
                        z = z + stepZ;
                        tMaxZ = tMaxZ + tStepZ;
                        blockZ = z >> Block.VoxelSizeBitShift;
                        blockVoxelZ = z % Block.VoxelSize;
                    }
                }
                else if (tMaxY < tMaxZ)
                {
                    y = y + stepY;
                    tMaxY = tMaxY + tStepY;
                    blockY = y >> Block.VoxelSizeBitShift;
                    blockVoxelY = y % Block.VoxelSize;
                }
                else
                {
                    z = z + stepZ;
                    tMaxZ = tMaxZ + tStepZ;
                    blockZ = z >> Block.VoxelSizeBitShift;
                    blockVoxelZ = z % Block.VoxelSize;
                }

                if (x == endX && y == endY && z == endZ)
                    return true;

                ushort block = blocksLocal[blockSizeLocal.GetArrayOffset(blockX, blockY, blockZ)];
                if (block != 0 && !blockVoxelsEmptyForLightingLocal[GetBlockVoxelsOffset(block, blockVoxelX, blockVoxelY, blockVoxelZ)])
                    return false;
            }
        }

        /// <summary>
        /// Taken from ftp://ftp.isc.org/pub/usenet/comp.sources.unix/volume26/line3d originally created by Bob Pendelton
        /// Modified with optimizations for TiVE.
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        public bool NoVoxelInLineFast(int x1, int y1, int z1, int x2, int y2, int z2)
        {
            if (x1 == x2 && y1 == y2 && z1 == z2)
                return true;

            int dx = x2 - x1;
            int dy = y2 - y1;
            int dz = z2 - z1;

            int ax = FastAbs(dx) << 1;
            int ay = FastAbs(dy) << 1;
            int az = FastAbs(dz) << 1;

            int sx = FastSign(dx);
            int sy = FastSign(dy);
            int sz = FastSign(dz);

            int x = x1;
            int y = y1;
            int z = z1;

            int blockX = x >> Block.VoxelSizeBitShift;
            int blockY = y >> Block.VoxelSizeBitShift;
            int blockZ = z >> Block.VoxelSizeBitShift;
            int blockVoxelX = x % Block.VoxelSize;
            int blockVoxelY = y % Block.VoxelSize;
            int blockVoxelZ = z % Block.VoxelSize;
            ushort[] blocksLocal = blocks;
            Vector3i blockSizeLocal = blockSize;
            bool[] blockVoxelsEmptyForLightingLocal = blockVoxelsEmptyForLighting;

            ushort block;
            int xd, yd, zd;
            if (ax >= Math.Max(ay, az))            /* x dominant */
            {
                yd = ay - (ax >> 1);
                zd = az - (ax >> 1);
                for (; ;)
                {
                    if (yd >= 0)
                    {
                        y += sy;
                        yd -= ax;
                        blockY = y >> Block.VoxelSizeBitShift;
                        blockVoxelY = y % Block.VoxelSize;
                    }

                    if (zd >= 0)
                    {
                        z += sz;
                        zd -= ax;
                        blockZ = z >> Block.VoxelSizeBitShift;
                        blockVoxelZ = z % Block.VoxelSize;
                    }

                    x += sx;
                    blockX = x >> Block.VoxelSizeBitShift;
                    blockVoxelX = x % Block.VoxelSize;

                    yd += ay;
                    zd += az;

                    if (x == x2)
                        return true;

                    block = blocksLocal[blockSizeLocal.GetArrayOffset(blockX, blockY, blockZ)];
                    if (block != 0 && !blockVoxelsEmptyForLightingLocal[GetBlockVoxelsOffset(block, blockVoxelX, blockVoxelY, blockVoxelZ)])
                        return false;
                }
            }

            if (ay >= Math.Max(ax, az))            /* y dominant */
            {
                xd = ax - (ay >> 1);
                zd = az - (ay >> 1);
                for (; ;)
                {
                    if (xd >= 0)
                    {
                        x += sx;
                        xd -= ay;
                        blockX = x >> Block.VoxelSizeBitShift;
                        blockVoxelX = x % Block.VoxelSize;
                    }

                    if (zd >= 0)
                    {
                        z += sz;
                        zd -= ay;
                        blockZ = z >> Block.VoxelSizeBitShift;
                        blockVoxelZ = z % Block.VoxelSize;
                    }

                    y += sy;
                    blockY = y >> Block.VoxelSizeBitShift;
                    blockVoxelY = y % Block.VoxelSize;

                    xd += ax;
                    zd += az;

                    if (y == y2)
                        return true;

                    block = blocksLocal[blockSizeLocal.GetArrayOffset(blockX, blockY, blockZ)];
                    if (block != 0 && !blockVoxelsEmptyForLightingLocal[GetBlockVoxelsOffset(block, blockVoxelX, blockVoxelY, blockVoxelZ)])
                        return false;
                }
            }

            /* z dominant */
            xd = ax - (az >> 1);
            yd = ay - (az >> 1);
            for (; ;)
            {
                if (xd >= 0)
                {
                    x += sx;
                    xd -= az;
                    blockX = x >> Block.VoxelSizeBitShift;
                    blockVoxelX = x % Block.VoxelSize;
                }

                if (yd >= 0)
                {
                    y += sy;
                    yd -= az;
                    blockY = y >> Block.VoxelSizeBitShift;
                    blockVoxelY = y % Block.VoxelSize;
                }

                z += sz;
                blockZ = z >> Block.VoxelSizeBitShift;
                blockVoxelZ = z % Block.VoxelSize;

                xd += ax;
                yd += ay;

                if (z == z2)
                    return true;

                block = blocksLocal[blockSizeLocal.GetArrayOffset(blockX, blockY, blockZ)];
                if (block != 0 && !blockVoxelsEmptyForLightingLocal[GetBlockVoxelsOffset(block, blockVoxelX, blockVoxelY, blockVoxelZ)])
                    return false;
            }
        }

        /// <summary>
        /// Taken from ftp://ftp.isc.org/pub/usenet/comp.sources.unix/volume26/line3d originally created by Bob Pendelton
        /// Modified with optimizations for TiVE.
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        public bool NoBlocksInLine(int x1, int y1, int z1, int x2, int y2, int z2)
        {
            if (x1 == x2 && y1 == y2 && z1 == z2)
                return true;

            int dx = x2 - x1;
            int dy = y2 - y1;
            int dz = z2 - z1;

            int ax = FastAbs(dx) << 1;
            int ay = FastAbs(dy) << 1;
            int az = FastAbs(dz) << 1;

            int sx = FastSign(dx);
            int sy = FastSign(dy);
            int sz = FastSign(dz);

            int x = x1;
            int y = y1;
            int z = z1;
            ushort[] blocksLocal = blocks;
            Vector3i blockSizeLocal = blockSize;
            bool[] blockLightPassThroughLocal = blockLightPassThrough;

            int xd, yd, zd;
            if (ax >= Math.Max(ay, az))            /* x dominant */
            {
                yd = ay - (ax >> 1);
                zd = az - (ax >> 1);
                for (;;)
                {
                    if (yd >= 0)
                    {
                        y += sy;
                        yd -= ax;
                    }

                    if (zd >= 0)
                    {
                        z += sz;
                        zd -= ax;
                    }

                    x += sx;
                    yd += ay;
                    zd += az;

                    if (x == x2)
                        return true;

                    ushort block = blocksLocal[blockSizeLocal.GetArrayOffset(x, y, z)];
                    if (block != 0 && !blockLightPassThroughLocal[block])
                        return false;
                }
            }
            
            if (ay >= Math.Max(ax, az))            /* y dominant */
            {
                xd = ax - (ay >> 1);
                zd = az - (ay >> 1);
                for (;;)
                {
                    if (xd >= 0)
                    {
                        x += sx;
                        xd -= ay;
                    }

                    if (zd >= 0)
                    {
                        z += sz;
                        zd -= ay;
                    }

                    y += sy;
                    xd += ax;
                    zd += az;

                    if (y == y2)
                        return true;

                    ushort block = blocksLocal[blockSizeLocal.GetArrayOffset(x, y, z)];
                    if (block != 0 && !blockLightPassThroughLocal[block])
                        return false;
                }
            }

            /* z dominant */
            xd = ax - (az >> 1);
            yd = ay - (az >> 1);
            for (;;)
            {
                if (xd >= 0)
                {
                    x += sx;
                    xd -= az;
                }

                if (yd >= 0)
                {
                    y += sy;
                    yd -= az;
                }

                z += sz;
                xd += ax;
                yd += ay;

                if (z == z2)
                    return true;

                ushort block = blocksLocal[blockSizeLocal.GetArrayOffset(x, y, z)];
                if (block != 0 && !blockLightPassThroughLocal[block])
                    return false;
            }
        }

        #region Private helper methods
        /// <summary>
        /// Gets the offset into the blocks voxels array
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetBlockVoxelsOffset(ushort blockIndex, int x, int y, int z)
        {
            Debug.Assert(x >= 0 && x < Block.VoxelSize);
            Debug.Assert(y >= 0 && y < Block.VoxelSize);
            Debug.Assert(z >= 0 && z < Block.VoxelSize);

            return blockIndex * BlockTotalVoxelCount + (((z << Block.VoxelSizeBitShift) + x) << Block.VoxelSizeBitShift) + y; // y-axis major for speed
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FastAbs(int value)
        {
            return value >= 0 ? value : -value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FastSign(int value)
        {
            return value == 0 ? 0 : (value < 0 ? -1 : 1);
        }
        #endregion
    }
}
