using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ProdigalSoftware.TiVEPluginFramework;

namespace WorldCreation
{
    /// <summary>
    /// World generation stage to fill the game world with blocks.
    /// </summary>
    public class WorldGenFillWorld : IWorldGeneratorStage
    {
        private Random random;
        private readonly ushort[] backWalls = new ushort[4];
        private readonly ushort[] dirts = new ushort[4];
        private readonly ushort[] stones = new ushort[4];
        private readonly ushort[] sands = new ushort[4];

        /// <summary>
        /// Updates the specified gameworld with blocks
        /// </summary>
        public void UpdateWorld(IGameWorld gameWorld, long seed, IBlockList blockList)
        {
            for (int i = 0; i < 4; i++)
            {
                backWalls[i] = blockList.GetBlockIndex("back" + i);
                dirts[i] = blockList.GetBlockIndex("dirt" + i);
                stones[i] = blockList.GetBlockIndex("stone" + i);
                sands[i] = blockList.GetBlockIndex("sand" + i);
            }


            random = new Random((int)((seed >> 20) & 0xFFFFFFFF));
            double offset1 = random.NextDouble() * 100.0 - 50.0;
            double offset2 = random.NextDouble() * 40.0 - 20.0;
            double offset3 = random.NextDouble() * 6.0 - 3.0;

            double scale1 = random.NextDouble() * 0.003 + 0.001;
            double scale2 = random.NextDouble() * 0.01 + 0.005;
            double scale3 = random.NextDouble() * 0.04 + 0.02;

            Debug.WriteLine(scale1 + ", " + scale2 + ", " + scale3);
            
            // Use parallel for for speed since there is no syncing needed
            Parallel.For(0, gameWorld.Xsize, x =>
                {
                    double noise = Noise.GetNoise((offset1 + x) * scale1) * 0.6 +
                        Noise.GetNoise((offset2 + x) * scale2) * 0.25 +
                        Noise.GetNoise((offset3 + x) * scale3) * 0.15;

                    int bottomY = gameWorld.Ysize - (int) (noise * 75.0) - 125;
                    FillColumn(gameWorld, x, bottomY);
                });
        }

        public ushort Priority
        {
            get { return 200; }
        }

        public string StageDescription
        {
            get { return "Filling World"; }
        }

        private void FillColumn(IGameWorld gameWorld, int x, int topY)
        {
            for (int y = topY; y >= 0; y--)
            {
                gameWorld.SetBlock(x, y, 0, backWalls[0/*random.Next(4)*/]);
                double rand = ((x * gameWorld.Ysize + y) % 17) / 17.0;
                //double rand = random.NextDouble();
                ushort block;
                if (rand < .2)
                    block = stones[0/*random.Next(4)*/];
                else if (rand < .5)
                    block = sands[0/*random.Next(4)*/];
                else
                    block = dirts[0/*random.Next(4)*/];
                gameWorld.SetBlock(x, y, 1, block);
            }
        }
    }
}
