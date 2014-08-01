using System;
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
        private readonly BlockInformation[] worldBlocks;
        private readonly int xWorldSize;
        private readonly int yWorldSize;
        private readonly int zWorldSize;

        private readonly GameWorldVoxelChunk[] worldChunks;
        private readonly int xChunkSize;
        private readonly int yChunkSize;
        private readonly int zChunkSize;

        internal GameWorld(int xWorldSize, int yWorldSize, int zWorldSize)
        {
            this.xWorldSize = xWorldSize;
            this.yWorldSize = yWorldSize;
            this.zWorldSize = zWorldSize;

            worldBlocks = new BlockInformation[xWorldSize * yWorldSize * zWorldSize];
            for (int i = 0; i < worldBlocks.Length; i++)
                worldBlocks[i] = BlockInformation.Empty;

            xChunkSize = (int)Math.Ceiling(xWorldSize / (float)GameWorldVoxelChunk.TileSize);
            yChunkSize = (int)Math.Ceiling(yWorldSize / (float)GameWorldVoxelChunk.TileSize);
            zChunkSize = (int)Math.Ceiling(zWorldSize / (float)GameWorldVoxelChunk.TileSize);
            worldChunks = new GameWorldVoxelChunk[xChunkSize * yChunkSize * zChunkSize];
            for (int z = 0; z < zChunkSize; z++)
            {
                for (int x = 0; x < xChunkSize; x++)
                {
                    for (int y = 0; y < yChunkSize; y++)
                        worldChunks[GetChunkOffset(x, y, z)] = new GameWorldVoxelChunk(x, y, z, false);
                }
            }
        }

        public int Xsize
        {
            get { return xWorldSize; }
        }

        public int Ysize
        {
            get { return yWorldSize; }
        }

        public int Zsize
        {
            get { return zWorldSize; }
        }

        public BlockInformation GetBlock(int x, int y, int z)
        {
            return worldBlocks[GetBlockOffset(x, y, z)];
        }

        public void SetBlock(int x, int y, int z, BlockInformation block)
        {
            worldBlocks[GetBlockOffset(x, y, z)] = block ?? BlockInformation.Empty;
        }

        public int XChunkSize
        {
            get { return xChunkSize; }
        }

        public int YChunkSize
        {
            get { return yChunkSize; }
        }

        public int ZChunkSize
        {
            get { return zChunkSize; }
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
#if DEBUG
            if (x < 0 || x >= xWorldSize || y < 0 || y >= yWorldSize || z < 0 || z >= zWorldSize)
                throw new ArgumentException(string.Format("World location ({0}, {1}, {2}) out of range.", x, y, z));
#endif
            return (x * zWorldSize + z) * yWorldSize + y; // y-axis major for speed
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetChunkOffset(int x, int y, int z)
        {
#if DEBUG
            if (x < 0 || x >= xChunkSize || y < 0 || y >= yChunkSize || z < 0 || z >= zChunkSize)
                throw new ArgumentException(string.Format("Chunk location ({0}, {1}, {2}) out of range.", x, y, z));
#endif
            return (x * zChunkSize + z) * yChunkSize + y; // y-axis major for speed
        }
    }
}
