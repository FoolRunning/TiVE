﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.ProjectM.Data.Plugins
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
        public const int stoneBlockDuplicates = 1;
        public const int stoneBackBlockDuplicates = 10;
        public const int grassBlockDuplicates = 200;
        public const int leavesBlockDuplicates = 5;

        private const int Front = 1;
        private const int Back = 2;
        private const int Left = 4;
        private const int Right = 8;
        private const int Top = 16;
        private const int Bottom = 32;

        private static readonly Regex blockNameRegex = new Regex(@"(?<part>[^\d]+)(?<num>\d+)?(?:_(?<other>\d+))?", RegexOptions.Compiled);

        public static bool ParseBlockName(string blockName, out string part, out int num, out string other)
        {
            Match match = blockNameRegex.Match(blockName);
            if (!match.Success)
            {
                part = null;
                num = 0;
                other = null;
                return false;
            }

            part = match.Groups["part"].Value;
            num = match.Groups["num"].Success ? int.Parse(match.Groups["num"].Value) : 0;
            other = match.Groups["other"].Success ? match.Groups["other"].Value : null;
            return true;
        }

        public static void SmoothGameWorldForMazeBlocks(IGameWorld gameWorld, bool forLoadingWorld)
        {
            List<BlockRandomizer> stoneRandomizers = new List<BlockRandomizer>();
            List<BlockRandomizer> backRandomizers = new List<BlockRandomizer>();
            List<BlockRandomizer> leavesRandomizers = new List<BlockRandomizer>();
            for (int i = 0; i < 64; i++)
            {
                stoneRandomizers.Add(new BlockRandomizer("stoneBrick" + i + "_", stoneBlockDuplicates));
                if (!forLoadingWorld)
                {
                    backRandomizers.Add(new BlockRandomizer("back" + i + "_", stoneBackBlockDuplicates));
                    leavesRandomizers.Add(new BlockRandomizer("leaves" + i + "_", leavesBlockDuplicates));
                }
            }

            HashSet<Block> blocksToConsiderEmpty = new HashSet<Block>();
            blocksToConsiderEmpty.Add(Block.Empty);
            if (!forLoadingWorld)
            {
                blocksToConsiderEmpty.Add(Factory.Get<Block>("fire"));
                blocksToConsiderEmpty.Add(Factory.Get<Block>("redLight"));
                blocksToConsiderEmpty.Add(Factory.Get<Block>("treeLight"));
            }
            for (int i = 0; i < grassBlockDuplicates; i++)
            {
                if (!forLoadingWorld)
                    blocksToConsiderEmpty.Add(Factory.Get<Block>("grass" + i));
                blocksToConsiderEmpty.Add(Factory.Get<Block>("loadingGrass" + i));
            }
            if (!forLoadingWorld)
            {
                for (int i = 0; i < 6; i++)
                    blocksToConsiderEmpty.Add(Factory.Get<Block>("light" + i));
            }

            HashSet<Block> blocksToSmooth = new HashSet<Block>();
            for (int i = 0; i < 64; i++)
            {
                for (int q = 0; q < stoneBlockDuplicates; q++)
                    blocksToSmooth.Add(Factory.Get<Block>("stoneBrick" + i + "_" + q));
                if (!forLoadingWorld)
                {
                    for (int q = 0; q < stoneBackBlockDuplicates; q++)
                        blocksToSmooth.Add(Factory.Get<Block>("back" + i + "_" + q));
                    for (int q = 0; q < leavesBlockDuplicates; q++)
                        blocksToSmooth.Add(Factory.Get<Block>("leaves" + i + "_" + q));
                    blocksToSmooth.Add(Factory.Get<Block>("wood" + i));
                }
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

                        string blockNamePart;
                        int num;
                        string other;
                        ParseBlockName(block.Name, out blockNamePart, out num, out other);

                        if (blockNamePart == "stoneBrick")
                            gameWorld[x, y, z] = stoneRandomizers[sides].NextBlock();
                        else if (blockNamePart == "leaves")
                            gameWorld[x, y, z] = leavesRandomizers[sides].NextBlock();
                        else if (blockNamePart == "back")
                            gameWorld[x, y, z] = backRandomizers[sides].NextBlock();
                        else
                            gameWorld[x, y, z] = Factory.Get<Block>(blockNamePart + sides);
                    }
                }
            }
        }
    }
}
