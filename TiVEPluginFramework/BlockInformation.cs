using System;

namespace ProdigalSoftware.TiVEPluginFramework
{
    [Flags]
    public enum VoxelSides
    {
        None = 0,
        Top = 1 << 0,
        Left = 1 << 1,
        Right = 1 << 2,
        Bottom = 1 << 3,
        Front = 1 << 4,
        Back = 1 << 5,
        All = Top | Left | Right | Bottom | Front | Back,
    }

    public sealed class BlockInformation
    {
        /// <summary>
        /// Number of voxels that make up a block on each axis
        /// </summary>
        public const int BlockSize = 11;

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
