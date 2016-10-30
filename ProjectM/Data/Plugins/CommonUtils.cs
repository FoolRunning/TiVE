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

        public static void MakeBlockRound(Block block, CubeSides sides)
        {
            const float mid = BlockLOD32.VoxelSize / 2.0f - 0.5f;
            const float sphereSize = BlockLOD32.VoxelSize / 2.0f;

            for (int x = 0; x < BlockLOD32.VoxelSize; x++)
            {
                for (int y = 0; y < BlockLOD32.VoxelSize; y++)
                {
                    for (int z = 0; z < BlockLOD32.VoxelSize; z++)
                    {
                        if (((sides & CubeSides.Top) != 0 && (sides & CubeSides.Front) != 0 && y - (int)mid > BlockLOD32.VoxelSize - z) ||   // rounded Top-Front
                            ((sides & CubeSides.Front) != 0 && (sides & CubeSides.Bottom) != 0 && y + (int)mid < z) ||                  // rounded Front-Bottom
                            ((sides & CubeSides.Bottom) != 0 && (sides & CubeSides.Back) != 0 && y + (int)mid < BlockLOD32.VoxelSize - z) || // rounded Bottom-Back
                            ((sides & CubeSides.Back) != 0 && (sides & CubeSides.Top) != 0 && y - (int)mid > z))                        // rounded Back-Top
                        {
                            // Cylinder around the x-axis
                            float dist = (y - mid) * (y - mid) + (z - mid) * (z - mid);
                            if (dist > sphereSize * sphereSize)
                                block.LOD32[x, y, z] = Voxel.Empty;
                        }

                        if (((sides & CubeSides.Right) != 0 && (sides & CubeSides.Front) != 0 && x - (int)mid > BlockLOD32.VoxelSize - z) || // rounded Right-Front
                            ((sides & CubeSides.Front) != 0 && (sides & CubeSides.Left) != 0 && x + (int)mid < z) ||                    // rounded Front-Left
                            ((sides & CubeSides.Left) != 0 && (sides & CubeSides.Back) != 0 && x + (int)mid < BlockLOD32.VoxelSize - z) ||   // rounded Left-Back
                            ((sides & CubeSides.Back) != 0 && (sides & CubeSides.Right) != 0 && x - (int)mid > z))                      // rounded Back-Right
                        {
                            // Cylinder around the y-axis
                            float dist = (x - mid) * (x - mid) + (z - mid) * (z - mid);
                            if (dist > sphereSize * sphereSize)
                                block.LOD32[x, y, z] = Voxel.Empty;
                        }

                        if (((sides & CubeSides.Right) != 0 && (sides & CubeSides.Top) != 0 && x - (int)mid > BlockLOD32.VoxelSize - y) ||   // rounded Right-Top
                            ((sides & CubeSides.Top) != 0 && (sides & CubeSides.Left) != 0 && x + (int)mid < y) ||                      // rounded Top-Left
                            ((sides & CubeSides.Left) != 0 && (sides & CubeSides.Bottom) != 0 && x + (int)mid < BlockLOD32.VoxelSize - y) || // rounded Left-Bottom
                            ((sides & CubeSides.Bottom) != 0 && (sides & CubeSides.Right) != 0 && x - (int)mid > y))                    // rounded Bottom-Right
                        {
                            // Cylinder around the z-axis
                            float dist = (x - mid) * (x - mid) + (y - mid) * (y - mid);
                            if (dist > sphereSize * sphereSize)
                                block.LOD32[x, y, z] = Voxel.Empty;
                        }

                        if ((((sides & CubeSides.Top) != 0 && (sides & CubeSides.Bottom) != 0 && (sides & CubeSides.Left) != 0 && x < mid) || // rounded Left
                            ((sides & CubeSides.Top) != 0 && (sides & CubeSides.Bottom) != 0 && (sides & CubeSides.Right) != 0 && x > mid) || // rounded Right
                            ((sides & CubeSides.Top) != 0 && (sides & CubeSides.Right) != 0 && (sides & CubeSides.Left) != 0 && y > mid) ||   // rounded Top
                            ((sides & CubeSides.Bottom) != 0 && (sides & CubeSides.Right) != 0 && (sides & CubeSides.Left) != 0 && y < mid))  // rounded Bottom
                            && (((sides & CubeSides.Front) != 0 && z > mid) || ((sides & CubeSides.Back) != 0 && z < mid)))         // on the front or back
                        {
                            // rounded front or back
                            float dist = (x - mid) * (x - mid) + (y - mid) * (y - mid) + (z - mid) * (z - mid);
                            if (dist > sphereSize * sphereSize)
                                block.LOD32[x, y, z] = Voxel.Empty;
                        }
                    }
                }
            }
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
                for (int i = 0; i < 7; i++)
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

                        CubeSides sides = CubeSides.None;
                        if (z == gameWorld.BlockSize.Z - 1 || blocksToConsiderEmpty.Contains(gameWorld[x, y, z + 1]))
                            sides |= CubeSides.Front;
                        if (z == 0 || blocksToConsiderEmpty.Contains(gameWorld[x, y, z - 1]))
                            sides |= CubeSides.Back;
                        if (x == 0 || blocksToConsiderEmpty.Contains(gameWorld[x - 1, y, z]))
                            sides |= CubeSides.Left;
                        if (x == gameWorld.BlockSize.X - 1 || blocksToConsiderEmpty.Contains(gameWorld[x + 1, y, z]))
                            sides |= CubeSides.Right;
                        if (y == gameWorld.BlockSize.Y - 1 || blocksToConsiderEmpty.Contains(gameWorld[x, y + 1, z]))
                            sides |= CubeSides.Top;
                        if (y == 0 || blocksToConsiderEmpty.Contains(gameWorld[x, y - 1, z]))
                            sides |= CubeSides.Bottom;

                        string blockNamePart;
                        int num;
                        string other;
                        ParseBlockName(block.Name, out blockNamePart, out num, out other);

                        if (blockNamePart == "stoneBrick")
                            gameWorld[x, y, z] = stoneRandomizers[(int)sides].NextBlock();
                        else if (blockNamePart == "leaves")
                            gameWorld[x, y, z] = leavesRandomizers[(int)sides].NextBlock();
                        else if (blockNamePart == "back")
                            gameWorld[x, y, z] = backRandomizers[(int)sides].NextBlock();
                        else
                            gameWorld[x, y, z] = Factory.Get<Block>(blockNamePart + (int)sides);
                    }
                }
            }
        }
    }
}
