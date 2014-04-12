using System;
using System.Diagnostics;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using ProdigalSoftware.TiVEPluginFramework;

namespace WorldCreation
{
    /// <summary>
    /// World generation stage to fill the game world with blocks.
    /// </summary>
    public class WorldGenFillWorld : IWorldGeneratorStage
    {
        /// <summary>
        /// Updates the specified gameworld with blocks
        /// </summary>
        public void UpdateWorld(IGameWorld gameWorld, long seed, IBlockList blockList)
        {
            BlockRandomizer backWalls = new BlockRandomizer(blockList, "back", 4);
            BlockRandomizer dirts = new BlockRandomizer(blockList, "dirt", 4);
            BlockRandomizer stones = new BlockRandomizer(blockList, "stone", 4);
            BlockRandomizer sands = new BlockRandomizer(blockList, "sand", 4);
            ushort fountain = blockList.GetBlockIndex("fountain");

            Random random1 = new Random((int)((seed >> 11) & 0xFFFFFFFF));
            Random random2 = new Random((int)((seed >> 15) & 0xFFFFFFFF));

            double xOff1 = random1.NextDouble() * 50.0 - 25.0;
            double yOff1 = random1.NextDouble() * 50.0 - 25.0;
            double xOff2 = random1.NextDouble() * 100.0 - 50.0;
            double yOff2 = random1.NextDouble() * 100.0 - 50.0;
            double xOff3 = random1.NextDouble() * 500.0 - 250.0;
            double yOff3 = random1.NextDouble() * 500.0 - 250.0;

            double scaleX1 = random2.NextDouble() * 0.15 + 0.01;
            double scaleY1 = random2.NextDouble() * 0.15 + 0.02;
            double scaleX2 = random2.NextDouble() * 0.10 + 0.05;
            double scaleY2 = random2.NextDouble() * 0.10 + 0.05;
            double scaleX3 = random2.NextDouble() * 0.03 + 0.07;
            double scaleY3 = random2.NextDouble() * 0.03 + 0.07;

            // Use parallel for for speed since there is no syncing needed
            Parallel.For(0, gameWorld.Xsize, x =>
            {
                for (int y = 0; y < gameWorld.Ysize; y++)
                {
                    //if (gameWorld.GetBiome(x, y) == 0)
                    //    continue;
                    double noiseVal = Noise.GetNoise((xOff1 + x) * scaleX1, (yOff1 + y) * scaleY1) * 0.2f +
                            Noise.GetNoise((xOff2 + x) * scaleX2, (yOff2 + y) * scaleY2) * 0.5f +
                            Noise.GetNoise((xOff3 + x) * scaleX3, (yOff3 + y) * scaleY3) * 0.3f;
                    gameWorld.SetBlock(x, y, 0, backWalls.NextBlock());
                    int depth = 1;
                    if (noiseVal > 0.2)
                    {
                        Fill(gameWorld, x, y, ref depth, dirts);
                        if (noiseVal > 0.4)
                            Fill(gameWorld, x, y, ref depth, dirts);
                        if (noiseVal > 0.6)
                            Fill(gameWorld, x, y, ref depth, dirts);
                        if (noiseVal > 0.8)
                            Fill(gameWorld, x, y, ref depth, dirts);
                        if (noiseVal > 0.85)
                            gameWorld.SetBlock(x, y, 13, fountain);
                    }
                    else if (noiseVal < -0.3)
                    {
                        Fill(gameWorld, x, y, ref depth, sands);
                        if (noiseVal < -0.4)
                            Fill(gameWorld, x, y, ref depth, sands);
                        if (noiseVal < -0.6)
                            Fill(gameWorld, x, y, ref depth, sands);
                        if (noiseVal < -0.8)
                            Fill(gameWorld, x, y, ref depth, sands);
                        if (noiseVal < -0.85)
                            gameWorld.SetBlock(x, y, 13, fountain);
                    }
                    else
                    {
                        Fill(gameWorld, x, y, ref depth, stones);
                        if (noiseVal > -0.3 && noiseVal < 0.2)
                            Fill(gameWorld, x, y, ref depth, stones);
                        if (noiseVal > -0.2 && noiseVal < 0.1)
                            Fill(gameWorld, x, y, ref depth, stones);
                        if (noiseVal > -0.1 && noiseVal <= 0.0)
                            Fill(gameWorld, x, y, ref depth, stones);
                        if (noiseVal > -0.05 && noiseVal < -0.03)
                            gameWorld.SetBlock(x, y, 13, fountain);
                    }
                }
            });
        }

        private void Fill(IGameWorld gameWorld, int x, int y, ref int depth, BlockRandomizer block)
        {
            for (int i = 0; i < 3; i++)
                gameWorld.SetBlock(x, y, depth++, block.NextBlock());
        }

        public ushort Priority
        {
            get { return 200; }
        }

        public string StageDescription
        {
            get { return "Filling World"; }
        }

        private sealed class BlockRandomizer
        {
            private readonly ushort[] blockIds;
            private readonly int blockCount;
            private readonly Random random = new Random();

            public BlockRandomizer(IBlockList blockList, string blockname, int blockCount)
            {
                this.blockCount = blockCount;
                blockIds = new ushort[blockCount];
                for (int i = 0; i < blockCount; i++)
                    blockIds[i] = blockList.GetBlockIndex(blockname + i);
            }

            public ushort NextBlock()
            {
                int blockNum;
                lock(random)
                    blockNum = random.Next(blockCount);
                return blockIds[blockNum];
            }
        }
    }
}
