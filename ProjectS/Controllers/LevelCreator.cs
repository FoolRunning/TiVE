using System;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.ProjectS.Controllers
{
    /// <summary>
    /// World generation stage to fill the game world with blocks.
    /// </summary>
    public class LevelCreator : IWorldGeneratorStage
    {
        /// <summary>
        /// Updates the specified gameworld with blocks
        /// </summary>
        public void UpdateWorld(IGameWorld gameWorld, long seed, IBlockList blockList)
        {
            BlockInformation wallBottom = blockList["WallBottom"];
            BlockInformation wallTop = blockList["WallTop"];
            BlockInformation wallEndBottom = blockList["WallEndBottom"];
            BlockInformation wallEndTop = blockList["WallEndTop"];
            BlockInformation light1 = blockList["Light1"];
            BlockInformation light2 = blockList["Light2"];
            BlockInformation light3 = blockList["Light3"];
            BlockInformation floorBright = blockList["FloorWhite"];
            BlockInformation floorDark = blockList["FloorDark"];

            Random random = new Random();

            for (int x = 0; x < gameWorld.BlockSize.X; x++)
            {
                for (int y = 0; y < gameWorld.BlockSize.Y; y++)
                {
                    gameWorld[x, y, 0] = (x + y) % 2 == 0 ? floorBright : floorDark;

                    if (x == 0 || x == gameWorld.BlockSize.X - 1 || y == 0 || y == gameWorld.BlockSize.Y - 1)
                    {
                        gameWorld[x, y, 1] = wallBottom;
                        gameWorld[x, y, 2] = wallTop;

                        if (x % 10 == 0 && y % 10 == 0)
                        {
                            double rnd = random.NextDouble();
                            if (rnd < 0.3333333)
                                gameWorld[x, y, 3] = light1;
                            else if (rnd < 0.6666666)
                                gameWorld[x, y, 3] = light2;
                            else
                                gameWorld[x, y, 3] = light3;
                        }
                    }
                }
            }
        }

        public ushort Priority
        {
            get { return 100; }
        }

        public string StageDescription
        {
            get { return "Loading Level"; }
        }
    }
}
