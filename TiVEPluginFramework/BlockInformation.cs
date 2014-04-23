using System;
using System.Runtime.CompilerServices;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public sealed class BlockInformation
    {
        /// <summary>
        /// Number of voxels that make up a block on each axis
        /// </summary>
        public const int BlockSize = 9;

        public static readonly BlockInformation Empty = new BlockInformation("Empty", new uint[BlockSize, BlockSize, BlockSize]);

        public readonly string BlockName;
        public readonly uint[] Voxels = new uint[BlockSize * BlockSize * BlockSize];
        public readonly ParticleSystemInformation ParticleSystem;

        public BlockInformation(string blockName, uint[,,] voxels, ParticleSystemInformation particleSystem = null)
        {
            if (blockName == null)
                throw new ArgumentNullException("blockName");

            if (voxels == null)
                throw new ArgumentNullException("voxels");

            if (voxels.GetLength(0) != BlockSize || voxels.GetLength(1) != BlockSize || voxels.GetLength(2) != BlockSize)
                throw new ArgumentException("voxels must be a cube of " + BlockSize + " items");

            BlockName = blockName;
            ParticleSystem = particleSystem;
            for (int z = 0; z < BlockSize; z++)
            {
                for (int x = 0; x < BlockSize; x++)
                {
                    for (int y = 0; y < BlockSize; y++)
                        Voxels[GetOffset(x, y, z)] = voxels[x, y, z];
                }
            }
        }

        public uint this[int x, int y, int z]
        {
            get { return Voxels[GetOffset(x, y, z)]; }
            set { Voxels[GetOffset(x, y, z)] = value; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetOffset(int x, int y, int z)
        {
            return x * BlockSize * BlockSize + z * BlockSize + y;
        }
    }
}
