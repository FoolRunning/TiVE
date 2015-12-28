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
        public string BlockListForWorld(string gameWorldName)
        {
            return gameWorldName == "Loading" ? "loading" : null;
        }

        public IGameWorld CreateGameWorld(string gameWorldName, IBlockList blockList)
        {
            if (gameWorldName != "Loading")
                return null;

            int dataXSize = loadingLevel.GetLength(1);
            int dataYSize = loadingLevel.GetLength(0);
            const int worldSizeX = 75;
            const int worldSizeY = 50;
            IGameWorld gameWorld = Factory.CreateGameWorld(worldSizeX, worldSizeY, 8);
            gameWorld.LightingModelType = LightingModelType.Realistic;

            BlockRandomizer grasses = new BlockRandomizer(blockList, "grass", 50);
            ushort dirt = blockList["dirt"];
            ushort stoneBack = blockList["backStone"];
            ushort stone = blockList["ston0"];
            ushort light = blockList["loadingLight"];
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

            SmoothWorld(gameWorld, blockList);
            return gameWorld;
        }
        #endregion

        private static void SmoothWorld(IGameWorld gameWorld, IBlockList blockList)
        {
            HashSet<int> blocksToConsiderEmpty = new HashSet<int>();
            blocksToConsiderEmpty.Add(0);
            for (int i = 0; i < 50; i++)
                blocksToConsiderEmpty.Add(blockList["grass" + i]);
            for (int i = 0; i < 6; i++)
                blocksToConsiderEmpty.Add(blockList["light" + i]);

            for (int z = 0; z < gameWorld.BlockSize.Z; z++)
            {
                for (int x = 0; x < gameWorld.BlockSize.X; x++)
                {
                    for (int y = 0; y < gameWorld.BlockSize.Y; y++)
                    {
                        ushort blockIndex = gameWorld[x, y, z];
                        if (blockIndex == 0)
                            continue;

                        Block block = blockList[blockIndex];
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

                        gameWorld[x, y, z] = blockList[blockNameKey + sides];
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
