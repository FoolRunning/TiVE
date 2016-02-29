﻿using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;
using ProdigalSoftware.TiVEPluginFramework.Generators;

namespace ProdigalSoftware.ProjectM.Plugins
{
    [UsedImplicitly]
    public class MazeBlockLoader : IBlockGenerator
    {
        private static readonly Random random = new Random();
        private static readonly Voxel mortarColor = new Voxel(170, 170, 170);

        private const int Front = 1;
        private const int Back = 2;
        private const int Left = 4;
        private const int Right = 8;
        private const int Top = 16;
        private const int Bottom = 32;

        public IEnumerable<Block> CreateBlocks()
        {
            const byte bc = Block.VoxelSize / 2;
            const int mv = Block.VoxelSize - 1;

            Vector3b blockCenterVector = new Vector3b(bc, bc, bc);
            for (int i = 0; i < 64; i++)
                yield return CreateRoundedBlockInfo("wood" + i, new Voxel(213, 128, 43), 1.0f, i);

            for (int i = 0; i < 64; i++)
            {
                yield return CreateRoundedBlockInfo("lava" + i, new Voxel(255, 255, 255, 255, VoxelSettings.AllowLightPassthrough | VoxelSettings.SkipVoxelNormalCalc), 1.0f, i,
                    new LightComponent(new Vector3b(bc, bc, bc), new Color3f(0.4f, 0.02f, 0.01f), 4), 0.1f);
            }

            for (int i = 0; i < 64; i++)
            {
                Block stone = CreateRoundedBlockInfo("ston" + i, new Voxel(240, 240, 240), 1.0f, i, null, 0.3f);
                for (int x = 0; x <= mv; x++)
                {
                    for (int y = 0; y <= mv; y++)
                    {
                        if ((i & Bottom) != 0)
                        {
                            stone[x, 0, 0] = Voxel.Empty;
                            stone[x, 0, 1] = Voxel.Empty;
                            stone[x, 0, mv] = Voxel.Empty;

                            stone[x, 0, bc] = Voxel.Empty;
                            stone[x, 0, bc - 1] = Voxel.Empty;
                            stone[x, 0, bc + 1] = Voxel.Empty;
                        }

                        if ((i & Top) != 0)
                        {
                            stone[x, mv, 0] = Voxel.Empty;
                            stone[x, mv, 1] = Voxel.Empty;
                            stone[x, mv, mv] = Voxel.Empty;

                            stone[x, mv, bc] = Voxel.Empty;
                            stone[x, mv, bc - 1] = Voxel.Empty;
                            stone[x, mv, bc + 1] = Voxel.Empty;
                        }

                        if ((i & Left) != 0)
                        {
                            stone[0, y, 0] = Voxel.Empty;
                            stone[0, y, 1] = Voxel.Empty;
                            stone[0, y, mv] = Voxel.Empty;

                            stone[0, y, bc] = Voxel.Empty;
                            stone[0, y, bc - 1] = Voxel.Empty;
                            stone[0, y, bc + 1] = Voxel.Empty;
                        }

                        if ((i & Right) != 0)
                        {
                            stone[mv, y, 0] = Voxel.Empty;
                            stone[mv, y, 1] = Voxel.Empty;
                            stone[mv, y, mv] = Voxel.Empty;

                            stone[mv, y, bc] = Voxel.Empty;
                            stone[mv, y, bc - 1] = Voxel.Empty;
                            stone[mv, y, bc + 1] = Voxel.Empty;
                        }

                        ReplaceVoxel(stone, x, y, 0, mortarColor);
                        ReplaceVoxel(stone, x, y, bc, mortarColor);
                    }
                }
                for (int z = 0; z < bc; z++)
                {
                    if ((i & Bottom) != 0)
                    {
                        stone[bc - 5, 0, z] = Voxel.Empty;
                        stone[bc - 4, 0, z] = Voxel.Empty;
                        stone[bc - 3, 0, z] = Voxel.Empty;

                        stone[bc + 3, 0, z + bc] = Voxel.Empty;
                        stone[bc + 4, 0, z + bc] = Voxel.Empty;
                        stone[bc + 5, 0, z + bc] = Voxel.Empty;
                    }

                    if ((i & Top) != 0)
                    {
                        stone[bc - 5, mv, z] = Voxel.Empty;
                        stone[bc - 4, mv, z] = Voxel.Empty;
                        stone[bc - 3, mv, z] = Voxel.Empty;

                        stone[bc + 3, mv, z + bc] = Voxel.Empty;
                        stone[bc + 4, mv, z + bc] = Voxel.Empty;
                        stone[bc + 5, mv, z + bc] = Voxel.Empty;
                    }

                    if ((i & Left) != 0)
                    {
                        stone[0, bc - 5, z] = Voxel.Empty;
                        stone[0, bc - 4, z] = Voxel.Empty;
                        stone[0, bc - 3, z] = Voxel.Empty;

                        stone[0, bc + 3, z + bc] = Voxel.Empty;
                        stone[0, bc + 4, z + bc] = Voxel.Empty;
                        stone[0, bc + 5, z + bc] = Voxel.Empty;
                    }

                    if ((i & Right) != 0)
                    {
                        stone[mv, bc - 5, z] = Voxel.Empty;
                        stone[mv, bc - 4, z] = Voxel.Empty;
                        stone[mv, bc - 3, z] = Voxel.Empty;

                        stone[mv, bc + 3, z + bc] = Voxel.Empty;
                        stone[mv, bc + 4, z + bc] = Voxel.Empty;
                        stone[mv, bc + 5, z + bc] = Voxel.Empty;
                    }

                    for (int n = 0; n <= mv; n++)
                    {
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
                Block grass = new Block("grass" + i);
                grass.AddComponent(new VoxelNoiseComponent(0.2f));
                grass.AddComponent(new LightPassthroughComponent());

                for (int z = 0; z < Block.VoxelSize; z++)
                {
                    for (int x = 0; x < Block.VoxelSize; x++)
                    {
                        for (int y = 0; y < Block.VoxelSize; y++)
                        {
                            if (random.NextDouble() < 0.5 - z / 50.0 && (z == 0 || GrassVoxelUnder(grass, x, y, z)))
                            {
                                grass[x, y, z] = new Voxel(70, 240, 50, 255, VoxelSettings.SkipVoxelNormalCalc);
                                if (z == Block.VoxelSize - 1 && random.NextDouble() < 0.2 &&
                                    x > 0 && x < Block.VoxelSize - 1 && y > 0 && y < Block.VoxelSize - 1)
                                {
                                    // Make a flower
                                    Voxel flowerVoxel = CreateRandomFlowerVoxel();
                                    grass[x, y, z] = flowerVoxel;
                                    grass[x - 1, y, z - 1] = flowerVoxel;
                                    grass[x + 1, y, z - 1] = flowerVoxel;
                                    grass[x, y - 1, z - 1] = flowerVoxel;
                                    grass[x, y + 1, z - 1] = flowerVoxel;
                                }
                            }
                        }
                    }
                }

                yield return grass;
            }

            const bool forFantasy = true;

            yield return CreateBlockInfo("backStone", 0, new Color4f(240, 240, 240, 255), 1.0f, colorVariation: 0.3f);
            yield return CreateBlockInfo("dirt", 0, new Color4f(0.6f, 0.45f, 0.25f, 1.0f), 1.0f, colorVariation: 0.4f);

            Block fireBlock = new Block("fire");
            fireBlock.AddComponent(new ParticleComponent("Fire", new Vector3i(bc, bc, 1)));
            fireBlock.AddComponent(new LightComponent(new Vector3b(bc, bc, 4), new Color3f(forFantasy ? 0.7f : 1.0f, forFantasy ? 0.55f : 0.8f, forFantasy ? 0.4f : 0.6f), forFantasy ? 7 : 15));
            yield return fireBlock;

            yield return CreateBlockInfo("loadingLight", 3, new Color4f(1.0f, 1.0f, 1.0f, 1.0f), 1.0f, null,
                new LightComponent(blockCenterVector, new Color3f(0.6f, 0.6f, 0.6f), 35));

            yield return CreateBlockInfo("roomLight", 5, new Color4f(1.0f, 1.0f, 1.0f, 1.0f), 1.0f, null,
                new LightComponent(blockCenterVector, new Color3f(forFantasy ? 0.5f : 0.8f, forFantasy ? 0.5f : 0.8f, forFantasy ? 0.5f : 0.8f), forFantasy ? 25 : 50));

            const int lightDist = forFantasy ? 15 : 45;
            const float bright = forFantasy ? 0.6f : 1.0f;
            const float mid = forFantasy ? 0.45f : 0.5f;
            const float dim = forFantasy ? 0.3f : 0.3f;
            ParticleComponent bugInformation = new ParticleComponent("LightBugs", new Vector3i(bc, bc, bc));

            yield return CreateBlockInfo("light0", 2, new Color4f(1.0f, 1.0f, 1.0f, 1.0f), 1.0f, bugInformation,
                new LightComponent(blockCenterVector, new Color3f(dim, mid, bright), lightDist));

            yield return CreateBlockInfo("light1", 2, new Color4f(1.0f, 1.0f, 1.0f, 1.0f), 1.0f, bugInformation,
                new LightComponent(blockCenterVector, new Color3f(mid, dim, bright), lightDist));

            yield return CreateBlockInfo("light2", 2, new Color4f(1.0f, 1.0f, 1.0f, 1.0f), 1.0f, bugInformation,
                new LightComponent(blockCenterVector, new Color3f(bright, dim, mid), lightDist));

            yield return CreateBlockInfo("light3", 2, new Color4f(1.0f, 1.0f, 1.0f, 1.0f), 1.0f, bugInformation,
                new LightComponent(blockCenterVector, new Color3f(bright, mid, dim), lightDist));

            yield return CreateBlockInfo("light4", 2, new Color4f(1.0f, 1.0f, 1.0f, 1.0f), 1.0f, bugInformation,
                new LightComponent(blockCenterVector, new Color3f(dim, bright, mid), lightDist));

            yield return CreateBlockInfo("light5", 2, new Color4f(1.0f, 1.0f, 1.0f, 1.0f), 1.0f, bugInformation,
                new LightComponent(blockCenterVector, new Color3f(mid, bright, dim), lightDist));

            yield return CreateBlockInfo("smallLight", 1, new Color4f(1.0f, 1.0f, 1.0f, 1.0f), 1.0f, null,
                new LightComponent(new Vector3b(bc, bc, bc), new Color3f(forFantasy ? 0.7f : 1.0f, forFantasy ? 0.7f : 1.0f, forFantasy ? 0.7f : 1.0f), forFantasy ? 1 : 3));

            yield return CreateBlockInfo("smallLightHover", 1, new Color4f(1.0f, 1.0f, 1.0f, 1.0f), 1.0f, null,
                new LightComponent(new Vector3b(bc, bc, bc), new Color3f(1.0f, 1.0f, 1.0f), 3));

            Block fountainBlock = new Block("fountain");
            fountainBlock.AddComponent(new ParticleComponent("Fountain", new Vector3i(bc, bc, 0)));
            yield return fountainBlock;
        }

        private static void ReplaceVoxel(Block block, int x, int y, int z, Voxel newVoxel)
        {
            if (block[x, y, z] != Voxel.Empty)
                block[x, y, z] = newVoxel;
        }

        private static Voxel CreateRandomFlowerVoxel()
        {
            switch (random.Next(6))
            {
                case 1: return new Voxel(255, 0, 0, 255, VoxelSettings.SkipVoxelNormalCalc);
                case 2: return new Voxel(0, 0, 255, 255, VoxelSettings.SkipVoxelNormalCalc);
                case 3: return new Voxel(255, 0, 255, 255, VoxelSettings.SkipVoxelNormalCalc);
                case 4: return new Voxel(255, 255, 0, 255, VoxelSettings.SkipVoxelNormalCalc);
                case 5: return new Voxel(0, 255, 255, 255, VoxelSettings.SkipVoxelNormalCalc);
                default: return new Voxel(255, 255, 255, 255, VoxelSettings.SkipVoxelNormalCalc);
            }
        }

        private static bool GrassVoxelUnder(Block block, int x, int y, int z)
        {
            int countUnder = 0;
            if (block[x, y, z - 1] != Voxel.Empty)
                countUnder++;
            if (x > 0 && block[x - 1, y, z - 1] != Voxel.Empty)
                countUnder++;
            if (x < Block.VoxelSize - 1 && block[x + 1, y, z - 1] != Voxel.Empty)
                countUnder++;
            if (y > 0 && block[x, y - 1, z - 1] != Voxel.Empty)
                countUnder++;
            if (y < Block.VoxelSize - 1 && block[x, y + 1, z - 1] != Voxel.Empty)
                countUnder++;
            return countUnder == 1;
        }

        private static Block CreateRoundedBlockInfo(string name, Voxel voxel, float voxelDensity, int sides, LightComponent light = null, float colorVariation = 0.2f)
        {
            const float mid = Block.VoxelSize / 2.0f - 0.5f;
            const float sphereSize = Block.VoxelSize / 2.0f;

            Block block = new Block(name);
            if (colorVariation > 0.0f)
                block.AddComponent(new VoxelNoiseComponent(colorVariation, mortarColor));

            if (light != null)
            {
                block.AddComponent(light);
                block.AddComponent(new LightPassthroughComponent());
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
                            block[x, y, z] = voxel;
                    }
                }
            }

            return block;
        }

