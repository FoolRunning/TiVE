using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ProdigalSoftware.TiVE.Renderer.Voxels;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    /// <summary>
    /// Contains the information about the game world. 
    /// </summary>
    internal sealed class GameWorld : IGameWorld
    {
        private readonly int worldSizeX;
        private readonly int worldSizeY;
        private readonly int worldSizeZ;

        private readonly BlockInformation[] worldBlocks;
        private readonly int blockSizeX;
        private readonly int blockSizeY;
        private readonly int blockSizeZ;

        private readonly GameWorldVoxelChunk[] worldChunks;
        private readonly int chunkSizeX;
        private readonly int chunkSizeY;
        private readonly int chunkSizeZ;

        internal GameWorld(int blockSizeX, int blockSizeY, int blockSizeZ, bool useInstancing)
        {
            this.blockSizeX = blockSizeX;
            this.blockSizeY = blockSizeY;
            this.blockSizeZ = blockSizeZ;

            worldSizeX = blockSizeX * BlockInformation.BlockSize;
            worldSizeY = blockSizeX * BlockInformation.BlockSize;
            worldSizeZ = blockSizeX * BlockInformation.BlockSize;

            worldBlocks = new BlockInformation[blockSizeX * blockSizeY * blockSizeZ];
            for (int i = 0; i < worldBlocks.Length; i++)
                worldBlocks[i] = BlockInformation.Empty;

            chunkSizeX = (int)Math.Ceiling(blockSizeX / (float)GameWorldVoxelChunk.TileSize);
            chunkSizeY = (int)Math.Ceiling(blockSizeY / (float)GameWorldVoxelChunk.TileSize);
            chunkSizeZ = (int)Math.Ceiling(blockSizeZ / (float)GameWorldVoxelChunk.TileSize);
            worldChunks = new GameWorldVoxelChunk[chunkSizeX * chunkSizeY * chunkSizeZ];
            for (int z = 0; z < chunkSizeZ; z++)
            {
                for (int x = 0; x < chunkSizeX; x++)
                {
                    for (int y = 0; y < chunkSizeY; y++)
                        worldChunks[GetChunkOffset(x, y, z)] = useInstancing ? new InstancedGameWorldVoxelChunk(x, y, z) : new GameWorldVoxelChunk(x, y, z);
                }
            }
        }

        public int WorldSizeX
        {
            get { return worldSizeX; }
        }

        public int WorldSizeY
        {
            get { return worldSizeY; }
        }

        public int WorldSizeZ
        {
            get { return worldSizeZ; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetVoxel(int worldX, int worldY, int worldZ)
        {
            Debug.Assert(worldX >= 0 && worldX < worldSizeX);
            Debug.Assert(worldY >= 0 && worldY < worldSizeY);
            Debug.Assert(worldZ >= 0 && worldZ < worldSizeZ);

            int blockX = worldX / BlockInformation.BlockSize;
            int blockY = worldY / BlockInformation.BlockSize;
            int blockZ = worldZ / BlockInformation.BlockSize;

            int voxelX = worldX % BlockInformation.BlockSize;
            int voxelY = worldY % BlockInformation.BlockSize;
            int voxelZ = worldZ % BlockInformation.BlockSize;

            BlockInformation block = worldBlocks[GetBlockOffset(blockX, blockY, blockZ)];
            return block[voxelX, voxelY, voxelZ];
        }

        public int BlockSizeX
        {
            get { return blockSizeX; }
        }

        public int BlockSizeY
        {
            get { return blockSizeY; }
        }

        public int BlockSizeZ
        {
            get { return blockSizeZ; }
        }

        public BlockInformation this[int x, int y, int z]
        {
            get { return worldBlocks[GetBlockOffset(x, y, z)]; }
            set { worldBlocks[GetBlockOffset(x, y, z)] = value ?? BlockInformation.Empty; }
        }

        public int ChunkSizeX
        {
            get { return chunkSizeX; }
        }

        public int ChunkSizeY
        {
            get { return chunkSizeY; }
        }

        public int ChunkSizeZ
        {
            get { return chunkSizeZ; }
        }

        public GameWorldVoxelChunk GetChunk(int chunkX, int chunkY, int chunkZ)
        {
            return worldChunks[GetChunkOffset(chunkX, chunkY, chunkZ)];
        }

        public void SetChunk(int chunkX, int chunkY, int chunkZ, GameWorldVoxelChunk chunk)
        {
            worldChunks[GetChunkOffset(chunkX, chunkY, chunkZ)] = chunk;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlockOffset(int x, int y, int z)
        {
            Debug.Assert(x >= 0 && x < blockSizeX);
            Debug.Assert(y >= 0 && y < blockSizeY);
            Debug.Assert(z >= 0 && z < blockSizeZ);

            return (x * blockSizeZ + z) * blockSizeY + y; // y-axis major for speed
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetChunkOffset(int x, int y, int z)
        {
            Debug.Assert(x >= 0 && x < chunkSizeX);
            Debug.Assert(y >= 0 && y < chunkSizeY);
            Debug.Assert(z >= 0 && z < chunkSizeZ);

            return (x * chunkSizeZ + z) * chunkSizeY + y; // y-axis major for speed
        }
    }
}
