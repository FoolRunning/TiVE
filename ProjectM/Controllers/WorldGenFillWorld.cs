﻿using System;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.ProjectM.Controllers
{
    /// <summary>
    /// World generation to create a random world
    /// </summary>
    public class GenerateRandomWorld : IWorldGenerator
    {

        #region Implementation of IWorldGenerator
        public void UpdateGameWorld(IGameWorld gameWorld, string gameWorldName)
        {
            long seed = LongRandom();
            //AddBiomes(gameWorld, seed);
            FillWorld(gameWorld, seed, null);
            CreateCaves(gameWorld, seed);
        }
        #endregion

        //private static void AddBiomes(IGameWorld gameWorld, long seed)
        //{
        //    Random random = new Random((int)((seed >> 20) & 0xFFFFFFFF));
        //    double offset1 = random.NextDouble() * 100.0 - 50.0;
        //    double offset2 = random.NextDouble() * 40.0 - 20.0;
        //    double offset3 = random.NextDouble() * 6.0 - 3.0;

        //    double scale1 = random.NextDouble() * 0.003 + 0.001;
        //    double scale2 = random.NextDouble() * 0.01 + 0.005;
        //    double scale3 = random.NextDouble() * 0.04 + 0.02;

        //    //Debug.WriteLine(scale1 + ", " + scale2 + ", " + scale3);

        //    for (int x = 0; x < gameWorld.BlockSize.X; x++)
        //    {
        //        double noise = Noise.GetNoise((offset1 + x) * scale1) * 0.6 +
        //            Noise.GetNoise((offset2 + x) * scale2) * 0.25 +
        //            Noise.GetNoise((offset3 + x) * scale3) * 0.15;

        //        int bottomY = gameWorld.BlockSize.Y - (int)(noise * 75.0) - 125;
        //        for (int y = bottomY; y >= 0; y--)
        //            gameWorld.SetBiome(x, y, 1);

        //    }
        //}

        private static void FillWorld(IGameWorld gameWorld, long seed, IBlockList blockList)
        {
            BlockRandomizer backWalls = new BlockRandomizer(blockList, "back", 5);
            BlockRandomizer dirts = new BlockRandomizer(blockList, "dirt", 5);
            BlockRandomizer stones = new BlockRandomizer(blockList, "stone", 5);
            BlockRandomizer sands = new BlockRandomizer(blockList, "sand", 5);
            BlockRandomizer lights = new BlockRandomizer(blockList, "light", 6);
            BlockInformation fire = blockList["fire"];
            BlockInformation fountain = blockList["fountain"];
            BlockInformation snow = blockList["snow"];

            Random random1 = new Random((int)((seed >> 11) & 0xFFFFFFFF));
            Random random2 = new Random((int)((seed >> 15) & 0xFFFFFFFF));

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
                    //if (gameWorld.GetBiome(x, y) == 0)
                    //    continue;
                    double noiseVal = Noise.GetNoise((xOff1 + x) * scaleX1, (yOff1 + y) * scaleY1) *
                            Noise.GetNoise((xOff2 + x) * scaleX2, (yOff2 + y) * scaleY2) +
                            Noise.GetNoise((xOff3 + x) * scaleX3, (yOff3 + y) * scaleY3) * 0.5f;
                    gameWorld[x, y, 0] = backWalls.NextBlock();
                    int depth = 1;
                    if (noiseVal > 0.2)
                    {
                        Fill(gameWorld, x, y, ref depth, dirts);
                        if (noiseVal > 0.4)
                            Fill(gameWorld, x, y, ref depth, dirts);
                        if (noiseVal > 0.6)
                            Fill(gameWorld, x, y, ref depth, dirts);
                        if (noiseVal > 0.8)
                            Fill(gameWorld, x, y, ref depth, dirts);
                        if (noiseVal > 0.85)
                            gameWorld[x, y, depth] = fountain;
                    }
                    else if (noiseVal < -0.3)
                    {
                        Fill(gameWorld, x, y, ref depth, sands);
                        if (noiseVal < -0.4)
                            Fill(gameWorld, x, y, ref depth, sands);
                        if (noiseVal < -0.6)
                            Fill(gameWorld, x, y, ref depth, sands);
                        if (noiseVal < -0.8)
                            Fill(gameWorld, x, y, ref depth, sands);
                        if (noiseVal < -0.85)
                            gameWorld[x, y, depth] = fountain;
                    }
                    else
                    {
                        Fill(gameWorld, x, y, ref depth, stones);
                        if (noiseVal > -0.3 && noiseVal < 0.2)
                            Fill(gameWorld, x, y, ref depth, stones);
                        if (noiseVal > -0.2 && noiseVal < 0.1)
                            Fill(gameWorld, x, y, ref depth, stones);
                        if (noiseVal > -0.1 && noiseVal <= 0.0)
                            Fill(gameWorld, x, y, ref depth, stones);
                        if (noiseVal > -0.05 && noiseVal < -0.04)
                            gameWorld[x, y, depth] = fire;
                    }
                    if (random1.NextDouble() < 0.01)
                        gameWorld[x, y, random1.Next(gameWorld.BlockSize.Z - 3)] = lights.NextBlock();

                    //if (random1.NextDouble() < 0.2)
                        gameWorld[x, y, gameWorld.BlockSize.Z - 1] = snow;
                }
            }
        }

        /// <summary>
        /// Updates the world with random cave formations. Caves are generated using Perlin Simplex noise using the specified seed value to
        /// generate a little randomness.
        /// </summary>
        private static void CreateCaves(IGameWorld gameWorld, long seed)
        {
            Random random1 = new Random((int)(seed & 0xFFFFFFFF));
            Random random2 = new Random((int)((seed >> 32) & 0xFFFFFFFF));

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

            // Use parallel for for speed since there is no syncing needed
            for (int x = 0; x < gameWorld.BlockSize.X; x++)
            {
                for (int y = 0; y < gameWorld.BlockSize.Y; y++)
                {
                    double noiseVal = Noise.GetNoise((xOff1 + x) * scaleX1, (yOff1 + y) * scaleY1) * 0.7f +
                            Noise.GetNoise((xOff2 + x) * scaleX2, (yOff2 + y) * scaleY2) * 0.4f +
                            Noise.GetNoise((xOff3 + x) * scaleX3, (yOff3 + y) * scaleY3) * 0.2f;
                    if (noiseVal > 0.2)
                    {
                        gameWorld[x, y, 1] = null;
                        gameWorld[x, y, 2] = null;
                        gameWorld[x, y, 3] = null;
                    }
                    if (noiseVal > 0.3)
                    {
                        gameWorld[x, y, 4] = null;
                        gameWorld[x, y, 5] = null;
                        gameWorld[x, y, 6] = null;
                    }
                    if (noiseVal > 0.4)
                    {
                        gameWorld[x, y, 7] = null;
                        gameWorld[x, y, 8] = null;
                        gameWorld[x, y, 9] = null;
                    }
                    if (noiseVal > 0.5)
                    {
                        gameWorld[x, y, 10] = null;
                        gameWorld[x, y, 11] = null;
                        gameWorld[x, y, 12] = null;
                    }
                    if (noiseVal > 0.6)
                    {
                        gameWorld[x, y, 13] = null;
                        gameWorld[x, y, 14] = null;
                        //gameWorld[x, y, 14] null);
                        //gameWorld[x, y, 15] null);
                    }
                }
            }
        }

        private static void Fill(IGameWorld gameWorld, int x, int y, ref int depth, BlockRandomizer block)
        {
            for (int i = 0; i < 3; i++)
                gameWorld[x, y, depth++] = block.NextBlock();
        }

        private static long LongRandom()
        {
            byte[] buf = new byte[8];
            Random random = new Random();
            random.NextBytes(buf);
            return BitConverter.ToInt64(buf, 0);
        }

        private sealed class BlockRandomizer
        {
            private readonly BlockInformation[] blocks;
            private readonly int blockCount;
            private readonly Random random = new Random();

            public BlockRandomizer(IBlockList blockList, string blockname, int blockCount)
            {
                this.blockCount = blockCount;
                blocks = new BlockInformation[blockCount];
                for (int i = 0; i < blockCount; i++)
                    blocks[i] = blockList[blockname + i];
            }

            public BlockInformation NextBlock()
            {
                int blockNum;
                lock(random)
                    blockNum = random.Next(blockCount);
                return blocks[blockNum];
            }
        }
    }
}
