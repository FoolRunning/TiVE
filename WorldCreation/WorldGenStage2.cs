using System;
using ProdigalSoftware.TiVEPluginFramework;

namespace WorldCreation
{
    public class WorldGenStage2 : IWorldGenerator
    {
        public void UpdateWorld(IGameWorld gameWorld, long seed, IBlockList blockList)
        {
            Random random1 = new Random((int)(seed & 0xFFFFFFFF));
            double xOff1 = random1.NextDouble() * 500.0 - 250.0;
            double yOff1 = random1.NextDouble() * 500.0 - 250.0;
            double xOff2 = random1.NextDouble() * 1000.0 - 500.0;
            double yOff2 = random1.NextDouble() * 1000.0 - 500.0;
            double xOff3 = random1.NextDouble() * 5000.0 - 2500.0;
            double yOff3 = random1.NextDouble() * 5000.0 - 2500.0;

            Random random2 = new Random((int)((seed >> 32) & 0xFFFFFFFF));
            double scaleX1 = random2.NextDouble() * 0.07;
            double scaleY1 = random2.NextDouble() * 0.07;
            double scaleX2 = random2.NextDouble() * 0.1;
            double scaleY2 = random2.NextDouble() * 0.1;
            double scaleX3 = random2.NextDouble() * 0.25;
            double scaleY3 = random2.NextDouble() * 0.25;

            for (int x = 0; x < gameWorld.Xsize; x++)
            {
                for (int y = 0; y < gameWorld.Ysize; y++)
                {
                    float noiseVal =
                        Noise.GetNoise((xOff1 + x) * scaleX1, (yOff1 + y) * scaleY1) * 0.5f +
                        Noise.GetNoise((xOff2 + x) * scaleX2, (yOff2 + y) * scaleY2) * 0.3f +
                        Noise.GetNoise((xOff3 + x) * scaleX3, (yOff3 + y) * scaleY3) * 0.2f;
                    if (noiseVal > 0.6f)
                        gameWorld.SetBlock(x, y, 1, 0);
                }
            }
        }

        public uint Priority
        {
            get { return 3000; }
        }
    }
}
