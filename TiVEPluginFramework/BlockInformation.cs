using System;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public sealed class BlockInformation
    {
        /// <summary>
        /// Number of voxels that make up a block on each axis
        /// </summary>
        public const int BlockSize = 16;

        public BlockInformation(string blockName, uint[,,] voxels)
        {
            if (blockName == null)
                throw new ArgumentNullException("blockName");

            if (voxels == null)
                throw new ArgumentNullException("voxels");

            BlockName = blockName;
            Voxels = voxels;
        }

        public string BlockName { get; private set; }

        public uint[, ,] Voxels { get; private set; }
    }
}
