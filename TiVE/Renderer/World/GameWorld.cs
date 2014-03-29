using System;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    /// <summary>
    /// Contains the information about the game world. 
    /// </summary>
    internal sealed class GameWorld : IGameWorld
    {
        private readonly ushort[][][] worldBlocks;
        private readonly int xSize;
        private readonly int ySize;
        private readonly int zSize;
        private readonly BlockList blockList;

        internal GameWorld(int xSize, int ySize, int zSize, BlockList blockList)
        {
            this.xSize = xSize;
            this.ySize = ySize;
            this.zSize = zSize;

            this.blockList = blockList;
            worldBlocks = new ushort[xSize][][];
            for (int x = 0; x < xSize; x++)
            {
                worldBlocks[x] = new ushort[ySize][];
                for (int y = 0; y < ySize; y++)
                    worldBlocks[x][y] = new ushort[zSize];
            }
        }

        public int Xsize
        {
            get { return xSize; }
        }

        public int Ysize
        {
            get { return ySize; }
        }

        public int Zsize
        {
            get { return zSize; }
        }

        public void SetBlock(int x, int y, int z, ushort blockIndex)
        {
            if (blockIndex >= blockList.BlockCount)
                throw new ArgumentOutOfRangeException("blockIndex", "Block with the specified index does not exist");

            ushort[] depthBlocks = worldBlocks[x][y];
            lock (depthBlocks)
                depthBlocks[z] = blockIndex;
        }

        public ushort GetBlock(int x, int y, int z)
        {
            ushort[] depthBlocks = worldBlocks[x][y];
            lock (depthBlocks)
                return depthBlocks[z];
        }
    }
}
