using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ProdigalSoftware.TiVEPluginFramework.Lighting;
using ProdigalSoftware.TiVEPluginFramework.Particles;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public sealed class BlockInformation
    {
        /// <summary>Number of voxels that make up a block on each axis</summary>
        public const int BlockSize = 9;

        public static readonly BlockInformation Empty = new BlockInformation("Empty");

        public readonly string BlockName;
        public readonly ParticleSystemInformation ParticleSystem;
        public readonly ILight Light;

        private readonly uint[] voxels = new uint[BlockSize * BlockSize * BlockSize];

        public BlockInformation(string blockName, ParticleSystemInformation particleSystem = null, ILight light = null)
        {
            if (blockName == null)
                throw new ArgumentNullException("blockName");

            BlockName = blockName;
            ParticleSystem = particleSystem;
            Light = light;
        }

        /// <summary>
        /// Gets/sets the voxel at the specified location
        /// </summary>
        public uint this[int x, int y, int z]
        {
            get { return voxels[GetOffset(x, y, z)]; }
            set { voxels[GetOffset(x, y, z)] = value; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetOffset(int x, int y, int z)
        {
            Debug.Assert(x >= 0 && x < BlockSize);
            Debug.Assert(y >= 0 && y < BlockSize);
            Debug.Assert(z >= 0 && z < BlockSize);

            return (z * BlockSize + x) * BlockSize + y; // y-axis major for speed
        }
    }
}
