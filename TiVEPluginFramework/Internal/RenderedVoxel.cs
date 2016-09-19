using System;

namespace ProdigalSoftware.TiVEPluginFramework.Internal
{
    [Flags]
    internal enum VoxelSides : byte
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

    internal sealed class RenderedVoxel
    {
        internal readonly Voxel Voxel;
        internal readonly Vector3b Location;
        private readonly byte data;

        public RenderedVoxel(Voxel voxel, Vector3b location, VoxelSides sides, bool checkSurroundingVoxels)
        {
            Voxel = voxel;
            Location = location;
            data = (byte)(((byte)sides & 0x3F) | (checkSurroundingVoxels ? 0x40 : 0x00));
        }

        public VoxelSides Sides
        {
            get { return (VoxelSides)(data & 0x3F); }
        }

        public bool CheckSurroundingVoxels
        {
            get { return data >= 0x40; }
        }
    }
}
