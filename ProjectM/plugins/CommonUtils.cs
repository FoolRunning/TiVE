using System.Collections.Generic;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.ProjectM.Plugins
{
    public sealed class BlockRandomizer
    {
        private readonly Block[] blocks;
        private readonly RandomGenerator random = new RandomGenerator();

        public BlockRandomizer(string blockname, int blockCount)
        {
            blocks = new Block[blockCount];
            for (int i = 0; i < blocks.Length; i++)
                blocks[i] = Factory.Get<Block>(blockname + i);
        }

        public Block NextBlock()
        {
            return blocks[random.Next(blocks.Length)];
        }
    }

    public static class CommonUtils
    {
        public const int stoneBlockDuplicates = 5;
        public const int stoneBackBlockDuplicates = 10;
        public const int grassBlockDuplicates = 50;

        private const int Front = 1;
        private const int Back = 2;
        private const int Left = 4;
        private const int Right = 8;
        private const int Top = 16;
        private const int Bottom = 32;

        public static void SmoothGameWorldForMazeBlocks(IGameWorld gameWorld)
        {
            HashSet<Block> blocksToConsiderEmpty = new HashSet<Block>();
            List<BlockRandomizer> stoneRandomizers = new List<BlockRandomizer>();
            for (int i = 0; i < 64; i++)
                stoneRandomizers.Add(new BlockRandomizer("ston" + i + "_", stoneBlockDuplicates));

            blocksToConsiderEmpty.Add(Block.Empty);
            blocksToConsiderEmpty.Add(Factory.Get<Block>("smallLightHover"));
            for (int i = 0; i < grassBlockDuplicates; i++)
                blocksToConsiderEmpty.Add(Factory.Get<Block>("grass" + i));
            for (int i = 0; i < 6; i++)
                blocksToConsiderEmpty.Add(Factory.Get<Block>("light" + i));

            HashSet<Block> blocksToSmooth = new HashSet<Block>();
            for (int i = 0; i < 64; i++)
            {
                for (int q = 0; q < stoneBlockDuplicates; q++)
                    blocksToSmooth.Add(Factory.Get<Block>("ston" + i + "_" + q));
                blocksToSmooth.Add(Factory.Get<Block>("wood" + i));
            }

            for (int z = 0; z < gameWorld.BlockSize.Z; z++)
            {
                for (int x = 0; x < gameWorld.BlockSize.X; x++)
                {
                    for (int y = 0; y < gameWorld.BlockSize.Y; y++)
                    {
                        Block block = gameWorld[x, y, z];
                        if (block == Block.Empty || !blocksToSmooth.Contains(block))
                            continue;

                        int sides = 0;

                        if (z == gameWorld.BlockSize.Z - 1 || blocksToConsiderEmpty.Contains(gameWorld[x, y, z + 1]))
                            sides |= Front;

                        if (z == 0 || blocksToConsiderEmpty.Contains(gameWorld[x, y, z - 1]))
                            sides |= Back;

                        if (x == 0 || blocksToConsiderEmpty.Contains(gameWorld[x - 1, y, z]))
                            sides |= Left;

                        if (x == gameWorld.BlockSize.X - 1 || blocksToConsiderEmpty.Contains(gameWorld[x + 1, y, z]))
                            sides |= Right;

                        if (y == gameWorld.BlockSize.Y - 1 || blocksToConsiderEmpty.Contains(gameWorld[x, y + 1, z]))
                            sides |= Top;

                        if (y == 0 || blocksToConsiderEmpty.Contains(gameWorld[x, y - 1, z]))
                            sides |= Bottom;

                        string blockNamePart = block.Name.Substring(0, 4);
                        if (blockNamePart == "ston")
                            gameWorld[x, y, z] = stoneRandomizers[sides].NextBlock();
                        else
                            gameWorld[x, y, z] = Factory.Get<Block>(blockNamePart + sides);
                    }
                }
            }
        }
    }
}
