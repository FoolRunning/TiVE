using System;
using System.Drawing;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    /// <summary>
    /// Represents a block of the game world. Each block is made up of a 20x20x20 voxel cube.
    /// See <see cref="GameWorld"/> for how blocks are used.
    /// </summary>
    public class Block : VoxelGroup
    {
        /// <summary>
        /// Number of voxels that make up a block on each axis
        /// </summary>
        public const int Block_Size = 20;

        /// <summary>
        /// Creates a new <see cref="Block"/>
        /// </summary>
        public Block(bool fill, bool frontOnly) : base(Block_Size, Block_Size, Block_Size)
        {
            // TODO: Create blocks from files instead of programatically
            if (!fill)
                return;

            Color dirt = frontOnly ? Color.FromArgb(255, 62, 25, 1) : Color.FromArgb(255, 129, 53, 2);
            //Color dirt = Color.FromArgb(200, 2, 53, 129);
            Random random = new Random();
            for (int x = 0; x < Block_Size; x++)
            {
                for (int y = 0; y < Block_Size; y++)
                {
                    for (int z = frontOnly ? Block_Size - 1 : 0; z < Block_Size; z++)
                    {
                        if (frontOnly || z < Block_Size - 3 || random.NextDouble() < 0.3)
                            SetVoxel(x, y, z, FromColor(CreateColorFromColor(dirt, random)));
                    }
                }
            }

            //for (int x = 1; x < Block_Size - 1; x++)
            //{
            //    SetVoxel(x, 0, 0, 0xFF0000FF);
            //    SetVoxel(x, 0, Block_Size - 1, 0xFF0000FF);
            //    SetVoxel(x, Block_Size - 1, 0, 0xFF00FFFF);
            //    SetVoxel(x, Block_Size - 1, Block_Size - 1, 0xFF00FFFF);
            //}

            //for (int y = 1; y < Block_Size - 1; y++)
            //{
            //    SetVoxel(0, y, 0, 0xFF00FF00);
            //    SetVoxel(0, y, Block_Size - 1, 0xFF00FF00);
            //    SetVoxel(Block_Size - 1, y, 0, 0xFFFF0000);
            //    SetVoxel(Block_Size - 1, y, Block_Size - 1, 0xFFFF0000);
            //}

            //for (int z = 1; z < Block_Size - 1; z++)
            //{
            //    SetVoxel(0, 0, z, 0xFFFF00FF);
            //    SetVoxel(Block_Size - 1, 0, z, 0xFFFFFFFF);
            //    SetVoxel(0, Block_Size - 1, z, 0xFFFF00FF);
            //    SetVoxel(Block_Size - 1, Block_Size - 1, z, 0xFFFFFFFF);
            //}

            //SetVoxel(0, 0, 0, 0xFFFFFFFF);
            //SetVoxel(0, Block_Size - 1, 0, 0xFFFFFFFF);
            //SetVoxel(0, 0, Block_Size - 1, 0xFFFFFFFF);
            //SetVoxel(0, Block_Size - 1, Block_Size - 1, 0xFFFFFFFF);

            //SetVoxel(Block_Size - 1, 0, 0, 0xFFFFFFFF);
            //SetVoxel(Block_Size - 1, Block_Size - 1, 0, 0xFFFFFFFF);
            //SetVoxel(Block_Size - 1, 0, Block_Size - 1, 0xFFFFFFFF);
            //SetVoxel(Block_Size - 1, Block_Size - 1, Block_Size - 1, 0xFFFFFFFF);
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
