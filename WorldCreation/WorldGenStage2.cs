using System;
using ProdigalSoftware.TiVEPluginFramework;

namespace WorldCreation
{
    public class WorldGenStage2 : IWorldGenerator
    {
        public void UpdateWorld(IGameWorld gameWorld, long seed, IBlockList blockList)
        {
            Random random1 = new Random((int)(seed & 0xFFFFFFFF));
            Random random2 = new Random((int)((seed >> 32) & 0xFFFFFFFF));

            for (int i = 0; i < 400000; i++)
            {
                int x = random1.Next(gameWorld.Xsize);
                int y = random2.Next(gameWorld.Ysize);
                gameWorld.SetBlock(x, y, 1, 0);
            }
        }

        public uint Priority
        {
            get { return 3000; }
        }
    }
}
