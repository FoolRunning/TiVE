using System.Runtime.CompilerServices;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVE.VoxelMeshSystem;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.RenderSystem
{
    internal class VoxelGroup
    {
        private static readonly MeshBuilder meshBuilder = new MeshBuilder(50000);

        protected readonly Voxel[] voxels;

        private readonly Vector3i size;
        private int voxelCount;
        private int renderedVoxelCount;
        private IVertexDataCollection mesh;

        public VoxelGroup(int sizeX, int sizeY, int sizeZ)
        {
            size = new Vector3i(sizeX, sizeY, sizeZ);
            voxels = new Voxel[sizeX * sizeY * sizeZ];
        }

        //public VoxelGroup(Block block) : this(Block.VoxelSize, Block.VoxelSize, Block.VoxelSize)
        //{
        //    Array.Copy(block.VoxelsArray, voxels, voxels.Length);
        //}

        public void Dispose()
        {
            if (mesh != null)
                mesh.Dispose();

            mesh = null;
        }

        public Vector3i Size
        {
            get { return size; }
        }

        public Voxel this[int x, int y, int z]
        {
            get { return voxels[GetBlockOffset(x, y, z)]; }
            set { voxels[GetBlockOffset(x, y, z)] = value; }
        }

        public RenderStatistics Render(ShaderManager shaderManager, ref Matrix4f matrixMVP)
        {
            if (mesh == null)
            {
                mesh = CreateVoxelMesh();
                mesh.Initialize();
                meshBuilder.DropMesh();
            }

            ShaderProgram shader = shaderManager.GetShaderProgram("MainWorld");
            shader.Bind();

            shader.SetUniform("matrix_ModelViewProjection", ref matrixMVP);
            
            TiVEController.Backend.Draw(PrimitiveType.Triangles, mesh);

            return new RenderStatistics(1, voxelCount, renderedVoxelCount);
        }

        private IVertexDataCollection CreateVoxelMesh()
        {
            voxelCount = 0;
            renderedVoxelCount = 0;

            meshBuilder.StartNewMesh();
            for (byte z = 0; z < size.Z; z++)
            {
                for (byte x = 0; x < size.X; x++)
                {
                    for (byte y = 0; y < size.Y; y++)
                    {
                        Voxel color = this[x, y, z];
                        if (color == 0)
                            continue;

                        voxelCount++;

                        CubeSides sides = 0;
                        if (z == 0 || voxels[GetBlockOffset(x, y, z - 1)] == 0)
                            sides |= CubeSides.ZMinus;
                        if (z == size.Z - 1 || voxels[GetBlockOffset(x, y, z + 1)] == 0)
                            sides |= CubeSides.ZPlus;
                        if (x == 0 || voxels[GetBlockOffset(x - 1, y, z)] == 0)
                            sides |= CubeSides.XMinus;
                        if (x == size.X - 1 || voxels[GetBlockOffset(x + 1, y, z)] == 0)
                            sides |= CubeSides.XPlus;
                        if (y == 0 || voxels[GetBlockOffset(x, y - 1, z)] == 0)
                            sides |= CubeSides.YMinus;
                        if (y == size.Y - 1 || voxels[GetBlockOffset(x, y + 1, z)] == 0)
                            sides |= CubeSides.YPlus;

                        if (sides != CubeSides.None)
                        {
                            // ENHANCE: Calculate ambient occlusion factor for mesh

                            meshBuilder.AddVoxel(sides, x, y, z, (Color4b)color, VoxelMeshUtils.GetVoxelNormal(sides), 255);
                            renderedVoxelCount++;
                        }
                    }
                }
            }

            return (IVertexDataCollection)meshBuilder.GetMesh();
        }
        
        /// <summary>
        /// Gets the offset into the voxel array for the voxel at the specified location
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlockOffset(int x, int y, int z)
        {
            TiVEUtils.DebugCheckConstraints(x, y, z, size);
            return (x * size.Z + z) * size.Y + y; // y-axis major for speed
        }
    }
}
