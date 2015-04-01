using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.RenderSystem.World
{
    internal sealed class ChunkComponent : IComponent
    {
        /// <summary>Size in blocks of a chunk on each axis</summary>
        public const int BlockSize = 4;
        /// <summary>Size in voxels of a chunk on each axis</summary>
        public const int VoxelSize = BlockSize * Block.VoxelSize;

        public readonly Vector3i ChunkLoc;
        public readonly Vector3i ChunkBlockLoc;
        public readonly Vector3i ChunkVoxelLoc;

        public ChunkComponent(Vector3i chunkLoc)
        {
            ChunkLoc = chunkLoc;
            ChunkBlockLoc = new Vector3i(chunkLoc.X * BlockSize, chunkLoc.Y * BlockSize, chunkLoc.Z * BlockSize);
            ChunkVoxelLoc = new Vector3i(chunkLoc.X * VoxelSize, chunkLoc.Y * VoxelSize, chunkLoc.Z * VoxelSize);
        }
    }
}
