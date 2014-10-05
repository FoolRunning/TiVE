using System;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.ProjectM.Controllers
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
            for (int x = 0; x < gameWorld.BlockSize.X; x++)
            {
                for (int y = 0; y < gameWorld.BlockSize.Y; y++)
                {
                    double noiseVal = Noise.GetNoise((xOff1 + x) * scaleX1, (yOff1 + y) * scaleY1) * 0.7f +
                            Noise.GetNoise((xOff2 + x) * scaleX2, (yOff2 + y) * scaleY2) * 0.4f +
                            Noise.GetNoise((xOff3 + x) * scaleX3, (yOff3 + y) * scaleY3) * 0.2f;
                    if (noiseVal > 0.2)
                    {
                        gameWorld[x, y, 1] = null;
                        gameWorld[x, y, 2] = null;
                        gameWorld[x, y, 3] = null;
                    }
                    if (noiseVal > 0.3)
                    {
                        gameWorld[x, y, 4] = null;
                        gameWorld[x, y, 5] = null;
                        gameWorld[x, y, 6] = null;
                    }
                    if (noiseVal > 0.4)
                    {
                        gameWorld[x, y, 7] = null;
                        gameWorld[x, y, 8] = null;
                        gameWorld[x, y, 9] = null;
                    }
                    if (noiseVal > 0.5)
                    {
                        gameWorld[x, y, 10] = null;
                        gameWorld[x, y, 11] = null;
                        gameWorld[x, y, 12] = null;
                    }
                    if (noiseVal > 0.6)
                    {
                        gameWorld[x, y, 13] = null;
                        gameWorld[x, y, 14] = null;
                        //gameWorld[x, y, 14] null);
                        //gameWorld[x, y, 15] null);
                    }
                }
            }
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
