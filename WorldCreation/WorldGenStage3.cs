using System;
using ProdigalSoftware.TiVEPluginFramework;

namespace WorldCreation
{
    public class WorldGenStage3 : IWorldGenerator
    {
        public void UpdateWorld(IGameWorld gameWorld, long seed, IBlockList blockList)
        {
            Random random = new Random((int)((seed >> 20) & 0xFFFFFFFF));

            int startX = random.Next(gameWorld.Xsize / 3) + gameWorld.Xsize / 3;
            int startY = gameWorld.Ysize - random.Next(100) - 50;
            int prevY = startY;

            ClearAll(gameWorld, startX, prevY);

            for (int x = startX + 1; x < gameWorld.Xsize; x++)
            {
                int newY = prevY + (random.Next(5) - 2);
                ClearAll(gameWorld, x, newY);
                prevY = newY;
            }

            prevY = startY;
            for (int x = startX - 1; x >= 0; x--)
            {
                int newY = prevY + (random.Next(5) - 2);
                ClearAll(gameWorld, x, newY);
                prevY = newY;
            }
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
