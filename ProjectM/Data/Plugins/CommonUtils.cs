using System.Collections.Generic;
using System.Text.RegularExpressions;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.ProjectM.Data.Plugins
{
    public sealed class BlockRandomizer
    {
        private readonly RandomGenerator random = new RandomGenerator();
        private readonly string[] blockNames;
        private readonly Block[] blocks;

        public BlockRandomizer(string blockname, int blockCount)
        {
            blockNames = new string[blockCount];
            blocks = new Block[blockCount];
            for (int i = 0; i < blockNames.Length; i++)
                blockNames[i] = blockname + i;
        }

        public Block NextBlock()
        {
            int blockIndex = random.Next(blockNames.Length);
            Block block = blocks[blockIndex];
            if (block == null)
                blocks[blockIndex] = block = Factory.Get<Block>(blockNames[blockIndex]);
            return block;
        }
    }

    public static class CommonUtils
    {
        public const int stoneBlockDuplicates = 5;
        public const int stoneBackBlockDuplicates = 50;
        public const int grassBlockDuplicates = 300;
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
                        if (((sides & CubeSides.YPlus) != 0 && (sides & CubeSides.ZPlus) != 0 && y - (int)mid > BlockLOD32.VoxelSize - z) ||   // rounded YPlus-ZPlus
                            ((sides & CubeSides.ZPlus) != 0 && (sides & CubeSides.YMinus) != 0 && y + (int)mid < z) ||                  // rounded ZPlus-YMinus
                            ((sides & CubeSides.YMinus) != 0 && (sides & CubeSides.ZMinus) != 0 && y + (int)mid < BlockLOD32.VoxelSize - z) || // rounded YMinus-Back
                            ((sides & CubeSides.ZMinus) != 0 && (sides & CubeSides.YPlus) != 0 && y - (int)mid > z))                        // rounded Back-YPlus
                        {
                            // Cylinder around the x-axis
                            float dist = (y - mid) * (y - mid) + (z - mid) * (z - mid);
                            if (dist > sphereSize * sphereSize)
                                block.LOD32[x, y, z] = Voxel.Empty;
                        }

                        if (((sides & CubeSides.XPlus) != 0 && (sides & CubeSides.ZPlus) != 0 && x - (int)mid > BlockLOD32.VoxelSize - z) || // rounded XPlus-ZPlus
                            ((sides & CubeSides.ZPlus) != 0 && (sides & CubeSides.XMinus) != 0 && x + (int)mid < z) ||                    // rounded ZPlus-XMinus
                            ((sides & CubeSides.XMinus) != 0 && (sides & CubeSides.ZMinus) != 0 && x + (int)mid < BlockLOD32.VoxelSize - z) ||   // rounded XMinus-Back
                            ((sides & CubeSides.ZMinus) != 0 && (sides & CubeSides.XPlus) != 0 && x - (int)mid > z))                      // rounded Back-XPlus
                        {
                            // Cylinder around the y-axis
                            float dist = (x - mid) * (x - mid) + (z - mid) * (z - mid);
                            if (dist > sphereSize * sphereSize)
                                block.LOD32[x, y, z] = Voxel.Empty;
                        }

                        if (((sides & CubeSides.XPlus) != 0 && (sides & CubeSides.YPlus) != 0 && x - (int)mid > BlockLOD32.VoxelSize - y) ||   // rounded XPlus-YPlus
                            ((sides & CubeSides.YPlus) != 0 && (sides & CubeSides.XMinus) != 0 && x + (int)mid < y) ||                      // rounded YPlus-XMinus
                            ((sides & CubeSides.XMinus) != 0 && (sides & CubeSides.YMinus) != 0 && x + (int)mid < BlockLOD32.VoxelSize - y) || // rounded XMinus-YMinus
                            ((sides & CubeSides.YMinus) != 0 && (sides & CubeSides.XPlus) != 0 && x - (int)mid > y))                    // rounded YMinus-XPlus
                        {
                            // Cylinder around the z-axis
                            float dist = (x - mid) * (x - mid) + (y - mid) * (y - mid);
                            if (dist > sphereSize * sphereSize)
                                block.LOD32[x, y, z] = Voxel.Empty;
                        }

                        if ((((sides & CubeSides.YPlus) != 0 && (sides & CubeSides.YMinus) != 0 && (sides & CubeSides.XMinus) != 0 && x < mid) || // rounded XMinus
                            ((sides & CubeSides.YPlus) != 0 && (sides & CubeSides.YMinus) != 0 && (sides & CubeSides.XPlus) != 0 && x > mid) || // rounded XPlus
                            ((sides & CubeSides.YPlus) != 0 && (sides & CubeSides.XPlus) != 0 && (sides & CubeSides.XMinus) != 0 && y > mid) ||   // rounded YPlus
                            ((sides & CubeSides.YMinus) != 0 && (sides & CubeSides.XPlus) != 0 && (sides & CubeSides.XMinus) != 0 && y < mid))  // rounded YMinus
                            && (((sides & CubeSides.ZPlus) != 0 && z > mid) || ((sides & CubeSides.ZMinus) != 0 && z < mid)))         // on the front or back
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
            List<BlockRandomizer> bumpyDirtRandomizers = new List<BlockRandomizer>();
            List<BlockRandomizer> backRandomizers = new List<BlockRandomizer>();
            List<BlockRandomizer> leavesRandomizers = new List<BlockRandomizer>();
            for (int i = 0; i < 64; i++)
            {
                backRandomizers.Add(new BlockRandomizer("back" + i + "_", stoneBackBlockDuplicates));
                if (!forLoadingWorld)
                {
                    stoneRandomizers.Add(new BlockRandomizer("stoneBrick" + i + "_", stoneBlockDuplicates));
                    bumpyDirtRandomizers.Add(new BlockRandomizer("bumpyDirt" + i + "_", stoneBackBlockDuplicates));
                    leavesRandomizers.Add(new BlockRandomizer("leaves" + i + "_", leavesBlockDuplicates));
                }
            }
            HashSet<string> grassBlocks = new HashSet<string>();
            for (int i = 0; i < grassBlockDuplicates; i++)
            {
                grassBlocks.Add("grass" + i);
                grassBlocks.Add("loadingGrass" + i);
            }

            HashSet<string> blocksToConsiderEmpty = new HashSet<string>();
            blocksToConsiderEmpty.Add(Block.Empty.Name);
            blocksToConsiderEmpty.Add("fire");
            blocksToConsiderEmpty.Add("redLight");
            blocksToConsiderEmpty.Add("treeLight");
            blocksToConsiderEmpty.Add("fountain");
            for (int i = 0; i < grassBlockDuplicates; i++)
            {
                blocksToConsiderEmpty.Add("grass" + i);
                blocksToConsiderEmpty.Add("loadingGrass" + i);
            }
            for (int i = 0; i < 7; i++)
                blocksToConsiderEmpty.Add("light" + i);

            HashSet<string> blocksToSmooth = new HashSet<string>();
            for (int i = 0; i < 64; i++)
            {
                for (int q = 0; q < stoneBackBlockDuplicates; q++)
                    blocksToSmooth.Add("back" + i + "_" + q);
                for (int q = 0; q < stoneBlockDuplicates; q++)
                    blocksToSmooth.Add("stoneBrick" + i + "_" + q);
                for (int q = 0; q < stoneBackBlockDuplicates; q++)
                    blocksToSmooth.Add("bumpyDirt" + i + "_" + q);
                for (int q = 0; q < leavesBlockDuplicates; q++)
                    blocksToSmooth.Add("leaves" + i + "_" + q);
                blocksToSmooth.Add("wood" + i);
            }

            for (int z = 0; z < gameWorld.BlockSize.Z; z++)
            {
                for (int x = 0; x < gameWorld.BlockSize.X; x++)
                {
                    for (int y = 0; y < gameWorld.BlockSize.Y; y++)
                    {
                        Block block = gameWorld[x, y, z];
                        if (block == Block.Empty || !blocksToSmooth.Contains(block.Name))
                            continue;

                        CubeSides sides = CubeSides.None;
                        if (z == gameWorld.BlockSize.Z - 1 || (blocksToConsiderEmpty.Contains(gameWorld[x, y, z + 1].Name) && !grassBlocks.Contains(gameWorld[x, y, z + 1].Name)))
                            sides |= CubeSides.ZPlus;
                        if (z == 0 || blocksToConsiderEmpty.Contains(gameWorld[x, y, z - 1].Name))
                            sides |= CubeSides.ZMinus;
                        if (x == 0 || blocksToConsiderEmpty.Contains(gameWorld[x - 1, y, z].Name))
                            sides |= CubeSides.XMinus;
                        if (x == gameWorld.BlockSize.X - 1 || blocksToConsiderEmpty.Contains(gameWorld[x + 1, y, z].Name))
                            sides |= CubeSides.XPlus;
                        if (y == gameWorld.BlockSize.Y - 1 || blocksToConsiderEmpty.Contains(gameWorld[x, y + 1, z].Name))
                            sides |= CubeSides.YPlus;
                        if (y == 0 || blocksToConsiderEmpty.Contains(gameWorld[x, y - 1, z].Name))
                            sides |= CubeSides.YMinus;

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
                        else if (blockNamePart == "bumpyDirt")
                            gameWorld[x, y, z] = bumpyDirtRandomizers[(int)sides].NextBlock();
                        else
                            gameWorld[x, y, z] = Factory.Get<Block>(blockNamePart + (int)sides);
                    }
                }
            }
        }
    }
}
