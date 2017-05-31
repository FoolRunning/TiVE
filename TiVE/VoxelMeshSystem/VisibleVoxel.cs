namespace ProdigalSoftware.TiVEPluginFramework.Internal
{
    /// <summary>
    /// Holds information about a voxel in a voxel sprite or block that is actually visible from outside. Memory footprint is very small (8 bytes).
    /// </summary>
    internal struct VisibleVoxel
    {
        internal readonly Voxel Voxel;
        private readonly int data;

        public VisibleVoxel(Voxel voxel, int x, int y, int z, CubeSides sides, bool checkSurroundingVoxels)
        {
            Voxel = voxel;
            data = (x & 0xFF) << 24 | (y & 0xFF) << 16 | (z & 0xFF) << 8 | ((byte)sides & 0x3F) | (checkSurroundingVoxels ? 0x40 : 0x00);
        }

        public int X => (data >> 24) & 0xFF;

        public int Y => (data >> 16) & 0xFF;

        public int Z => (data >> 8) & 0xFF;

        public CubeSides Sides => (CubeSides)(data & 0x3F);

        public bool CheckSurroundingVoxels => data >= 0x40;
    }
}
