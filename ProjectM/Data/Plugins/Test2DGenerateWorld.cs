using JetBrains.Annotations;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Generators;

namespace ProdigalSoftware.ProjectM.Data.Plugins
{
    [UsedImplicitly]
    public class Test2DGenerateWorld : IWorldGenerator
    {
        private static readonly RandomGenerator random = new RandomGenerator();
        
        public IGameWorld CreateGameWorld(string gameWorldName)
        {
            if (gameWorldName != "2DTest")
                return null;

            IGameWorld gameWorld = Factory.NewGameWorld(1000, 15, 1000);
            gameWorld.LightingModelType = LightingModelType.Fantasy3;

            FillWorld(gameWorld);
            gameWorld[500, 0, 500] = Factory.Get<Block>("player");

            CommonUtils.SmoothGameWorldForMazeBlocks(gameWorld, false);

            return gameWorld;
        }

        private static void FillWorld(IGameWorld gameWorld)
        {
            BlockRandomizer grasses = new BlockRandomizer("grass", CommonUtils.grassBlockDuplicates);
            BlockRandomizer unlitGrasses = new BlockRandomizer("loadingGrass", CommonUtils.grassBlockDuplicates);
            BlockRandomizer lights = new BlockRandomizer("light", 7);
            //Block player = Factory.Get<Block>("player");
            //Block dirt = Factory.Get<Block>("dirt");
            //Block unlitDirt = Factory.Get<Block>("unlitBackDirt");
            Block dirt = Factory.Get<Block>("bumpyDirt0_0");
            //Block stone = Factory.Get<Block>("stoneBrick0_0");
            Block stoneBack = Factory.Get<Block>("back0_0");
            //Block wood = Factory.Get<Block>("wood0");
            //Block leaves = Factory.Get<Block>("leaves0_0");
            Block fountain = Factory.Get<Block>("fountain");
            //Block smallLight = Factory.Get<Block>("smallLight");
            Block redLight = Factory.Get<Block>("redLight");
            //Block treeLight = Factory.Get<Block>("treeLight");
            //Block smallLightHover = blockList["smallLightHover"];
            Block fire = Factory.Get<Block>("fire");
            //Block lava = Factory.Get<Block>("lava");

            for (int x = 0; x < gameWorld.BlockSize.X; x++)
            {
                for (int z = 0; z < gameWorld.BlockSize.Z; z++)
                {
                    //gameWorld[x, 15, z] = unlitDirt;
                    for (int y = 2; y < 8; y++)
                    {
                        int materialNoise = (int)((Noise.GetNoise(197 + x * 0.1, -13 + y * 0.25, -497 + z * 0.11) + 1) * 500); // noise 0..1000
                        gameWorld[x, y, z] = materialNoise < 500 ? stoneBack : dirt;
                    }
                }
            }

            for (int x = 0; x < gameWorld.BlockSize.X; x++)
            {
                for (int z = 0; z < gameWorld.BlockSize.Z; z++)
                {
                    int caveNoise = (int)((Noise.GetNoise(x * 0.13, z * 0.12) + 1) * 500); // noise 0..1000
                    if (caveNoise > 500)
                    {
                        for (int y = 2; y < 5; y++)
                            gameWorld[x, y, z] = Block.Empty;
                    }
                }
            }

            for (int x = 0; x < gameWorld.BlockSize.X; x++)
            {
                for (int z = 1; z < gameWorld.BlockSize.Z; z++)
                {
                    int grassNoise = (int)((Noise.GetNoise(-237 + x * 0.15, 37 + z * 0.22) + 1) * 500);
                    //if (grassNoise < 100)
                    //    gameWorld[x, 0, z] = fountain;
                    //else if (grassNoise < 500)
                    //    gameWorld[x, 0, z] = grasses.NextBlock();

                    if (gameWorld[x, 3, z] == Block.Empty && gameWorld[x, 3, z - 1] == dirt)
                    {
                        for (int y = 2; y < 5; y++)
                        {
                            if (y == 3 && grassNoise < 60)
                                gameWorld[x, y, z] = fire;
                            else if (y == 3 && grassNoise < 100)
                                gameWorld[x, y, z] = fountain;
                            else if (grassNoise < 500)
                                gameWorld[x, y, z] = grasses.NextBlock();
                            else
                                gameWorld[x, y, z] = Block.Empty;
                        }
                    }
                }
            }

            for (int x = 0; x < gameWorld.BlockSize.X; x++)
            {
                for (int z = 0; z < gameWorld.BlockSize.Z; z++)
                {
                    int lightNoise = (int)((Noise.GetNoise(95 + x * 3.7, 197 + z * 3.5) + 1) * 500);
                    if (lightNoise > 925)
                    {
                        int lightNum = (lightNoise - 925) / 11;
                        if (gameWorld[x, 2, z] == Block.Empty)
                            gameWorld[x, 2, z] = Factory.Get<Block>("light" + lightNum);
                        //else
                        //    gameWorld[x, 0, z] = redLight;
                    }
                }
            }
        }

