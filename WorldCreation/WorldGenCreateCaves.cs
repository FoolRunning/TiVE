using System;
using System.Threading.Tasks;
using ProdigalSoftware.TiVEPluginFramework;

namespace WorldCreation
{
    /// <summary>
    /// World generation stage to create caves
    /// </summary>
    public sealed class WorldGenCreateCaves : IWorldGeneratorStage
    {
        /// <summary>
        /// Updates the world with random cave formations. Caves are generated using Perlin Simplex noise using the specified seed value to
        /// generate a little randomness.
        /// </summary>
        public void UpdateWorld(IGameWorld gameWorld, long seed, IBlockList blockList)
        {
            Random random1 = new Random((int)(seed & 0xFFFFFFFF));
            Random random2 = new Random((int)((seed >> 32) & 0xFFFFFFFF));

            double xOff1 = random1.NextDouble() * 50.0 - 25.0;
            double yOff1 = random1.NextDouble() * 50.0 - 25.0;
            double xOff2 = random1.NextDouble() * 100.0 - 50.0;
            double yOff2 = random1.NextDouble() * 100.0 - 50.0;
            double xOff3 = random1.NextDouble() * 500.0 - 250.0;
            double yOff3 = random1.NextDouble() * 500.0 - 250.0;

            double scaleX1 = random2.NextDouble() * 0.04 + 0.01;
            double scaleY1 = random2.NextDouble() * 0.05 + 0.02;
            double scaleX2 = random2.NextDouble() * 0.10 + 0.05;
            double scaleY2 = random2.NextDouble() * 0.10 + 0.05;
            double scaleX3 = random2.NextDouble() * 0.20 + 0.07;
            double scaleY3 = random2.NextDouble() * 0.20 + 0.07;

            // Use parallel for for speed since there is no syncing needed
            Parallel.For(0, gameWorld.Xsize, x =>
            //for (int x = 0; x < gameWorld.Xsize; x++)
                {
                    for (int y = 0; y < gameWorld.Ysize; y++)
                    {
                        double noiseVal = Noise.GetNoise((xOff1 + x) * scaleX1, (yOff1 + y) * scaleY1) * 0.5f +
                                Noise.GetNoise((xOff2 + x) * scaleX2, (yOff2 + y) * scaleY2) * 0.3f +
                                Noise.GetNoise((xOff3 + x) * scaleX3, (yOff3 + y) * scaleY3) * 0.2f;
                        if (noiseVal > 0.2)
                        {
                            gameWorld.SetBlock(x, y, 1, 0);
                            gameWorld.SetBlock(x, y, 2, 0);
                            gameWorld.SetBlock(x, y, 3, 0);
                        }
                        if (noiseVal > 0.3)
                        {
                            gameWorld.SetBlock(x, y, 4, 0);
                            gameWorld.SetBlock(x, y, 5, 0);
                            gameWorld.SetBlock(x, y, 6, 0);
                        }
                        if (noiseVal > 0.4)
                        {
                            gameWorld.SetBlock(x, y, 7, 0);
                            gameWorld.SetBlock(x, y, 8, 0);
                            gameWorld.SetBlock(x, y, 9, 0);
                        }
                        if (noiseVal > 0.5)
                        {
                            gameWorld.SetBlock(x, y, 10, 0);
                            gameWorld.SetBlock(x, y, 11, 0);
                            gameWorld.SetBlock(x, y, 12, 0);
                        }
                        if (noiseVal > 0.6)
                        {
                            gameWorld.SetBlock(x, y, 13, 0);
                            gameWorld.SetBlock(x, y, 14, 0);
                            gameWorld.SetBlock(x, y, 15, 0);
                        }
                    }
                });
        }

        public ushort Priority
        {
            get { return 300; }
        }

        public string StageDescription
        {
            get { return "Creating Caves"; }
        }
    }
}
