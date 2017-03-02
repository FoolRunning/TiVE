using JetBrains.Annotations;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Generators;

namespace ProdigalSoftware.ProjectM.Data.Plugins
{
    [UsedImplicitly]
    public class LoadingGenerateWorld : IWorldGenerator
    {
        byte[,] loadingLevel = {
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0,88, 0, 0, 0, 0,88,88, 0, 0, 0,88,88, 0, 0,88,88,88, 0, 0,88,88,88, 0, 0,88, 0,88, 0, 0,88,88, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0,88, 0, 0, 0,88, 0, 0,88, 0,88, 0, 0,88, 0,88, 0, 0,88, 0, 0,88, 0, 0, 0,88, 0,88, 0,88, 0, 0,88, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0,88, 0, 0, 0,88, 0, 0,88, 0,88, 0, 0,88, 0,88, 0, 0,88, 0, 0,88, 0, 0, 0,88, 0,88, 0,88, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0,88, 0, 0, 0,88, 0, 0,88, 0,88,88,88,88, 0,88, 0, 0,88, 0, 0,88, 0, 0,88, 0,88, 0, 0,88, 0,88,88, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0,88, 0, 0, 0,88, 0, 0,88, 0,88, 0, 0,88, 0,88, 0, 0,88, 0, 0,88, 0, 0,88, 0,88, 0, 0,88, 0, 0,88, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0,88,88,88, 0, 0,88,88, 0, 0,88, 0, 0,88, 0,88,88,88, 0, 0,88,88,88, 0,88, 0,88, 0, 0, 0,88,88,88, 0,99, 0,99, 0,99, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,55, 0, 0, 0, 0, 0, 0, 0, 0, 0,55, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,55, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };
        
        #region Implementation of IWorldGenerator
        public IGameWorld CreateGameWorld(string gameWorldName)
        {
            if (gameWorldName != "Loading")
                return null;

            int dataXSize = loadingLevel.GetLength(1);
            int dataYSize = loadingLevel.GetLength(0);
            const int worldSizeX = 75;
            const int worldSizeY = 50;
            IGameWorld gameWorld = Factory.NewGameWorld(worldSizeX, worldSizeY, 8);
            gameWorld.LightingModelType = LightingModelType.Realistic;

            BlockRandomizer grasses = new BlockRandomizer("loadingGrass", CommonUtils.grassBlockDuplicates);
            Block dirt = Factory.Get<Block>("dirt");
            Block stoneBack = Factory.Get<Block>("backStone");
            Block stone = Factory.Get<Block>("back0_0");
            Block light = Factory.Get<Block>("loadingLight");
            for (int x = 0; x < worldSizeX; x++)
            {
                for (int y = 0; y < worldSizeY; y++)
                {
                    gameWorld[x, y, 0] = dirt;
                    gameWorld[x, y, 1] = grasses.NextBlock();
                }
            }
            //for (int color = 0; color < 256; color += 8)
            //    gameWorld[color / 8 + 21, 20, 1] = Factory.Get<Block>("linear" + color);
            gameWorld[25, 20, 1] = Factory.Get<Block>("linear8");
            gameWorld[26, 20, 1] = Factory.Get<Block>("linear16");
            gameWorld[27, 20, 1] = Factory.Get<Block>("linear32");
            gameWorld[28, 20, 1] = Factory.Get<Block>("linear64");
            gameWorld[29, 20, 1] = Factory.Get<Block>("linear128");
            gameWorld[30, 20, 1] = Factory.Get<Block>("linear255");

            const int offsetX = 13;
            const int offsetY = 15;
            for (int dataX = 0; dataX < dataXSize; dataX++)
            {
                int x = offsetX + dataX;
                for (int dataY = 0; dataY < dataYSize; dataY++)
                {
                    int y = offsetY + dataY;
                    int levelData = loadingLevel[dataYSize - dataY - 1, dataX];
                    if (levelData == 55)
                        gameWorld[x, y, 6] = light;
                    else if (levelData == 88)
                    {
                        gameWorld[x, y, 0] = stoneBack;
                        gameWorld[x, y, 1] = stone;
                        gameWorld[x, y, 2] = stone;
                        gameWorld[x, y, 3] = stone;
                    }
                    else if (levelData == 99)
                    {
                        gameWorld[x, y, 0] = stoneBack;
                        gameWorld[x, y, 1] = stone;
                        gameWorld[x, y, 2] = stone;
                    }
                }
            }

            CommonUtils.SmoothGameWorldForMazeBlocks(gameWorld, true);
            return gameWorld;
        }
        #endregion
    }
}