        //private static void FillWorldOld(IGameWorld gameWorld)
        //{
        //    BlockRandomizer grasses = new BlockRandomizer("grass", CommonUtils.grassBlockDuplicates);
        //    BlockRandomizer unlitGrasses = new BlockRandomizer("loadingGrass", CommonUtils.grassBlockDuplicates);
        //    BlockRandomizer lights = new BlockRandomizer("light", 7);
        //    //Block player = Factory.Get<Block>("player");
        //    //Block dirt = Factory.Get<Block>("dirt");
        //    Block unlitDirt = Factory.Get<Block>("unlitBackDirt");
        //    Block dirt = Factory.Get<Block>("bumpyDirt0_0");
        //    Block stone = Factory.Get<Block>("stoneBrick0_0");
        //    Block stoneBack = Factory.Get<Block>("back0_0");
        //    Block wood = Factory.Get<Block>("wood0");
        //    Block leaves = Factory.Get<Block>("leaves0_0");
        //    Block fountain = Factory.Get<Block>("fountain");
        //    //Block smallLight = Factory.Get<Block>("smallLight");
        //    //Block redLight = Factory.Get<Block>("redLight");
        //    Block treeLight = Factory.Get<Block>("treeLight");
        //    //Block smallLightHover = blockList["smallLightHover"];
        //    //Block fire = Factory.Get<Block>("fire");
        //    //Block lava = Factory.Get<Block>("lava");

        //    for (int x = 0; x < gameWorld.BlockSize.X; x++)
        //    {
        //        for (int z = 0; z < gameWorld.BlockSize.Z; z++)
        //        {
        //            //gameWorld[x, 15, z] = unlitDirt;
        //            gameWorld[x, 7, z] = dirt;
        //            gameWorld[x, 6, z] = dirt;
        //            gameWorld[x, 5, z] = dirt;
        //            gameWorld[x, 4, z] = dirt;
        //            gameWorld[x, 3, z] = dirt;
        //            gameWorld[x, 2, z] = dirt;
        //            //gameWorld[x, 1, z] = dirt;
        //        }
        //    }

        //    for (int i = 0; i < 150000; i++)
        //    {
        //        int x = random.Next(gameWorld.BlockSize.X - 7) + 1;
        //        int y = random.Next(5) + 2;
        //        int z = random.Next(gameWorld.BlockSize.Z - 7) + 1;
        //        for (int xOffset = 0; xOffset < 2; xOffset++)
        //        {
        //            int wx = x + xOffset;
        //            for (int zOffset = 0; zOffset < 2; zOffset++)
        //            {
        //                int wz = z + zOffset;
        //                for (int yOffset = 0; yOffset < 2; yOffset++)
        //                    gameWorld[wx, y + yOffset, wz] = stoneBack;
        //            }
        //        }
        //    }

        //    for (int i = 0; i < 150000; i++)
        //    {
        //        int x = random.Next(gameWorld.BlockSize.X - 6) + 3;
        //        int z = random.Next(gameWorld.BlockSize.Z - 6) + 3;
        //        for (int xOffset = -1; xOffset < 1; xOffset++)
        //        {
        //            int wx = x + xOffset;
        //            for (int zOffset = -1; zOffset < 2; zOffset++)
        //            {
        //                int wz = z + zOffset;
        //                if (zOffset == -1)
        //                {
        //                    gameWorld[wx, 4, wz] = grasses.NextBlock();
        //                    gameWorld[wx, 3, wz] = (random.Next(100) < 2) ? fountain : grasses.NextBlock();
        //                    gameWorld[wx, 2, wz] = grasses.NextBlock();
        //                    //gameWorld[x + xOffset, 1, z + zOffset] = grasses.NextBlock();
        //                }
        //                else
        //                {
        //                    gameWorld[wx, 4, wz] = Block.Empty;
        //                    gameWorld[wx, 3, wz] = Block.Empty;
        //                    gameWorld[wx, 2, wz] = Block.Empty;
        //                    //gameWorld[x + xOffset, 1, z + zOffset] = Block.Empty;
        //                }

        //                if (random.Next(100) < 20)
        //                {
        //                    gameWorld[x, 5, z] = Block.Empty;
        //                    if (random.Next(100) < 10)
        //                        gameWorld[x, 6, z] = Block.Empty;
        //                }
        //            }
        //        }
        //    }

        //    HashSet<Block> blocksOnStone = new HashSet<Block>();
        //    blocksOnStone.Add(fountain);
        //    for (int i = 0; i < CommonUtils.grassBlockDuplicates; i++)
        //    {
        //        blocksOnStone.Add(Factory.Get<Block>("grass" + i));
        //        blocksOnStone.Add(Factory.Get<Block>("loadingGrass" + i));
        //    }
        //    for (int x = 1; x < gameWorld.BlockSize.X - 1; x++)
        //    {
        //        for (int z = 1; z < gameWorld.BlockSize.Z; z++)
        //        {
        //            for (int y = 2; y < 5; y++)
        //            {
        //                if (gameWorld[x, y, z - 1] != dirt && blocksOnStone.Contains(gameWorld[x, y, z]))
        //                    gameWorld[x, y, z] = Block.Empty;
        //            }
        //        }
        //    }

        //    for (int x = 0; x < gameWorld.BlockSize.X; x++)
        //    {
        //        for (int z = 0; z < gameWorld.BlockSize.Z; z++)
        //        {
        //            if (gameWorld[x, 2, z] == Block.Empty)
        //            {
        //                if (random.Next(100) < 2)
        //                    gameWorld[x, 2, z] = lights.NextBlock();
        //            }
        //        }
        //    }
        //}
    }
}
