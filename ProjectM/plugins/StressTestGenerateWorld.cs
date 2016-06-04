using System;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Generators;

namespace ProdigalSoftware.ProjectM.Plugins
{
    /// <summary>
    /// World generation to create a random world
    /// </summary>
    public class GenerateRandomWorld : IWorldGenerator
    {
        private const int Front = 1;
        private const int Back = 2;
        private const int Left = 4;
        private const int Right = 8;
        private const int Top = 16;
        private const int Bottom = 32;

        public IGameWorld CreateGameWorld(string gameWorldName)
        {
            if (gameWorldName != "StressTest")
                return null;

            IGameWorld gameWorld = Factory.NewGameWorld(500, 500, 20);
            gameWorld.LightingModelType = LightingModelType.Fantasy1;
            long seed = LongRandom();
            FillWorld(gameWorld, seed);
            CreateCaves(gameWorld, seed);
            SmoothWorld(gameWorld);

            return gameWorld;
        }

        #region Private helper methods
        private static void SmoothWorld(IGameWorld gameWorld)
        {
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
                        if (blockNameKey != "STston" && blockNameKey != "STsand" && blockNameKey != "STlava")
                            continue;

                        int sides = 0;

                        if (z == 0 || GetBlockSet(gameWorld[x, y, z - 1]) != blockNameKey)
                            sides |= Back;

                        if (z == gameWorld.BlockSize.Z - 1 || GetBlockSet(gameWorld[x, y, z + 1]) != blockNameKey)
                            sides |= Front;

                        if (x == 0 || GetBlockSet(gameWorld[x - 1, y, z]) != blockNameKey)
                            sides |= Left;

                        if (x == gameWorld.BlockSize.X - 1 || GetBlockSet(gameWorld[x + 1, y, z]) != blockNameKey)
                            sides |= Right;

                        if (y == 0 || GetBlockSet(gameWorld[x, y - 1, z]) != blockNameKey)
                            sides |= Bottom;

                        if (y == gameWorld.BlockSize.Y - 1 || GetBlockSet(gameWorld[x, y + 1, z]) != blockNameKey)
                            sides |= Top;

                        gameWorld[x, y, z] = Factory.Get<Block>(blockNameKey + sides);
                    }
                }
            }
        }

        private static void FillWorld(IGameWorld gameWorld, long seed)
        {
            BlockRandomizer lights = new BlockRandomizer("STlight", 7);

            Block back = Factory.Get<Block>("STback");
            Block sand = Factory.Get<Block>("STsand0");
            Block lava = Factory.Get<Block>("STlava0");
            Block stone = Factory.Get<Block>("STston0");
            Block fire = Factory.Get<Block>("STfire");
            Block fountain = Factory.Get<Block>("STfountain");

            RandomGenerator random1 = new RandomGenerator((int)((seed >> 11) & 0xFFFFFFFF));
            RandomGenerator random2 = new RandomGenerator((int)((seed >> 15) & 0xFFFFFFFF));

            double xOff1 = random1.NextDouble() * 50.0 - 25.0;
            double yOff1 = random1.NextDouble() * 50.0 - 25.0;
            double xOff2 = random1.NextDouble() * 100.0 - 50.0;
            double yOff2 = random1.NextDouble() * 100.0 - 50.0;
            double xOff3 = random1.NextDouble() * 500.0 - 250.0;
            double yOff3 = random1.NextDouble() * 500.0 - 250.0;

            double scaleX1 = random2.NextDouble() * 0.07 + 0.01;
            double scaleY1 = random2.NextDouble() * 0.07 + 0.02;
            double scaleX2 = random2.NextDouble() * 0.05 + 0.05;
            double scaleY2 = random2.NextDouble() * 0.05 + 0.05;
            double scaleX3 = random2.NextDouble() * 0.015 + 0.07;
            double scaleY3 = random2.NextDouble() * 0.015 + 0.07;

            for (int x = 0; x < gameWorld.BlockSize.X; x++)
            {
                for (int y = 0; y < gameWorld.BlockSize.Y; y++)
                {
                    double noiseVal = Noise.GetNoise((xOff1 + x) * scaleX1, (yOff1 + y) * scaleY1) *
                            Noise.GetNoise((xOff2 + x) * scaleX2, (yOff2 + y) * scaleY2) +
                            Noise.GetNoise((xOff3 + x) * scaleX3, (yOff3 + y) * scaleY3) * 0.5f;
                    gameWorld[x, y, 0] = back;
                    int depth = 1;
                    if (noiseVal > 0.2)
                    {
                        gameWorld[x, y, 1] = lava;
                        //Fill(gameWorld, x, y, ref depth, lava);
                        //if (noiseVal > 0.4)
                        //    Fill(gameWorld, x, y, ref depth, dirt);
                        //if (noiseVal > 0.6)
                        //    Fill(gameWorld, x, y, ref depth, dirt);
                        //if (noiseVal > 0.8)
                        //    Fill(gameWorld, x, y, ref depth, dirt);
                        //if (noiseVal > 0.85)
                        //    gameWorld[x, y, depth] = fountain;
                    }
                    else if (noiseVal < -0.3)
                    {
                        Fill(gameWorld, x, y, ref depth, sand);
                        if (noiseVal < -0.4)
                            Fill(gameWorld, x, y, ref depth, sand);
                        if (noiseVal < -0.6)
                            Fill(gameWorld, x, y, ref depth, sand);
                        if (noiseVal < -0.8)
                            Fill(gameWorld, x, y, ref depth, sand);
                        if (noiseVal < -0.85)
                            gameWorld[x, y, depth] = fountain;
                    }
                    else
                    {
                        Fill(gameWorld, x, y, ref depth, stone);
                        if (noiseVal > -0.3 && noiseVal < 0.2)
                            Fill(gameWorld, x, y, ref depth, stone);
                        if (noiseVal > -0.2 && noiseVal < 0.1)
                            Fill(gameWorld, x, y, ref depth, stone);
                        if (noiseVal > -0.1 && noiseVal <= 0.0)
                            Fill(gameWorld, x, y, ref depth, stone);
                        if (noiseVal > -0.05 && noiseVal < -0.04)
                            gameWorld[x, y, depth] = fire;
                    }
                    if (random1.NextDouble() < 0.02)
                        gameWorld[x, y, random1.Next(gameWorld.BlockSize.Z - 7) + 1] = lights.NextBlock();
                }
            }
        }

        /// <summary>
        /// Updates the world with random cave formations. Caves are generated using Perlin Simplex noise using the specified seed value to
        /// generate a little randomness.
        /// </summary>
        private static void CreateCaves(IGameWorld gameWorld, long seed)
        {
            RandomGenerator random1 = new RandomGenerator((int)(seed & 0xFFFFFFFF));
            RandomGenerator random2 = new RandomGenerator((int)((seed >> 32) & 0xFFFFFFFF));

            double xOff1 = random1.NextDouble() * 50.0 - 25.0;
            double yOff1 = random1.NextDouble() * 50.0 - 25.0;
            double xOff2 = random1.NextDouble() * 100.0 - 50.0;
            double yOff2 = random1.NextDouble() * 100.0 - 50.0;
            double xOff3 = random1.NextDouble() * 500.0 - 250.0;
            double yOff3 = random1.NextDouble() * 500.0 - 250.0;

            double scaleX1 = random2.NextDouble() * 0.04 + 0.01;
            double scaleY1 = random2.NextDouble() * 0.05 + 0.02;
            double scaleX2 = random2.NextDouble() * 0.10 + 0.05;
            double scaleY2 = random2.NextDouble() * 0.10 + 0.05;
            double scaleX3 = random2.NextDouble() * 0.20 + 0.07;
            double scaleY3 = random2.NextDouble() * 0.20 + 0.07;

            for (int x = 0; x < gameWorld.BlockSize.X; x++)
            {
                for (int y = 0; y < gameWorld.BlockSize.Y; y++)
                {
                    double noiseVal = Noise.GetNoise((xOff1 + x) * scaleX1, (yOff1 + y) * scaleY1) * 0.7f +
                            Noise.GetNoise((xOff2 + x) * scaleX2, (yOff2 + y) * scaleY2) * 0.4f +
                            Noise.GetNoise((xOff3 + x) * scaleX3, (yOff3 + y) * scaleY3) * 0.2f;
                    if (noiseVal > 0.1)
                    {
                        gameWorld[x, y, 1] = Block.Empty;
                        gameWorld[x, y, 2] = Block.Empty;
                        gameWorld[x, y, 3] = Block.Empty;
                    }
                    if (noiseVal > 0.2)
                    {
                        gameWorld[x, y, 4] = Block.Empty;
                        gameWorld[x, y, 5] = Block.Empty;
                        gameWorld[x, y, 6] = Block.Empty;
                    }
                    if (noiseVal > 0.3)
                    {
                        gameWorld[x, y, 7] = Block.Empty;
                        gameWorld[x, y, 8] = Block.Empty;
                        gameWorld[x, y, 9] = Block.Empty;
                    }
                    if (noiseVal > 0.4)
                    {
                        gameWorld[x, y, 10] = Block.Empty;
                        gameWorld[x, y, 11] = Block.Empty;
                        gameWorld[x, y, 12] = Block.Empty;
                    }
                    if (noiseVal > 0.5)
                    {
                        gameWorld[x, y, 13] = Block.Empty;
                        gameWorld[x, y, 14] = Block.Empty;
                        gameWorld[x, y, 15] = Block.Empty;
                        gameWorld[x, y, 16] = Block.Empty;
                    }
                }
            }
        }

        private static string GetBlockSet(Block block)
        {
            if (block.Name.Length <= 6)
                return "";

            return block.Name.Substring(0, 6);
        }

        private static void Fill(IGameWorld gameWorld, int x, int y, ref int depth, BlockRandomizer block)
        {
            for (int i = 0; i < 3; i++)
                gameWorld[x, y, depth++] = block.NextBlock();
        }

        private static void Fill(IGameWorld gameWorld, int x, int y, ref int depth, Block block)
        {
            for (int i = 0; i < 3; i++)
                gameWorld[x, y, depth++] = block;
        }

        private static long LongRandom()
        {
            byte[] buf = new byte[8];
            Random random = new Random();
            random.NextBytes(buf);
            return BitConverter.ToInt64(buf, 0);
        }
        #endregion
    }
}
