namespace ProdigalSoftware.TiVEPluginFramework.Internal
{
    internal sealed class RenderedVoxel
    {
        internal readonly Voxel Voxel;
        internal readonly Vector3b Location;
        private readonly byte data;

        public RenderedVoxel(Voxel voxel, Vector3b location, CubeSides sides, bool checkSurroundingVoxels)
        {
            Voxel = voxel;
            Location = location;
            data = (byte)(((byte)sides & 0x3F) | (checkSurroundingVoxels ? 0x40 : 0x00));
        }

        public CubeSides Sides => 
            (CubeSides)(data & 0x3F);

        public bool CheckSurroundingVoxels => 
            data >= 0x40;
    }
}
