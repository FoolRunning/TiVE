using System;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.ProjectM.Controllers
{
    /// <summary>
    /// World generation stage to create the bioms of the game world
    /// </summary>
    public sealed class WorldGenCreateBiomes : IWorldGeneratorStage
    {
        /// <summary>
        /// Updates the specified gameworld with blocks
        /// </summary>
        public void UpdateWorld(IGameWorld gameWorld, long seed, IBlockList blockList)
        {
            Random random = new Random((int)((seed >> 20) & 0xFFFFFFFF));
            double offset1 = random.NextDouble() * 100.0 - 50.0;
            double offset2 = random.NextDouble() * 40.0 - 20.0;
            double offset3 = random.NextDouble() * 6.0 - 3.0;

            double scale1 = random.NextDouble() * 0.003 + 0.001;
            double scale2 = random.NextDouble() * 0.01 + 0.005;
            double scale3 = random.NextDouble() * 0.04 + 0.02;

            //Debug.WriteLine(scale1 + ", " + scale2 + ", " + scale3);

            for (int x = 0; x < gameWorld.BlockSizeX; x++)
            {
                double noise = Noise.GetNoise((offset1 + x) * scale1) * 0.6 +
                    Noise.GetNoise((offset2 + x) * scale2) * 0.25 +
                    Noise.GetNoise((offset3 + x) * scale3) * 0.15;

                int bottomY = gameWorld.BlockSizeY - (int) (noise * 75.0) - 125;
                FillColumn(gameWorld, x, bottomY);
            }
        }

        public ushort Priority
        {
            get { return 100; }
        }

        public string StageDescription
        {
            get { return "Creating Biomes"; }
        }

        private void FillColumn(IGameWorld gameWorld, int x, int topY)
        {
            //for (int y = topY; y >= 0; y--)
            //    gameWorld.SetBiome(x, y, 1);
        }
    }
}
