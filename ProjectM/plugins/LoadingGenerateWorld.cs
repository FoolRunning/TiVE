using System.Collections.Generic;
using JetBrains.Annotations;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Generators;

namespace ProdigalSoftware.ProjectM.Plugins
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

        private const int Front = 1;
        private const int Back = 2;
        private const int Left = 4;
        private const int Right = 8;
        private const int Top = 16;
        private const int Bottom = 32;

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

            BlockRandomizer grasses = new BlockRandomizer("grass", 50);
            Block dirt = Factory.Get<Block>("dirt");
            Block stoneBack = Factory.Get<Block>("backStone");
            Block stone = Factory.Get<Block>("ston0");
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

            SmoothWorld(gameWorld);
            return gameWorld;
        }
        #endregion

        private static void SmoothWorld(IGameWorld gameWorld)
        {
            HashSet<Block> blocksToConsiderEmpty = new HashSet<Block>();
            blocksToConsiderEmpty.Add(Block.Empty);
            for (int i = 0; i < 50; i++)
                blocksToConsiderEmpty.Add(Factory.Get<Block>("grass" + i));
            for (int i = 0; i < 6; i++)
                blocksToConsiderEmpty.Add(Factory.Get<Block>("light" + i));

            for (int z = 0; z < gameWorld.BlockSize.Z; z++)
            {
                for (int x = 0; x < gameWorld.BlockSize.X; x++)
                {
                    for (int y = 0; y < gameWorld.BlockSize.Y; y++)
                    {
                        Block block = gameWorld[x, y, z];
                        if (block == Block.Empty)
                            continue;

                        string blockNameKey = GetBlockSet(block);

                        if (blockNameKey != "ston" && blockNameKey != "sand" && blockNameKey != "lava")
                            continue;

                        int sides = 0;

                        if (z == 0 || blocksToConsiderEmpty.Contains(gameWorld[x, y, z - 1]))
                            sides |= Back;

                        if (z == gameWorld.BlockSize.Z - 1 || blocksToConsiderEmpty.Contains(gameWorld[x, y, z + 1]))
                            sides |= Front;

                        if (x == 0 || blocksToConsiderEmpty.Contains(gameWorld[x - 1, y, z]))
                            sides |= Left;

                        if (x == gameWorld.BlockSize.X - 1 || blocksToConsiderEmpty.Contains(gameWorld[x + 1, y, z]))
                            sides |= Right;

                        if (y == 0 || blocksToConsiderEmpty.Contains(gameWorld[x, y - 1, z]))
                            sides |= Bottom;

                        if (y == gameWorld.BlockSize.Y - 1 || blocksToConsiderEmpty.Contains(gameWorld[x, y + 1, z]))
                            sides |= Top;

                        gameWorld[x, y, z] = Factory.Get<Block>(blockNameKey + sides);
                    }
                }
            }
        }

        private static string GetBlockSet(Block block)
        {
            return block.Name.Substring(0, 4);
        }
    }
}
