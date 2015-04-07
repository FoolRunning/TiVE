using System;
using System.Collections.Generic;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;
using ProdigalSoftware.TiVEPluginFramework.Generators;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.ProjectM.Plugins
{
    public class MazeBlockLoader : IBlockGenerator
    {
        private static readonly Random random = new Random();

        private const int Front = 1;
        private const int Back = 2;
        private const int Left = 4;
        private const int Right = 8;
        private const int Top = 16;
        private const int Bottom = 32;

        public IEnumerable<Block> CreateBlocks(string blockListName)
        {
            if (blockListName != "maze")
                yield break;

            const byte bc = Block.VoxelSize / 2;
            const int mv = Block.VoxelSize - 1;
            const uint mortarColor = 0xFFAFAFAF;

            Vector3b blockCenterVector = new Vector3b(bc, bc, bc);
            for (int i = 0; i < 64; i++)
            {
                yield return CreateBlockInfo("lava" + i, new Color4f(200, 15, 8, 255), 1.0f, i,
                    new LightComponent(new Vector3b(bc, bc, bc), new Color3f(0.2f, 0.01f, 0.005f), 4));
                
                Block stone = CreateBlockInfo("ston" + i, new Color4f(220, 220, 220, 255), 1.0f, i);
                for (int x = 0; x <= mv; x++)
                {
                    for (int y = 0; y <= mv; y++)
                    {
                        if ((i & Bottom) != 0)
                        {
                            stone[x, 0, 0] = 0;
                            stone[x, 0, 1] = 0;
                            stone[x, 0, mv] = 0;

                            stone[x, 0, bc] = 0;
                            stone[x, 0, bc - 1] = 0;
                            stone[x, 0, bc + 1] = 0;
                        }

                        if ((i & Top) != 0)
                        {
                            stone[x, mv, 0] = 0;
                            stone[x, mv, 1] = 0;
                            stone[x, mv, mv] = 0;

                            stone[x, mv, bc] = 0;
                            stone[x, mv, bc - 1] = 0;
                            stone[x, mv, bc + 1] = 0;
                        }

                        if ((i & Left) != 0)
                        {
                            stone[0, y, 0] = 0;
                            stone[0, y, 1] = 0;
                            stone[0, y, mv] = 0;

                            stone[0, y, bc] = 0;
                            stone[0, y, bc - 1] = 0;
                            stone[0, y, bc + 1] = 0;
                        }

                        if ((i & Right) != 0)
                        {
                            stone[mv, y, 0] = 0;
                            stone[mv, y, 1] = 0;
                            stone[mv, y, mv] = 0;

                            stone[mv, y, bc] = 0;
                            stone[mv, y, bc - 1] = 0;
                            stone[mv, y, bc + 1] = 0;
                        }

                        ReplaceVoxel(stone, x, y, 0, mortarColor);
                        ReplaceVoxel(stone, x, y, bc, mortarColor);
                    }
                }
                for (int n = 0; n <= mv; n++)
                {
                    for (int z = 0; z < bc; z++)
                    {
                        if ((i & Bottom) != 0)
                        {
                            stone[bc - 5, 0, z] = 0;
                            stone[bc - 4, 0, z] = 0;
                            stone[bc - 3, 0, z] = 0;

                            stone[bc + 3, 0, z + bc] = 0;
                            stone[bc + 4, 0, z + bc] = 0;
                            stone[bc + 5, 0, z + bc] = 0;
                        }

                        if ((i & Top) != 0)
                        {
                            stone[bc - 5, mv, z] = 0;
                            stone[bc - 4, mv, z] = 0;
                            stone[bc - 3, mv, z] = 0;

                            stone[bc + 3, mv, z + bc] = 0;
                            stone[bc + 4, mv, z + bc] = 0;
                            stone[bc + 5, mv, z + bc] = 0;
                        }

                        if ((i & Left) != 0)
                        {
                            stone[0, bc - 5, z] = 0;
                            stone[0, bc - 4, z] = 0;
                            stone[0, bc - 3, z] = 0;

                            stone[0, bc + 3, z + bc] = 0;
                            stone[0, bc + 4, z + bc] = 0;
                            stone[0, bc + 5, z + bc] = 0;
                        }

                        if ((i & Right) != 0)
                        {
                            stone[mv, bc - 5, z] = 0;
                            stone[mv, bc - 4, z] = 0;
                            stone[mv, bc - 3, z] = 0;

                            stone[mv, bc + 3, z + bc] = 0;
                            stone[mv, bc + 4, z + bc] = 0;
                            stone[mv, bc + 5, z + bc] = 0;
                        }

                        ReplaceVoxel(stone, bc - 4, n, z, mortarColor);
                        if (z < bc - 1 || (i & Front) == 0)
                            ReplaceVoxel(stone, bc + 4, n, z + bc, mortarColor);

                        ReplaceVoxel(stone, n, bc - 4, z, mortarColor);
                        if (z < bc - 1 || (i & Front) == 0)
                            ReplaceVoxel(stone, n, bc + 4, z + bc, mortarColor);
                    }
                }
                yield return stone;
            }

            for (int i = 0; i < 50; i++)
            {
                Block grass = Factory.CreateBlock("grass" + i);
                grass.AddComponent(new TransparentComponent());

                Color4f grassColor = new Color4f(50, 230, 50, 255);
                for (int z = 0; z < Block.VoxelSize; z++)
                {
                    for (int x = 0; x < Block.VoxelSize; x++)
                    {
                        for (int y = 0; y < Block.VoxelSize; y++)
                        {
                            if (random.NextDouble() < 0.5 - z / 50.0 && (z == 0 || GrassVoxelUnder(grass, x, y, z)))
                            {
                                grass[x, y, z] = CreateColorFromColor(grassColor).ToArgb();
                                if (z == Block.VoxelSize - 1 && random.NextDouble() < 0.2 &&
                                    x > 0 && x < Block.VoxelSize - 1 && y > 0 && y < Block.VoxelSize - 1)
                                {
                                    // Make a flower
                                    Color4f flowerColor = CreateRandomFlowerColor(random);
                                    grass[x, y, z] = CreateColorFromColor(flowerColor).ToArgb();
                                    grass[x - 1, y, z - 1] = CreateColorFromColor(flowerColor).ToArgb();
                                    grass[x + 1, y, z - 1] = CreateColorFromColor(flowerColor).ToArgb();
                                    grass[x, y - 1, z - 1] = CreateColorFromColor(flowerColor).ToArgb();
                                    grass[x, y + 1, z - 1] = CreateColorFromColor(flowerColor).ToArgb();
                                }
                            }
                        }
                    }
                }

                yield return grass;
            }

            for (int i = 0; i < 6; i++)
            {
                yield return CreateBlockInfo("backStone" + i, 0, new Color4f(240, 240, 240, 255), 1.0f, allowLightPassthrough: true);
                yield return CreateBlockInfo("back" + i, 0, new Color4f(0.6f, 0.45f, 0.25f, 1.0f), 1.0f, allowLightPassthrough: true);
            }

            Block fireBlock = Factory.CreateBlock("fire");
            fireBlock.AddComponent(new ParticleComponent("Fire", new Vector3i(bc, bc, 1)));
            fireBlock.AddComponent(new LightComponent(new Vector3b(bc, bc, 4), new Color3f(1.0f, 0.8f, 0.6f), 15));
            yield return fireBlock;

            yield return CreateBlockInfo("roomLight", 5, new Color4f(1.0f, 1.0f, 1.0f, 1.0f), 1.0f, null,
                new LightComponent(blockCenterVector, new Color3f(1.0f, 1.0f, 1.0f), 40));

            const int lightDist = 40;
            ParticleComponent bugInformation = new ParticleComponent("LightBugs", new Vector3i(bc, bc, bc));

            yield return CreateBlockInfo("light0", 2, new Color4f(0.5f, 0.8f, 1.0f, 1.0f), 1.0f, bugInformation,
                new LightComponent(blockCenterVector, new Color3f(0.5f, 0.8f, 1.0f), lightDist));

            yield return CreateBlockInfo("light1", 2, new Color4f(0.8f, 0.5f, 1.0f, 1.0f), 1.0f, bugInformation,
                new LightComponent(blockCenterVector, new Color3f(0.8f, 0.5f, 1.0f), lightDist));

            yield return CreateBlockInfo("light2", 2, new Color4f(1.0f, 0.5f, 0.8f, 1.0f), 1.0f, bugInformation,
                new LightComponent(blockCenterVector, new Color3f(1.0f, 0.5f, 0.8f), lightDist));

            yield return CreateBlockInfo("light3", 2, new Color4f(1.0f, 0.8f, 0.5f, 1.0f), 1.0f, bugInformation,
                new LightComponent(blockCenterVector, new Color3f(1.0f, 0.8f, 0.5f), lightDist));

            yield return CreateBlockInfo("light4", 2, new Color4f(0.5f, 1.0f, 0.8f, 1.0f), 1.0f, bugInformation,
                new LightComponent(blockCenterVector, new Color3f(0.5f, 1.0f, 0.8f), lightDist));

            yield return CreateBlockInfo("light5", 2, new Color4f(0.8f, 1.0f, 0.5f, 1.0f), 1.0f, bugInformation,
                new LightComponent(blockCenterVector, new Color3f(0.8f, 1.0f, 0.5f), lightDist));

            yield return CreateBlockInfo("fountain", bc, new Color4f(20, 20, 150, 255), 1.0f, new ParticleComponent("Fountain", new Vector3i(bc, bc, 13)));
        }


        private static void ReplaceVoxel(Block block, int x, int y, int z, uint newVoxel)
        {
            if (block[x, y, z] != 0)
                block[x, y, z] = newVoxel;
        }

        private static Color4f CreateRandomFlowerColor(Random random)
        {
            switch (random.Next(6))
            {
                case 1: return new Color4f(1.0f, 0.0f, 0.0f, 1.0f);
                case 2: return new Color4f(0.0f, 0.0f, 1.0f, 1.0f);
                case 3: return new Color4f(1.0f, 0.0f, 1.0f, 1.0f);
                case 4: return new Color4f(1.0f, 1.0f, 0.0f, 1.0f);
                case 5: return new Color4f(0.0f, 1.0f, 1.0f, 1.0f);
                default: return new Color4f(1.0f, 1.0f, 1.0f, 1.0f);
            }
        }

        private static bool GrassVoxelUnder(Block block, int x, int y, int z)
        {
            int countUnder = 0;
            if (block[x, y, z - 1] != 0)
                countUnder++;
            if (x > 0 && block[x - 1, y, z - 1] != 0)
                countUnder++;
            if (x < Block.VoxelSize - 1 && block[x + 1, y, z - 1] != 0)
                countUnder++;
            if (y > 0 && block[x, y - 1, z - 1] != 0)
                countUnder++;
            if (y < Block.VoxelSize - 1 && block[x, y + 1, z - 1] != 0)
                countUnder++;
            return countUnder > 0 && countUnder <= 1;
        }

        private static Block CreateBlockInfo(string name, Color4f color, float voxelDensity, int sides, LightComponent light = null)
        {
            const float mid = Block.VoxelSize / 2.0f - 0.5f;
            float sphereSize = Block.VoxelSize / 2.0f;

            Block block = Factory.CreateBlock(name);
            if (light != null)
            {
                block.AddComponent(light);
                block.AddComponent(new UnlitComponent());
            }

            for (int x = 0; x < Block.VoxelSize; x++)
            {
                for (int y = 0; y < Block.VoxelSize; y++)
                {
                    for (int z = 0; z < Block.VoxelSize; z++)
                    {
                        if (((sides & Top) != 0 && (sides & Front) != 0 && y - (int)mid > Block.VoxelSize - z) ||   // rounded Top-Front
                            ((sides & Front) != 0 && (sides & Bottom) != 0 && y + (int)mid < z) ||                             // rounded Front-Bottom
                            ((sides & Bottom) != 0 && (sides & Back) != 0 && y + (int)mid < Block.VoxelSize - z) || // rounded Bottom-Back
                            ((sides & Back) != 0 && (sides & Top) != 0 && y - (int)mid > z))                                   // rounded Back-Top
                        {
                            // Cylinder around the x-axis
                            float dist = (y - mid) * (y - mid) + (z - mid) * (z - mid);
                            if (dist > sphereSize * sphereSize)
                                continue;
                        }

                        if (((sides & Right) != 0 && (sides & Front) != 0 && x - (int)mid > Block.VoxelSize - z) || // rounded Right-Front
                            ((sides & Front) != 0 && (sides & Left) != 0 && x + (int)mid < z) ||                               // rounded Front-Left
                            ((sides & Left) != 0 && (sides & Back) != 0 && x + (int)mid < Block.VoxelSize - z) ||   // rounded Left-Back
                            ((sides & Back) != 0 && (sides & Right) != 0 && x - (int)mid > z))                                 // rounded Back-Right
                        {
                            // Cylinder around the y-axis
                            float dist = (x - mid) * (x - mid) + (z - mid) * (z - mid);
                            if (dist > sphereSize * sphereSize)
                                continue;
                        }

                        if (((sides & Right) != 0 && (sides & Top) != 0 && x - (int)mid > Block.VoxelSize - y) ||   // rounded Right-Top
                            ((sides & Top) != 0 && (sides & Left) != 0 && x + (int)mid < y) ||                                 // rounded Top-Left
                            ((sides & Left) != 0 && (sides & Bottom) != 0 && x + (int)mid < Block.VoxelSize - y) || // rounded Left-Bottom
                            ((sides & Bottom) != 0 && (sides & Right) != 0 && x - (int)mid > y))                               // rounded Bottom-Right
                        {
                            // Cylinder around the z-axis
                            float dist = (x - mid) * (x - mid) + (y - mid) * (y - mid);
                            if (dist > sphereSize * sphereSize)
                                continue;
                        }

                        if ((((sides & Top) != 0 && (sides & Bottom) != 0 && (sides & Left) != 0 && x < mid) || // rounded Left
                            ((sides & Top) != 0 && (sides & Bottom) != 0 && (sides & Right) != 0 && x > mid) || // rounded Right
                            ((sides & Top) != 0 && (sides & Right) != 0 && (sides & Left) != 0 && y > mid) ||   // rounded Top
                            ((sides & Bottom) != 0 && (sides & Right) != 0 && (sides & Left) != 0 && y < mid))  // rounded Bottom
                            && (((sides & Front) != 0 && z > mid) || ((sides & Back) != 0 && z < mid)))         // on the front or back
                        {
                            // rounded front or back
                            float dist = (x - mid) * (x - mid) + (y - mid) * (y - mid) + (z - mid) * (z - mid);
                            if (dist > sphereSize * sphereSize)
                                continue;
                        }

                        if (random.NextDouble() < voxelDensity)
                            block[x, y, z] = CreateColorFromColor(color).ToArgb();
                    }
                }
            }

            return block;
        }

        private static Block CreateBlockInfo(string name, float sphereSize, Color4f color, float voxelDensity,
            ParticleComponent particleSystem = null, LightComponent light = null, int zStart = 0, int zLimit = Block.VoxelSize,
            bool allowLightPassthrough = false)
        {
            const int mid = Block.VoxelSize / 2;

            Block block = Factory.CreateBlock(name);
            if (particleSystem != null)
                block.AddComponent(particleSystem);
            if (light != null)
            {
                block.AddComponent(light);
                block.AddComponent(new UnlitComponent());
            }
            if (allowLightPassthrough || light != null)
                block.AddComponent(new TransparentComponent());

            for (int x = 0; x < Block.VoxelSize; x++)
            {
                for (int y = 0; y < Block.VoxelSize; y++)
                {
                    for (int z = zStart; z < zLimit; z++)
                    {
                        if (sphereSize > 0)
                        {
                            int dist = (x - mid) * (x - mid) + (y - mid) * (y - mid) + (z - mid) * (z - mid);
                            if (dist > sphereSize * sphereSize)
                                continue;
                        }

                        if (random.NextDouble() < voxelDensity)
                            block[x, y, z] = (light == null) ? CreateColorFromColor(color).ToArgb() : ((Color4b)color).ToArgb();
                    }
                }
            }

            return block;
        }

        private static Color4b CreateColorFromColor(Color4f seed)
        {
            float scale = (float)(random.NextDouble() * 0.2 + 0.9);
            return new Color4b(Math.Min(seed.R * scale, 1.0f), Math.Min(seed.G * scale, 1.0f), 
                Math.Min(seed.B * scale, 1.0f), seed.A);
        }
    }
}
