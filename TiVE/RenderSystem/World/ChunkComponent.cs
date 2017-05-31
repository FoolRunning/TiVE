using System.IO;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;

namespace ProdigalSoftware.TiVE.RenderSystem.World
{
    internal sealed class ChunkComponent : VoxelMeshComponent
    {
        /// <summary>Size in blocks of a chunk on each axis</summary>
        public const int BlockSize = 4;
        /// <summary>Size in voxels of a chunk on each axis</summary>
        public const int VoxelSize = BlockSize * BlockLOD32.VoxelSize;

        public readonly Vector3i ChunkLoc;

        public RenderedLight[] LightsInChunk;

        public ChunkComponent(Vector3i chunkLoc) : base(new Vector3f(chunkLoc.X * VoxelSize, chunkLoc.Y * VoxelSize, chunkLoc.Z * VoxelSize))
        {
            ChunkLoc = chunkLoc;
        }

        public Vector3i ChunkBlockLoc
        {
            get { return new Vector3i(ChunkLoc.X * BlockSize, ChunkLoc.Y * BlockSize, ChunkLoc.Z * BlockSize); }
        }

        public override void SaveTo(BinaryWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}
