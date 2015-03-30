using System;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Generators;

namespace ProdigalSoftware.ProjectM.Plugins
{
    /// <summary>
    /// World generation to create a random world
    /// </summary>
    public class LiquidTestGenerateWorld : IWorldGenerator
    {
        private const int Front = 1;
        private const int Back = 2;
        private const int Left = 4;
        private const int Right = 8;
        private const int Top = 16;
        private const int Bottom = 32;

        public string BlockListForWorld(string gameWorldName)
        {
            return gameWorldName == "LiquidTest" ? "liquid" : null;
        }

        public IGameWorld CreateGameWorld(string gameWorldName, IBlockList blockList)
        {
            if (gameWorldName != "LiquidTest")
                return null;

            IGameWorld gameWorld = Factory.CreateGameWorld(50, 50, 20);
            gameWorld.LightingModelType = LightingModelType.Fantasy2;
            FillWorld(gameWorld, blockList);
            SmoothWorld(gameWorld, blockList);

            return gameWorld;
        }

        #region Private helper methods
        private static void FillWorld(IGameWorld gameWorld, IBlockList blockList)
        {
            BlockRandomizer backWalls = new BlockRandomizer(blockList, "back", 5);
            BlockRandomizer lights = new BlockRandomizer(blockList, "light", 7);

            ushort sand = blockList["sand0"];
            ushort lava = blockList["lava0"];
            ushort stone = blockList["ston0"];
            ushort fire = blockList["fire"];
            ushort fountain = blockList["fountain"];
            for (int x = 20; x < 30; x++)
            {
                for (int y = 20; y < 30; y++)
                    gameWorld[x, y, 0] = stone;
            }

            gameWorld[20, 20, 11] = lights.NextBlock();
            gameWorld[29, 20, 11] = lights.NextBlock();
            gameWorld[20, 29, 11] = lights.NextBlock();
            gameWorld[29, 29, 11] = lights.NextBlock();
            for (int n = 20; n < 30; n++)
            {
                for (int z = 1; z < 10; z++)
                {
                    gameWorld[n, 20, z] = stone;
                    gameWorld[n, 29, z] = stone;
                    gameWorld[20, n, z] = stone;
                    gameWorld[29, n, z] = stone;
                }
            }
        }

        private static void SmoothWorld(IGameWorld gameWorld, IBlockList blockList)
        {
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

                        if (z == 0 || GetBlockSet(blockList[gameWorld[x, y, z - 1]]) != blockNameKey)
                            sides |= Back;

                        if (z == gameWorld.BlockSize.Z - 1 || GetBlockSet(blockList[gameWorld[x, y, z + 1]]) != blockNameKey)
                            sides |= Front;

                        if (x == 0 || GetBlockSet(blockList[gameWorld[x - 1, y, z]]) != blockNameKey)
                            sides |= Left;

                        if (x == gameWorld.BlockSize.X - 1 || GetBlockSet(blockList[gameWorld[x + 1, y, z]]) != blockNameKey)
                            sides |= Right;

                        if (y == 0 || GetBlockSet(blockList[gameWorld[x, y - 1, z]]) != blockNameKey)
                            sides |= Bottom;

                        if (y == gameWorld.BlockSize.Y - 1 || GetBlockSet(blockList[gameWorld[x, y + 1, z]]) != blockNameKey)
                            sides |= Top;

                        gameWorld[x, y, z] = blockList[blockNameKey + sides];
                    }
                }
            }
        }

        private static string GetBlockSet(Block block)
        {
            return block.BlockName.Substring(0, 4);
        }
        #endregion

        #region BlockRandomizer class
        private sealed class BlockRandomizer
        {
            public readonly ushort[] Blocks;
            private readonly Random random = new Random();

            public BlockRandomizer(IBlockList blockList, string blockname, int blockCount)
            {
                Blocks = new ushort[blockCount];
                for (int i = 0; i < Blocks.Length; i++)
                    Blocks[i] = blockList[blockname + i];
            }

            public ushort NextBlock()
            {
                int blockNum = random.Next(Blocks.Length);
                return Blocks[blockNum];
            }
        }
        #endregion
    }
}
