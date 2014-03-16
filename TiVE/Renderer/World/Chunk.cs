#define USE_INSTANCED_RENDERING
using OpenTK;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    internal sealed class Chunk
    {
        private readonly int blockStartX;
        private readonly int blockStartY;
        private Matrix4 translationMatrix;

#if USE_INSTANCED_RENDERING
            private readonly InstancedVoxelGroup voxels;

            public Chunk(InstancedVoxelGroup voxels, ref Matrix4 translationMatrix, int blockStartX, int blockStartY)
#else
        private readonly VoxelGroup voxels;

        public Chunk(VoxelGroup voxels, ref Matrix4 translationMatrix, int blockStartX, int blockStartY)
#endif
        {
            this.voxels = voxels;
            this.translationMatrix = translationMatrix;
            this.blockStartX = blockStartX;
            this.blockStartY = blockStartY;
        }

#if USE_INSTANCED_RENDERING
            public InstancedVoxelGroup VoxelData
#else
        public VoxelGroup VoxelData
#endif
        {
            get { return voxels; }
        }

        public void Render(ref Matrix4 viewProjectionMatrix)
        {
            Matrix4 viewProjectionModelMatrix;
            Matrix4.Mult(ref translationMatrix, ref viewProjectionMatrix, out viewProjectionModelMatrix);

            voxels.Render(ref viewProjectionModelMatrix);
        }

        public void Delete()
        {
            voxels.Delete();
        }
    }

}
