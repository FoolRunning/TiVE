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
            { 0, 0, 0,55, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,55, 0, 0, 0, 0, 0, 0, 0, 0, 0,55, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,55, 0, 0, 0, 0, 0, 0, 0, 0, 0,55, 0, 0, 0 },
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
            gameWorld.LightingModelType = LightingModelType.Fantasy3;

            BlockRandomizer grasses = new BlockRandomizer("grass", CommonUtils.grassBlockDuplicates);
            Block dirt = Factory.Get<Block>("dirt");
            Block stoneBack = Factory.Get<Block>("backStone");
            Block stone = Factory.Get<Block>("ston0_0");
            Block light = Factory.Get<Block>("loadingLight");
            for (int x = 0; x < worldSizeX; x++)
            {
                for (int y = 0; y < worldSizeY; y++)
                {
                    gameWorld[x, y, 0] = dirt;
                    gameWorld[x, y, 1] = grasses.NextBlock();
                }
            }

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

            CommonUtils.SmoothGameWorldForMazeBlocks(gameWorld);
            return gameWorld;
        }
        #endregion
    }
}
