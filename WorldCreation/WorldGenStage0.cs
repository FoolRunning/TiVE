using ProdigalSoftware.TiVEPluginFramework;

namespace WorldCreation
{
    public class WorldGenStage0 : IWorldGenerator
    {
        public void UpdateWorld(IGameWorld gameWorld, long seed, IBlockList blockList)
        {
            ushort backWallIndex = blockList.GetBlockIndex("backwall1");

            for (int x = 0; x < gameWorld.Xsize; x++)
            {
                for (int y = 0; y < gameWorld.Ysize; y++)
                    gameWorld.SetBlock(x, y, 0, backWallIndex);
            }
        }

        public uint Priority
        {
            get { return 1000; }
        }
    }
}
