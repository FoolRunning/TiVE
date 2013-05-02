using System;
using System.Drawing;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    /// <summary>
    /// Represents a block of the game world. Each block is made up of a 20x20x20 voxel cube.
    /// See <see cref="GameWorld"/> for how blocks are used.
    /// </summary>
    internal class Block : VoxelGroup
    {
        /// <summary>
        /// Number of voxels that make up a block on each axis
        /// </summary>
        public const int BlockSize = 20;

        public Block(BlockInformation blockInfo) : base(BlockSize, BlockSize, BlockSize)
        {
            for (int x = 0; x < BlockSize; x++)
            {
                for (int y = 0; y < BlockSize; y++)
                {
                    for (int z = 0; z < BlockSize; z++)
                    {
                        //SetVoxel(x, y, z, FromColor(CreateColorFromColor(dirt, random)));
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="Block"/>
        /// </summary>
        public Block(bool fill, bool frontOnly) : base(BlockSize, BlockSize, BlockSize)
        {
            // TODO: Create blocks from files instead of programatically
            if (!fill)
                return;

            Color dirt = frontOnly ? Color.FromArgb(255, 62, 25, 1) : Color.FromArgb(255, 129, 53, 2);
            //Color dirt = Color.FromArgb(200, 2, 53, 129);
            Random random = new Random();
            for (int x = 0; x < BlockSize; x++)
            {
                for (int y = 0; y < BlockSize; y++)
                {
                    for (int z = frontOnly ? BlockSize - 1 : 0; z < BlockSize; z++)
                    {
                        if (frontOnly || z < BlockSize - 2 || random.NextDouble() < 0.3)
                            SetVoxel(x, y, z, FromColor(CreateColorFromColor(dirt, random)));
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
