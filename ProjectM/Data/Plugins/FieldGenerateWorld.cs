using JetBrains.Annotations;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Generators;

namespace ProdigalSoftware.ProjectM.Data.Plugins
{
    [UsedImplicitly]
    public class FieldGenerateWorld : IWorldGenerator
    {
        private static readonly RandomGenerator random = new RandomGenerator();
        
        public IGameWorld CreateGameWorld(string gameWorldName)
        {
            if (gameWorldName != "Field")
                return null;

            IGameWorld gameWorld = Factory.NewGameWorld(1000, 1000, 16);
            gameWorld.LightingModelType = LightingModelType.Realistic;

            FillWorld(gameWorld);
            gameWorld[400, 400, 4] = Factory.Get<Block>("player");
            CommonUtils.SmoothGameWorldForMazeBlocks(gameWorld, false);

            return gameWorld;
        }

        private static void FillWorld(IGameWorld gameWorld)
        {
            BlockRandomizer grasses = new BlockRandomizer("grass", CommonUtils.grassBlockDuplicates);
            BlockRandomizer unlitGrasses = new BlockRandomizer("loadingGrass", CommonUtils.grassBlockDuplicates);
            //Block player = Factory.Get<Block>("player");
            Block dirt = Factory.Get<Block>("dirt");
            //Block stone = Factory.Get<Block>("stoneBrick0_0");
            //Block stoneBack = Factory.Get<Block>("back0_0");
            Block wood = Factory.Get<Block>("wood0");
            Block leaves = Factory.Get<Block>("leaves0_0");
            //Block fountain = Factory.Get<Block>("fountain");
            //Block smallLight = Factory.Get<Block>("smallLight");
            //Block redLight = Factory.Get<Block>("redLight");
            Block treeLight = Factory.Get<Block>("treeLight");
            //Block smallLightHover = blockList["smallLightHover"];
            //Block fire = Factory.Get<Block>("fire");
            //Block lava = Factory.Get<Block>("lava");

            for (int x = 0; x < gameWorld.BlockSize.X; x++)
            {
                for (int y = 0; y < gameWorld.BlockSize.Y; y++)
                {
                    gameWorld[x, y, 0] = dirt;
                    gameWorld[x, y, 1] = dirt;
                    //gameWorld[x, y, 2] = grasses.NextBlock();
                    if (random.Next(100) < 5 && x % 3 == 1 && y % 3 == 1)
                        gameWorld[x, y, 8] = treeLight;

                    if (random.Next(100) < 5 && x % 3 == 1 && y % 3 == 1 && x >= 6 && y >= 6 && x < gameWorld.BlockSize.X - 6 && y < gameWorld.BlockSize.Y - 6)
                    {
                        // Add a tree
                        gameWorld[x, y, 2] = wood;
                        gameWorld[x, y, 3] = wood;
                        gameWorld[x, y, 4] = wood;
                        gameWorld[x, y, 5] = wood;
                        gameWorld[x, y, 6] = wood;
                        gameWorld[x, y, 7] = wood;
                        gameWorld[x, y, 8] = wood;
                        gameWorld[x, y, 9] = wood;
                        gameWorld[x, y, 10] = wood;
                        gameWorld[x - 1, y, 10] = wood;
                        gameWorld[x + 1, y, 10] = wood;
                        gameWorld[x, y - 1, 10] = wood;
                        gameWorld[x, y + 1, 10] = wood;
                        for (int z = 11; z < 16; z++)
                        {
                            for (int lx = x - 6; lx <= x + 6; lx++)
                            {
                                for (int ly = y - 6; ly <= y + 6; ly++)
                                {
                                    int dist = (lx - x) * (lx - x) + (ly - y) * (ly - y) + (15 - z) * (15 - z);
                                    if (dist <= 9)
                                        gameWorld[lx, ly, z] = wood;
                                    else if (dist <= 25)//(z - 9) * (z - 9))
                                        gameWorld[lx, ly, z] = leaves;
                                }
                            }
                        }
                        gameWorld[x, y, 11] = wood;
                        gameWorld[x - 1, y, 11] = wood;
                        gameWorld[x + 1, y, 11] = wood;
                        gameWorld[x, y - 1, 11] = wood;
                        gameWorld[x, y + 1, 11] = wood;
                    }
                    else
                        gameWorld[x, y, 2] = unlitGrasses.NextBlock();
                }
            }
        }
    }
}
