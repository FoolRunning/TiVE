using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ProdigalSoftware.TiVE.Renderer.Voxels;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    /// <summary>
    /// Contains the information about the game world.
    /// </summary>
    internal sealed class GameWorld : IGameWorld
    {
        private readonly Vector3i voxelSize;
        private readonly Vector3i blockSize;
        private readonly Vector3i chunkSize;

        private readonly Block[] worldBlocks;
        private readonly GameWorldVoxelChunk[] worldChunks;

        internal GameWorld(int blockSizeX, int blockSizeY, int blockSizeZ, bool useInstancing)
        {
            blockSize = new Vector3i(blockSizeX, blockSizeY, blockSizeZ);
            voxelSize = new Vector3i(blockSizeX * BlockInformation.BlockSize, blockSizeY * BlockInformation.BlockSize, blockSizeZ * BlockInformation.BlockSize);
            chunkSize = new Vector3i((int)Math.Ceiling(blockSizeX / (float)GameWorldVoxelChunk.TileSize),
                (int)Math.Ceiling(blockSizeY / (float)GameWorldVoxelChunk.TileSize),
                (int)Math.Ceiling(blockSizeZ / (float)GameWorldVoxelChunk.TileSize));

            worldBlocks = new Block[blockSizeX * blockSizeY * blockSizeZ];
            for (int i = 0; i < worldBlocks.Length; i++)
                worldBlocks[i] = new Block();

            worldChunks = new GameWorldVoxelChunk[chunkSize.X * chunkSize.Y * chunkSize.Z];
            for (int z = 0; z < chunkSize.Z; z++)
            {
                for (int x = 0; x < chunkSize.X; x++)
                {
                    for (int y = 0; y < chunkSize.Y; y++)
                        worldChunks[GetChunkOffset(x, y, z)] = useInstancing ? new InstancedGameWorldVoxelChunk(x, y, z) : new GameWorldVoxelChunk(x, y, z);
                }
            }
        }

        /// <summary>
        /// Gets the absolute voxel size of the game world
        /// </summary>
        public Vector3i VoxelSize
        {
            get { return voxelSize; }
        }

        /// <summary>
        /// Gets the size of the game world in chunks
        /// </summary>
        public Vector3i ChunkSize
        {
            get { return chunkSize; }
        }

        #region Implementation of IGameWorld
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
        #endregion

        public GameWorldVoxelChunk GetChunk(int chunkX, int chunkY, int chunkZ)
        {
            return worldChunks[GetChunkOffset(chunkX, chunkY, chunkZ)];
        }

        public List<LightInfo> GetLights(int blockX, int blockY, int blockZ)
        {
            return worldBlocks[GetBlockOffset(blockX, blockY, blockZ)].Lights;
        }

        /// <summary>
        /// Gets the voxel in the game world at the specified absolute voxel location
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetVoxel(int voxelX, int voxelY, int voxelZ)
        {
            BlockInformation block = GetBlockAtWorld(voxelX, voxelY, voxelZ);

            int blockVoxelX = voxelX % BlockInformation.BlockSize;
            int blockVoxelY = voxelY % BlockInformation.BlockSize;
            int blockVoxelZ = voxelZ % BlockInformation.BlockSize;
            return block[blockVoxelX, blockVoxelY, blockVoxelZ];
        }

        /// <summary>
        /// Gets the block containing the specified absolute voxel location
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockInformation GetBlockAtWorld(int voxelX, int voxelY, int voxelZ)
        {
            CheckConstraints(voxelX, voxelY, voxelZ, voxelSize);

            int blockX = voxelX / BlockInformation.BlockSize;
            int blockY = voxelY / BlockInformation.BlockSize;
            int blockZ = voxelZ / BlockInformation.BlockSize;

            return worldBlocks[GetBlockOffset(blockX, blockY, blockZ)].BlockInfo;
        }

        /// <summary>
        /// Gets the offset into the game world blocks array for the block at the specified location
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlockOffset(int x, int y, int z)
        {
            CheckConstraints(x, y, z, blockSize);
            return (x * blockSize.Z + z) * blockSize.Y + y; // y-axis major for speed
        }

        /// <summary>
        /// Gets the offset into the game world chunks array for the chunk at the specified location
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetChunkOffset(int x, int y, int z)
        {
            CheckConstraints(x, y, z, chunkSize);
            return (x * chunkSize.Z + z) * chunkSize.Y + y; // y-axis major for speed
        }

        [Conditional("DEBUG")]
        private void CheckConstraints(int x, int y, int z, Vector3i size)
        {
            if (x < 0 || x >= size.X)
                throw new ArgumentOutOfRangeException("x");
            if (y < 0 || y >= size.Y)
                throw new ArgumentOutOfRangeException("y");
            if (z < 0 || z >= size.Z)
                throw new ArgumentOutOfRangeException("z");
        }

        #region Block class
        /// <summary>
        /// Represents one block in the game world
        /// </summary>
        private sealed class Block
        {
            /// <summary>
            /// Information about the block (can not be null)
            /// </summary>
            public BlockInformation BlockInfo = BlockInformation.Empty;
            
            /// <summary>
            /// List of lights that affect this block or null for none
            /// </summary>
            public List<LightInfo> Lights = new List<LightInfo>();
        }
        #endregion
    }
}
