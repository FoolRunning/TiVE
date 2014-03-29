using System;
using System.Collections.Generic;
using OpenTK.Graphics;
using ProdigalSoftware.TiVEPluginFramework;

namespace WorldCreation
{
    public class DefaultBlockLoader : IBlockGenerator
    {
        private static readonly Random random = new Random();

        public IEnumerable<BlockInformation> CreateBlocks()
        {
            for (int i = 0; i < 20; i++)
            {
                Color4 color;
                string name;
                float voxelDensity;
                if (i < 5)
                {
                    color = new Color4(129, 53, 2, 255);
                    voxelDensity = 0.6f;
                    name = "dirt" + i;
                }
                else if (i < 10)
                {
                    color = new Color4(62, 25, 1, 255);
                    voxelDensity = 1.0f;
                    name = "back" + (i - 5);
                }
                else if (i < 15)
                {
                    color = new Color4(120, 120, 120, 255);
                    voxelDensity = 1.0f;
                    name = "stone" + (i - 10);
                }
                else
                {
                    color = new Color4(140, 100, 20, 255);
                    voxelDensity = 0.15f;
                    name = "sand" + (i - 15);
                }
                uint[, ,] voxels = CreateBlockInfo(i >= 5 && i < 10, color, voxelDensity);
                yield return new BlockInformation(name, voxels);
            }
        }

        private static uint[, ,] CreateBlockInfo(bool frontOnly, Color4 color, float voxelDensity)
        {
            uint[, ,] voxels = new uint[BlockInformation.BlockSize, BlockInformation.BlockSize, BlockInformation.BlockSize];
            for (int x = 0; x < BlockInformation.BlockSize; x++)
            {
                for (int y = 0; y < BlockInformation.BlockSize; y++)
                {
                    for (int z = frontOnly ? BlockInformation.BlockSize - 1 : 0; z < BlockInformation.BlockSize; z++)
                    //for (int z = 0; z < (frontOnly ? 1 : BlockInformation.BlockSize); z++)
                    {
                        //int dist = (x - 4) * (x - 4) + (y - 4) * (y - 4) + (z - 4) * (z - 4);
                        //if (dist > 25)
                        //    continue;

                        if (random.NextDouble() < voxelDensity)
                            voxels[x, y, z] = FromColor(CreateColorFromColor(color));
                    }
                }
            }

            //for (int x = 1; x < BlockSize - 1; x++)
            //{
            //    SetVoxel(x, 0, 0, 0xFF0000FF);
            //    SetVoxel(x, 0, BlockSize - 1, 0xFF0000FF);
            //    SetVoxel(x, BlockSize - 1, 0, 0xFF00FFFF);
            //    SetVoxel(x, BlockSize - 1, BlockSize - 1, 0xFF00FFFF);
            //}

            //for (int y = 1; y < BlockSize - 1; y++)
            //{
            //    SetVoxel(0, y, 0, 0xFF00FF00);
            //    SetVoxel(0, y, BlockSize - 1, 0xFF00FF00);
            //    SetVoxel(BlockSize - 1, y, 0, 0xFFFF0000);
            //    SetVoxel(BlockSize - 1, y, BlockSize - 1, 0xFFFF0000);
            //}

            //for (int z = 1; z < BlockSize - 1; z++)
            //{
            //    SetVoxel(0, 0, z, 0xFFFF00FF);
            //    SetVoxel(BlockSize - 1, 0, z, 0xFFFFFFFF);
            //    SetVoxel(0, BlockSize - 1, z, 0xFFFF00FF);
            //    SetVoxel(BlockSize - 1, BlockSize - 1, z, 0xFFFFFFFF);
            //}

            //SetVoxel(0, 0, 0, 0xFFFFFFFF);
            //SetVoxel(0, BlockSize - 1, 0, 0xFFFFFFFF);
            //SetVoxel(0, 0, BlockSize - 1, 0xFFFFFFFF);
            //SetVoxel(0, BlockSize - 1, BlockSize - 1, 0xFFFFFFFF);

            //SetVoxel(BlockSize - 1, 0, 0, 0xFFFFFFFF);
            //SetVoxel(BlockSize - 1, BlockSize - 1, 0, 0xFFFFFFFF);
            //SetVoxel(BlockSize - 1, 0, BlockSize - 1, 0xFFFFFFFF);
            //SetVoxel(BlockSize - 1, BlockSize - 1, BlockSize - 1, 0xFFFFFFFF);

            return voxels;
        }

        private static uint FromColor(Color4 color)
        {
            return (uint)color.ToArgb();
        }

        private static Color4 CreateColorFromColor(Color4 seed)
        {
            float scale = (float)(random.NextDouble() * 0.5 + 0.75);
            return new Color4(Math.Min(seed.R * scale, 1.0f), Math.Min(seed.G * scale, 1.0f), 
                Math.Min(seed.B * scale, 1.0f), seed.A);
        }
    }
}
