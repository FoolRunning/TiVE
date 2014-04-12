using System;

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
        public readonly uint[,,] Voxels;
        public readonly ParticleSystemInformation ParticleSystem;

        public BlockInformation(string blockName, uint[,,] voxels, ParticleSystemInformation particleSystem = null)
        {
            if (blockName == null)
                throw new ArgumentNullException("blockName");

            if (voxels == null)
                throw new ArgumentNullException("voxels");

            BlockName = blockName;
            Voxels = voxels;
            ParticleSystem = particleSystem;
        }
    }
}
