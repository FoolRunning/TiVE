using System.Runtime.CompilerServices;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public sealed class VoxelSprite
    {
        public readonly Vector3i Size;

        private readonly Voxel[] voxels;

        public VoxelSprite(int sizeX, int sizeY, int sizeZ)
        {
            Size = new Vector3i(sizeX, sizeY, sizeZ);
            voxels = new Voxel[sizeX * sizeY * sizeZ];
            for (int i = 0; i < voxels.Length; i++)
                voxels[i] = Voxel.Empty;
        }

        public Voxel this[int x, int y, int z]
        {
            get { return voxels[GetBlockOffset(x, y, z)]; }
            set { voxels[GetBlockOffset(x, y, z)] = value; }
        }

        /// <summary>
        /// Gets the offset into the voxel array for the voxel at the specified location
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlockOffset(int x, int y, int z)
        {
            TiVEUtils.CheckConstraints(x, y, z, Size);
            return (x * Size.Z + z) * Size.Y + y; // y-axis major for speed
        }
    }
}