        private static Block CreateBlockInfo(string name, float sphereRadius, Color4f color, float voxelDensity,
            ParticleComponent particleSystem = null, LightComponent light = null, int zStart = 0, int zLimit = Block.VoxelSize, 
            float colorVariation = 0.2f)
        {
            const int mid = Block.VoxelSize / 2;

            Block block = new Block(name);
            VoxelSettings settings = VoxelSettings.None;
            if (particleSystem != null)
                block.AddComponent(particleSystem);
            if (light == null)
                block.AddComponent(new VoxelNoiseComponent(colorVariation));
            else
            {
                block.AddComponent(light);
                block.AddComponent(new LightPassthroughComponent());
                settings = VoxelSettings.AllowLightPassthrough | VoxelSettings.SkipVoxelNormalCalc;
            }

            for (int x = 0; x < Block.VoxelSize; x++)
            {
                for (int y = 0; y < Block.VoxelSize; y++)
                {
                    for (int z = zStart; z < zLimit; z++)
                    {
                        if (sphereRadius > 0)
                        {
                            int dist = (x - mid) * (x - mid) + (y - mid) * (y - mid) + (z - mid) * (z - mid);
                            if (dist > sphereRadius * sphereRadius)
                                continue;
                        }

                        if (random.NextDouble() < voxelDensity)
                            block[x, y, z] = new Voxel(color.R, color.G, color.B, color.A, settings);
                    }
                }
            }

            return block;
        }
    }
}
