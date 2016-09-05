using System;
using System.Collections.Generic;
using System.IO;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.RenderSystem.World
{
    /// <summary>
    /// Contains the information about the game world.
    /// </summary>
    [MoonSharpUserData]
    internal sealed class GameWorld : IGameWorld
    {
        #region Constants/Member variables
        [MoonSharpVisible(false)]
        public static readonly Guid ID = new Guid("1DDA35E9-DE25-4033-B20E-57B8626A56BA");
        private const byte SerializedFileVersion = 1;

        private readonly Vector3i voxelSize32;
        private readonly Vector3i blockSize;
        private readonly ushort[] gameWorldBlocks;
        //private readonly BlockState[] blockStates;
        private readonly Dictionary<string, ushort> blockNameToId = new Dictionary<string, ushort>(1000);

        //private bool[] blockVoxelsEmptyForLighting;
        private Block[] blockIdToBlock = new Block[2000];
        private bool[] blockLightPassThrough;
        private int blockIdCount = 1;
        #endregion

        #region Constructors
        internal GameWorld(BinaryReader reader)
        {
            byte fileVersion = reader.ReadByte();
            if (fileVersion > SerializedFileVersion)
                throw new FileTooNewException("GameWorld");

            // Read block index
            int indexCount = reader.ReadUInt16();
            blockIdToBlock = new Block[indexCount + 100];
            for (int i = 0; i < indexCount; i++)
            {
                string blockName = reader.ReadString();
                blockNameToId.Add(blockName, (ushort)blockIdCount);
                blockIdToBlock[blockIdCount++] = TiVEController.BlockManager.GetBlock(blockName);
            }

            // Read block data
            blockSize = new Vector3i(reader);
            voxelSize32 = new Vector3i(blockSize.X * BlockLOD32.VoxelSize, blockSize.Y * BlockLOD32.VoxelSize, blockSize.Z * BlockLOD32.VoxelSize);
            //blockStates = new BlockState[blockSize.X * blockSize.Y * blockSize.Z];
            gameWorldBlocks = new ushort[blockSize.X * blockSize.Y * blockSize.Z];
            for (int i = 0; i < gameWorldBlocks.Length; i++)
            {
                gameWorldBlocks[i] = reader.ReadUInt16();
                //blockStates[i] = new BlockState(reader);
            }
        }

        public GameWorld(int blockSizeX, int blockSizeY, int blockSizeZ)
        {
            LightingModelType = LightingModelType.Realistic;

            blockSize = new Vector3i(blockSizeX, blockSizeY, blockSizeZ);
            voxelSize32 = new Vector3i(blockSizeX * BlockLOD32.VoxelSize, blockSizeY * BlockLOD32.VoxelSize, blockSizeZ * BlockLOD32.VoxelSize);
            //blockStates = new BlockState[blockSizeX * blockSizeY * blockSizeZ];
            gameWorldBlocks = new ushort[blockSizeX * blockSizeY * blockSizeZ];
            blockIdToBlock[0] = Block.Empty;
            blockNameToId.Add(Block.Empty.Name, 0);
        }
        #endregion

        internal int BlockTypeCount
        {
            get { return blockIdCount; }
        }

        #region Implementation of IGameWorld
        public LightingModelType LightingModelType { get; set; }

        /// <summary>
        /// Gets the voxel size of the game world
        /// </summary>
        public Vector3i VoxelSize32
        {
            get { return voxelSize32; }
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
        public Block this[int blockX, int blockY, int blockZ]
        {
            get 
            {
                ushort blockId = gameWorldBlocks[blockSize.GetArrayOffset(blockX, blockY, blockZ)];
                return blockIdToBlock[blockId];
            }
            set 
            {
                ushort blockId;
                if (!blockNameToId.TryGetValue(value.Name, out blockId))
                {
                    blockNameToId[value.Name] = blockId = (ushort)blockIdCount;
                    if (blockIdCount >= blockIdToBlock.Length)
                        Array.Resize(ref blockIdToBlock, blockIdToBlock.Length * 3 / 2 + 1);

                    blockIdToBlock[blockIdCount++] = value;
                }
                gameWorldBlocks[blockSize.GetArrayOffset(blockX, blockY, blockZ)] = blockId;
            }
        }
        #endregion

        public Vector3us FindBlock(Block block)
        {
            ushort blockId;
            if (!blockNameToId.TryGetValue(block.Name, out blockId))
                return new Vector3us();

            Vector3us foundLocation = new Vector3us();
            for (int z = 0; z < blockSize.Z; z++)
            {
                for (int x = 0; x < blockSize.X; x++)
                {
                    for (int y = 0; y < blockSize.Y; y++)
                    {
                        if (gameWorldBlocks[blockSize.GetArrayOffset(x, y, z)] == blockId)
                            foundLocation = new Vector3us(x, y, z);
                    }
                }
            }

            return foundLocation;
        }

        #region Implementation of ITiVESerializable
        [MoonSharpVisible(false)]
        public void SaveTo(BinaryWriter writer)
        {
            writer.Write(SerializedFileVersion);

            // Save block index
            writer.Write((ushort)blockNameToId.Count);
            for (int i = 0; i < blockIdCount; i++)
                writer.Write(blockIdToBlock[i].Name);

            // Save block data
            blockSize.SaveTo(writer);
            for (int i = 0; i < gameWorldBlocks.Length; i++)
            {
                writer.Write(gameWorldBlocks[i]);
                //blockStates[i].SaveTo(writer);
            }
        }
        #endregion

        #region Other internal methods
        internal void Initialize()
        {
            blockLightPassThrough = new bool[blockIdCount];
            for (int blockId = 0; blockId < blockIdCount; blockId++)
            {
                Block block = blockIdToBlock[blockId];
                blockLightPassThrough[blockId] = (blockId == 0 || block.HasComponent<LightPassthroughComponent>());
            }
        }

        //internal BlockState GetBlockState(int blockX, int blockY, int blockZ)
        //{
        //    return blockStates[blockSize.GetArrayOffset(blockX, blockY, blockZ)];
        //}

        //internal void SetBlockState(int blockX, int blockY, int blockZ, BlockState state)
        //{
        //    blockStates[blockSize.GetArrayOffset(blockX, blockY, blockZ)] = state;
        //}

        /// <summary>
        /// Gets the voxel in the game world at the specified absolute voxel location
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        internal Voxel GetVoxel(int voxelX, int voxelY, int voxelZ, LODLevel detailLevel)
        {
            switch (detailLevel)
            {
                case LODLevel.V32: return GetVoxelHelper32(voxelX, voxelY, voxelZ);
                case LODLevel.V16: return GetVoxelHelper16(voxelX, voxelY, voxelZ);
                case LODLevel.V8: return GetVoxelHelper8(voxelX, voxelY, voxelZ);
                case LODLevel.V4: return GetVoxelHelper4(voxelX, voxelY, voxelZ);
                default: throw new ArgumentException("detailLevel invalid: " + detailLevel);
            }
        }

        /// <summary>
        /// Voxel transversal algorithm taken from: http://www.cse.chalmers.se/edu/year/2011/course/TDA361_Computer_Graphics/grid.pdf
        /// Modified with optimizations for TiVE.
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        internal bool NoVoxelInLine(int x, int y, int z, int endX, int endY, int endZ, LODLevel detailLevel)
        {
            switch (detailLevel)
            {
                case LODLevel.V32: return NoVoxelInLineHelper32(x, y, z, endX, endY, endZ);
                case LODLevel.V16: return NoVoxelInLineHelper16(x, y, z, endX, endY, endZ);
                case LODLevel.V8: return NoVoxelInLineHelper8(x, y, z, endX, endY, endZ);
                case LODLevel.V4: return NoVoxelInLineHelper4(x, y, z, endX, endY, endZ);
                default: throw new ArgumentException("detailLevel invalid: " + detailLevel);
            }
        }

        /// <summary>
        /// Taken from ftp://ftp.isc.org/pub/usenet/comp.sources.unix/volume26/line3d originally created by Bob Pendelton
        /// Modified with optimizations for TiVE.
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        internal bool NoBlocksInLine(int x, int y, int z, int x2, int y2, int z2)
        {
            if (x == x2 && y == y2 && z == z2)
                return true;

            int dx = x2 - x;
            int dy = y2 - y;
            int dz = z2 - z;

            int ax = TiVEUtils.FastAbs(dx) << 1;
            int ay = TiVEUtils.FastAbs(dy) << 1;
            int az = TiVEUtils.FastAbs(dz) << 1;

            int sx = TiVEUtils.FastSign(dx);
            int sy = TiVEUtils.FastSign(dy);
            int sz = TiVEUtils.FastSign(dz);

            ushort[] blocksLocal = gameWorldBlocks;
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
        #endregion

        #region GetVoxel performance implementations
        /// <summary>
        /// Gets the voxel in the game world at the specified absolute voxel location
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        private Voxel GetVoxelHelper32(int voxelX, int voxelY, int voxelZ)
        {
            MiscUtils.CheckConstraints(voxelX, voxelY, voxelZ, voxelSize32);

            int blockX = voxelX >> BlockLOD32.VoxelSizeBitShift;
            int blockY = voxelY >> BlockLOD32.VoxelSizeBitShift;
            int blockZ = voxelZ >> BlockLOD32.VoxelSizeBitShift;
            ushort blockId = gameWorldBlocks[blockSize.GetArrayOffset(blockX, blockY, blockZ)];
            if (blockId == 0)
                return Voxel.Empty;

            int blockVoxelX = voxelX & BlockLOD32.MagicModulusNumber;
            int blockVoxelY = voxelY & BlockLOD32.MagicModulusNumber;
            int blockVoxelZ = voxelZ & BlockLOD32.MagicModulusNumber;
            return blockIdToBlock[blockId].LOD32[blockVoxelX, blockVoxelY, blockVoxelZ];
        }

        /// <summary>
        /// Gets the voxel in the game world at the specified absolute voxel location
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        private Voxel GetVoxelHelper16(int voxelX, int voxelY, int voxelZ)
        {
            MiscUtils.CheckConstraints(voxelX, voxelY, voxelZ, LODUtils.AdjustLocationForDetailLevelFrom32(voxelSize32, LODLevel.V16));

            int blockX = voxelX >> BlockLOD16.VoxelSizeBitShift;
            int blockY = voxelY >> BlockLOD16.VoxelSizeBitShift;
            int blockZ = voxelZ >> BlockLOD16.VoxelSizeBitShift;
            ushort blockId = gameWorldBlocks[blockSize.GetArrayOffset(blockX, blockY, blockZ)];
            if (blockId == 0)
                return Voxel.Empty;

            int blockVoxelX = voxelX & BlockLOD16.MagicModulusNumber;
            int blockVoxelY = voxelY & BlockLOD16.MagicModulusNumber;
            int blockVoxelZ = voxelZ & BlockLOD16.MagicModulusNumber;
            return blockIdToBlock[blockId].LOD16[blockVoxelX, blockVoxelY, blockVoxelZ];
        }

        /// <summary>
        /// Gets the voxel in the game world at the specified absolute voxel location
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        private Voxel GetVoxelHelper8(int voxelX, int voxelY, int voxelZ)
        {
            MiscUtils.CheckConstraints(voxelX, voxelY, voxelZ, LODUtils.AdjustLocationForDetailLevelFrom32(voxelSize32, LODLevel.V8));

            int blockX = voxelX >> BlockLOD8.VoxelSizeBitShift;
            int blockY = voxelY >> BlockLOD8.VoxelSizeBitShift;
            int blockZ = voxelZ >> BlockLOD8.VoxelSizeBitShift;
            ushort blockId = gameWorldBlocks[blockSize.GetArrayOffset(blockX, blockY, blockZ)];
            if (blockId == 0)
                return Voxel.Empty;

            int blockVoxelX = voxelX & BlockLOD8.MagicModulusNumber;
            int blockVoxelY = voxelY & BlockLOD8.MagicModulusNumber;
            int blockVoxelZ = voxelZ & BlockLOD8.MagicModulusNumber;
            return blockIdToBlock[blockId].LOD8[blockVoxelX, blockVoxelY, blockVoxelZ];
        }

        /// <summary>
        /// Gets the voxel in the game world at the specified absolute voxel location
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        private Voxel GetVoxelHelper4(int voxelX, int voxelY, int voxelZ)
        {
            MiscUtils.CheckConstraints(voxelX, voxelY, voxelZ, LODUtils.AdjustLocationForDetailLevelFrom32(voxelSize32, LODLevel.V4));

            int blockX = voxelX >> BlockLOD4.VoxelSizeBitShift;
            int blockY = voxelY >> BlockLOD4.VoxelSizeBitShift;
            int blockZ = voxelZ >> BlockLOD4.VoxelSizeBitShift;
            ushort blockId = gameWorldBlocks[blockSize.GetArrayOffset(blockX, blockY, blockZ)];
            if (blockId == 0)
                return Voxel.Empty;

            int blockVoxelX = voxelX & BlockLOD4.MagicModulusNumber;
            int blockVoxelY = voxelY & BlockLOD4.MagicModulusNumber;
            int blockVoxelZ = voxelZ & BlockLOD4.MagicModulusNumber;
            return blockIdToBlock[blockId].LOD4[blockVoxelX, blockVoxelY, blockVoxelZ];
        }
        #endregion

        #region NoVoxelInLine performance implementations
        /// <summary>
        /// Voxel transversal algorithm taken from: http://www.cse.chalmers.se/edu/year/2011/course/TDA361_Computer_Graphics/grid.pdf
        /// Modified with optimizations for TiVE.
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        private bool NoVoxelInLineHelper32(int x, int y, int z, int endX, int endY, int endZ)
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
            int blockX = x >> BlockLOD32.VoxelSizeBitShift;
            int blockY = y >> BlockLOD32.VoxelSizeBitShift;
            int blockZ = z >> BlockLOD32.VoxelSizeBitShift;
            int blockVoxelX = x & BlockLOD32.MagicModulusNumber;
            int blockVoxelY = y & BlockLOD32.MagicModulusNumber;
            int blockVoxelZ = z & BlockLOD32.MagicModulusNumber;

            Vector3i blockSizeLocal = blockSize;
            ushort[] blocksLocal = gameWorldBlocks;
            Block[] blockIdToBlockLocal = blockIdToBlock;
            for (; ; )
            {
                if (tMaxX < tMaxY)
                {
                    if (tMaxX < tMaxZ)
                    {
                        x = x + stepX;
                        tMaxX = tMaxX + tStepX;
                        blockVoxelX = x & BlockLOD32.MagicModulusNumber;
                        blockX = x >> BlockLOD32.VoxelSizeBitShift;
                    }
                    else
                    {
                        z = z + stepZ;
                        tMaxZ = tMaxZ + tStepZ;
                        blockVoxelZ = z & BlockLOD32.MagicModulusNumber;
                        blockZ = z >> BlockLOD32.VoxelSizeBitShift;
                    }
                }
                else if (tMaxY < tMaxZ)
                {
                    y = y + stepY;
                    tMaxY = tMaxY + tStepY;
                    blockVoxelY = y & BlockLOD32.MagicModulusNumber;
                    blockY = y >> BlockLOD32.VoxelSizeBitShift;
                }
                else
                {
                    z = z + stepZ;
                    tMaxZ = tMaxZ + tStepZ;
                    blockVoxelZ = z & BlockLOD32.MagicModulusNumber;
                    blockZ = z >> BlockLOD32.VoxelSizeBitShift;
                }

                if (x == endX && y == endY && z == endZ)
                    return true;

                ushort blockId = blocksLocal[blockSizeLocal.GetArrayOffset(blockX, blockY, blockZ)];
                if (blockId != 0)
                {
                    BlockLOD32 blockLOD = blockIdToBlockLocal[blockId].LOD32;
                    if (!blockLOD[blockVoxelX, blockVoxelY, blockVoxelZ].AllowLightPassthrough)
                        return false;
                }
            }
        }

        /// <summary>
        /// Voxel transversal algorithm taken from: http://www.cse.chalmers.se/edu/year/2011/course/TDA361_Computer_Graphics/grid.pdf
        /// Modified with optimizations for TiVE.
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        private bool NoVoxelInLineHelper16(int x, int y, int z, int endX, int endY, int endZ)
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
            int blockX = x >> BlockLOD16.VoxelSizeBitShift;
            int blockY = y >> BlockLOD16.VoxelSizeBitShift;
            int blockZ = z >> BlockLOD16.VoxelSizeBitShift;
            int blockVoxelX = x & BlockLOD16.MagicModulusNumber;
            int blockVoxelY = y & BlockLOD16.MagicModulusNumber;
            int blockVoxelZ = z & BlockLOD16.MagicModulusNumber;
            
            Vector3i blockSizeLocal = blockSize;
            ushort[] blocksLocal = gameWorldBlocks;
            Block[] blockIdToBlockLocal = blockIdToBlock;
            for (; ; )
            {
                if (tMaxX < tMaxY)
                {
                    if (tMaxX < tMaxZ)
                    {
                        x = x + stepX;
                        tMaxX = tMaxX + tStepX;
                        blockVoxelX = x & BlockLOD16.MagicModulusNumber;
                        blockX = x >> BlockLOD16.VoxelSizeBitShift;
                    }
                    else
                    {
                        z = z + stepZ;
                        tMaxZ = tMaxZ + tStepZ;
                        blockVoxelZ = z & BlockLOD16.MagicModulusNumber;
                        blockZ = z >> BlockLOD16.VoxelSizeBitShift;
                    }
                }
                else if (tMaxY < tMaxZ)
                {
                    y = y + stepY;
                    tMaxY = tMaxY + tStepY;
                    blockVoxelY = y & BlockLOD16.MagicModulusNumber;
                    blockY = y >> BlockLOD16.VoxelSizeBitShift;
                }
                else
                {
                    z = z + stepZ;
                    tMaxZ = tMaxZ + tStepZ;
                    blockVoxelZ = z & BlockLOD16.MagicModulusNumber;
                    blockZ = z >> BlockLOD16.VoxelSizeBitShift;
                }

                if (x == endX && y == endY && z == endZ)
                    return true;

                ushort blockId = blocksLocal[blockSizeLocal.GetArrayOffset(blockX, blockY, blockZ)];
                if (blockId != 0)
                {
                    BlockLOD16 blockLOD = blockIdToBlockLocal[blockId].LOD16;
                    if (!blockLOD[blockVoxelX, blockVoxelY, blockVoxelZ].AllowLightPassthrough)
                        return false;
                }
            }
        }

        /// <summary>
        /// Voxel transversal algorithm taken from: http://www.cse.chalmers.se/edu/year/2011/course/TDA361_Computer_Graphics/grid.pdf
        /// Modified with optimizations for TiVE.
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        private bool NoVoxelInLineHelper8(int x, int y, int z, int endX, int endY, int endZ)
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
            int blockX = x >> BlockLOD8.VoxelSizeBitShift;
            int blockY = y >> BlockLOD8.VoxelSizeBitShift;
            int blockZ = z >> BlockLOD8.VoxelSizeBitShift;
            int blockVoxelX = x & BlockLOD8.MagicModulusNumber;
            int blockVoxelY = y & BlockLOD8.MagicModulusNumber;
            int blockVoxelZ = z & BlockLOD8.MagicModulusNumber;
            
            Vector3i blockSizeLocal = blockSize;
            ushort[] blocksLocal = gameWorldBlocks;
            Block[] blockIdToBlockLocal = blockIdToBlock;
            for (; ; )
            {
                if (tMaxX < tMaxY)
                {
                    if (tMaxX < tMaxZ)
                    {
                        x = x + stepX;
                        tMaxX = tMaxX + tStepX;
                        blockVoxelX = x & BlockLOD8.MagicModulusNumber;
                        blockX = x >> BlockLOD8.VoxelSizeBitShift;
                    }
                    else
                    {
                        z = z + stepZ;
                        tMaxZ = tMaxZ + tStepZ;
                        blockVoxelZ = z & BlockLOD8.MagicModulusNumber;
                        blockZ = z >> BlockLOD8.VoxelSizeBitShift;
                    }
                }
                else if (tMaxY < tMaxZ)
                {
                    y = y + stepY;
                    tMaxY = tMaxY + tStepY;
                    blockVoxelY = y & BlockLOD8.MagicModulusNumber;
                    blockY = y >> BlockLOD8.VoxelSizeBitShift;
                }
                else
                {
                    z = z + stepZ;
                    tMaxZ = tMaxZ + tStepZ;
                    blockVoxelZ = z & BlockLOD8.MagicModulusNumber;
                    blockZ = z >> BlockLOD8.VoxelSizeBitShift;
                }

                if (x == endX && y == endY && z == endZ)
                    return true;

                ushort blockId = blocksLocal[blockSizeLocal.GetArrayOffset(blockX, blockY, blockZ)];
                if (blockId != 0)
                {
                    BlockLOD8 blockLOD = blockIdToBlockLocal[blockId].LOD8;
                    if (!blockLOD[blockVoxelX, blockVoxelY, blockVoxelZ].AllowLightPassthrough)
                        return false;
                }
            }
        }

        /// <summary>
        /// Voxel transversal algorithm taken from: http://www.cse.chalmers.se/edu/year/2011/course/TDA361_Computer_Graphics/grid.pdf
        /// Modified with optimizations for TiVE.
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        private bool NoVoxelInLineHelper4(int x, int y, int z, int endX, int endY, int endZ)
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
            int blockX = x >> BlockLOD4.VoxelSizeBitShift;
            int blockY = y >> BlockLOD4.VoxelSizeBitShift;
            int blockZ = z >> BlockLOD4.VoxelSizeBitShift;
            int blockVoxelX = x & BlockLOD4.MagicModulusNumber;
            int blockVoxelY = y & BlockLOD4.MagicModulusNumber;
            int blockVoxelZ = z & BlockLOD4.MagicModulusNumber;
            
            Vector3i blockSizeLocal = blockSize;
            ushort[] blocksLocal = gameWorldBlocks;
            Block[] blockIdToBlockLocal = blockIdToBlock;
            for (; ; )
            {
                if (tMaxX < tMaxY)
                {
                    if (tMaxX < tMaxZ)
                    {
                        x = x + stepX;
                        tMaxX = tMaxX + tStepX;
                        blockVoxelX = x & BlockLOD4.MagicModulusNumber;
                        blockX = x >> BlockLOD4.VoxelSizeBitShift;
                    }
                    else
                    {
                        z = z + stepZ;
                        tMaxZ = tMaxZ + tStepZ;
                        blockVoxelZ = z & BlockLOD4.MagicModulusNumber;
                        blockZ = z >> BlockLOD4.VoxelSizeBitShift;
                    }
                }
                else if (tMaxY < tMaxZ)
                {
                    y = y + stepY;
                    tMaxY = tMaxY + tStepY;
                    blockVoxelY = y & BlockLOD4.MagicModulusNumber;
                    blockY = y >> BlockLOD4.VoxelSizeBitShift;
                }
                else
                {
                    z = z + stepZ;
                    tMaxZ = tMaxZ + tStepZ;
                    blockVoxelZ = z & BlockLOD4.MagicModulusNumber;
                    blockZ = z >> BlockLOD4.VoxelSizeBitShift;
                }

                if (x == endX && y == endY && z == endZ)
                    return true;

                ushort blockId = blocksLocal[blockSizeLocal.GetArrayOffset(blockX, blockY, blockZ)];
                if (blockId != 0)
                {
                    BlockLOD4 blockLOD = blockIdToBlockLocal[blockId].LOD4;
                    if (!blockLOD[blockVoxelX, blockVoxelY, blockVoxelZ].AllowLightPassthrough)
                        return false;
                }
            }
        }
        #endregion
    }
}
