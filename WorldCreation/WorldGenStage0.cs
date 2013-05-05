using System;
using ProdigalSoftware.TiVEPluginFramework;

namespace WorldCreation
{
    public class WorldGenStage0 : IWorldGenerator
    {
        public void UpdateWorld(IGameWorld gameWorld, long seed, IBlockList blockList)
        {
            Random random = new Random((int)((seed >> 5) & 0xFFFFFFFF));
            ushort[] backWalls = new ushort[4];
            for (int i = 0; i < 4; i++)
                backWalls[i] = blockList.GetBlockIndex("back" + i);

            for (int x = 0; x < gameWorld.Xsize; x++)
            {
                for (int y = 0; y < gameWorld.Ysize; y++)
                    gameWorld.SetBlock(x, y, 0, backWalls[random.Next(4)]);
            }
        }

        public uint Priority
        {
            get { return 1000; }
        }
    }
}
