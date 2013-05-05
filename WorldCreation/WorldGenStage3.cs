using System;
using ProdigalSoftware.TiVEPluginFramework;

namespace WorldCreation
{
    public class WorldGenStage3 : IWorldGenerator
    {
        public void UpdateWorld(IGameWorld gameWorld, long seed, IBlockList blockList)
        {
            Random random = new Random((int)((seed >> 20) & 0xFFFFFFFF));

            double offset1 = random.NextDouble() * 100.0 - 50.0;
            double offset2 = random.NextDouble() * 100.0 - 50.0;
            double offset3 = random.NextDouble() * 100.0 - 50.0;
            double scale1 = random.NextDouble() * 0.003 + 0.0005;
            double scale2 = random.NextDouble() * 0.005 + 0.005;
            double scale3 = random.NextDouble() * 0.07 + 0.005;

            for (int x = 0; x < gameWorld.Xsize; x++)
            {
                float noise =
                    Noise.GetNoise((offset1 + x) * scale1) * 0.6f +
                        Noise.GetNoise((offset2 + x) * scale2) * 0.25f +
                        Noise.GetNoise((offset3 + x) * scale3) * 0.15f;

                int bottomY = gameWorld.Ysize - (int)(noise * 150.0f) - 50;
                ClearAll(gameWorld, x, bottomY);
            }

            //int startY = gameWorld.Ysize - random.Next(100) - 50;
            //int prevY = startY;

            //ClearAll(gameWorld, startX, prevY);

            //for (int x = startX + 1; x < gameWorld.Xsize; x++)
            //{
            //    int newY = prevY + (random.Next(5) - 2);
            //    ClearAll(gameWorld, x, newY);
            //    prevY = newY;
            //}

            //prevY = startY;
            //for (int x = startX - 1; x >= 0; x--)
            //{
            //    int newY = prevY + (random.Next(5) - 2);
            //    ClearAll(gameWorld, x, newY);
            //    prevY = newY;
            //}
        }

        private static void ClearAll(IGameWorld gameWorld, int x, int bottomY)
        {
            for (int y = gameWorld.Ysize - 1; y >= bottomY; y--)
            {
                gameWorld.SetBlock(x, y, 0, 0);
                gameWorld.SetBlock(x, y, 1, 0);
            }
        }

        public uint Priority
        {
            get { return 4000; }
        }
    }
}
