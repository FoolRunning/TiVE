using System;
using ProdigalSoftware.TiVEPluginFramework;

namespace WorldCreation
{
    public class WorldGenStage1 : IWorldGenerator
    {
        public void UpdateWorld(IGameWorld gameWorld, long seed, IBlockList blockList)
        {
            Random random = new Random((int)((seed >> 11) & 0xFFFFFFFF));
            ushort[] dirts = new ushort[4];
            for (int i = 0; i < 4; i++)
                dirts[i] = blockList.GetBlockIndex("dirt" + i);

            for (int x = 0; x < gameWorld.Xsize; x++)
            {
                for (int y = 0; y < gameWorld.Ysize; y++)
                    gameWorld.SetBlock(x, y, 1, dirts[random.Next(4)]);
            }
        }

        public uint Priority
        {
            get { return 2000; }
        }
    }
}
