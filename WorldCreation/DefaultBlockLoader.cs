using System;
using System.Collections.Generic;
using System.Drawing;
using ProdigalSoftware.TiVEPluginFramework;

namespace WorldCreation
{
    public class DefaultBlockLoader : IBlockGenerator
    {
        private static readonly Random random = new Random();
        public IEnumerable<BlockInformation> CreateBlocks()
        {
            for (int i = 0; i < 10; i++)
                yield return CreateBlockInfo(i % 5, i >= 5);
        }

        private static BlockInformation CreateBlockInfo(int idOffset, bool frontOnly)
        {
            uint[, ,] voxels = new uint[BlockInformation.BlockSize, BlockInformation.BlockSize, BlockInformation.BlockSize];
            //Color dirt = Color.FromArgb(200, 2, 53, 129);
            Color dirt = frontOnly ? Color.FromArgb(255, 62, 25, 1) : Color.FromArgb(255, 129, 53, 2);
            for (int x = 0; x < BlockInformation.BlockSize; x++)
            {
                for (int y = 0; y < BlockInformation.BlockSize; y++)
                {
                    for (int z = frontOnly ? BlockInformation.BlockSize - 1 : 0; z < BlockInformation.BlockSize; z++)
                    {
                        //if (frontOnly || random.NextDouble() < 0.5)
                            voxels[x, y, z] = FromColor(CreateColorFromColor(dirt, random));
                    }
                }
            }

            //    //for (int x = 1; x < BlockSize - 1; x++)
            //    //{
            //    //    SetVoxel(x, 0, 0, 0xFF0000FF);
            //    //    SetVoxel(x, 0, BlockSize - 1, 0xFF0000FF);
            //    //    SetVoxel(x, BlockSize - 1, 0, 0xFF00FFFF);
            //    //    SetVoxel(x, BlockSize - 1, BlockSize - 1, 0xFF00FFFF);
            //    //}

            //    //for (int y = 1; y < BlockSize - 1; y++)
            //    //{
            //    //    SetVoxel(0, y, 0, 0xFF00FF00);
            //    //    SetVoxel(0, y, BlockSize - 1, 0xFF00FF00);
            //    //    SetVoxel(BlockSize - 1, y, 0, 0xFFFF0000);
            //    //    SetVoxel(BlockSize - 1, y, BlockSize - 1, 0xFFFF0000);
            //    //}

            //    //for (int z = 1; z < BlockSize - 1; z++)
            //    //{
            //    //    SetVoxel(0, 0, z, 0xFFFF00FF);
            //    //    SetVoxel(BlockSize - 1, 0, z, 0xFFFFFFFF);
            //    //    SetVoxel(0, BlockSize - 1, z, 0xFFFF00FF);
            //    //    SetVoxel(BlockSize - 1, BlockSize - 1, z, 0xFFFFFFFF);
            //    //}

            //    //SetVoxel(0, 0, 0, 0xFFFFFFFF);
            //    //SetVoxel(0, BlockSize - 1, 0, 0xFFFFFFFF);
            //    //SetVoxel(0, 0, BlockSize - 1, 0xFFFFFFFF);
            //    //SetVoxel(0, BlockSize - 1, BlockSize - 1, 0xFFFFFFFF);

            //    //SetVoxel(BlockSize - 1, 0, 0, 0xFFFFFFFF);
            //    //SetVoxel(BlockSize - 1, BlockSize - 1, 0, 0xFFFFFFFF);
            //    //SetVoxel(BlockSize - 1, 0, BlockSize - 1, 0xFFFFFFFF);
            //    //SetVoxel(BlockSize - 1, BlockSize - 1, BlockSize - 1, 0xFFFFFFFF);

            return new BlockInformation((frontOnly ? "back" : "dirt") + idOffset, voxels);
        }

        private static uint FromColor(Color color)
        {
            uint val = color.R;
            val |= (uint)color.G << 8;
            val |= (uint)color.B << 16;
            val |= (uint)color.A << 24;
            return val;
        }

        private static Color CreateColorFromColor(Color seed, Random random)
        {
            double rand = random.NextDouble() + 0.6;
            return Color.FromArgb(seed.A,
                (int)Math.Min(seed.R * rand, 255),
                (int)Math.Min(seed.G * rand, 255),
                (int)Math.Min(seed.B * rand, 255));
        }

    }
}
