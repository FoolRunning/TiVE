using System.Runtime.CompilerServices;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVE.VoxelMeshSystem;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Internal;

namespace ProdigalSoftware.TiVE.RenderSystem
{
    internal class VoxelGroup
    {
        private static readonly MeshBuilder meshBuilder = new MeshBuilder(50000);

        protected readonly Voxel[] voxels;

        private readonly Vector3i size;
        private int polygonCount;
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

            return new RenderStatistics(1, polygonCount, voxelCount, renderedVoxelCount);
        }

        private IVertexDataCollection CreateVoxelMesh()
        {
            voxelCount = 0;
            renderedVoxelCount = 0;
            polygonCount = 0;

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

                        VoxelSides sides = 0;
                        if (z == 0 || voxels[GetBlockOffset(x, y, z - 1)] == 0)
                            sides |= VoxelSides.Back;
                        if (z == size.Z - 1 || voxels[GetBlockOffset(x, y, z + 1)] == 0)
                            sides |= VoxelSides.Front;
                        if (x == 0 || voxels[GetBlockOffset(x - 1, y, z)] == 0)
                            sides |= VoxelSides.Left;
                        if (x == size.X - 1 || voxels[GetBlockOffset(x + 1, y, z)] == 0)
                            sides |= VoxelSides.Right;
                        if (y == 0 || voxels[GetBlockOffset(x, y - 1, z)] == 0)
                            sides |= VoxelSides.Bottom;
                        if (y == size.Y - 1 || voxels[GetBlockOffset(x, y + 1, z)] == 0)
                            sides |= VoxelSides.Top;

                        if (sides != VoxelSides.None)
                        {
                            polygonCount += meshBuilder.AddVoxel(sides, x, y, z, (Color4b)color);
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
            Vector3i voxelSize = size;
            TiVEUtils.CheckConstraints(x, y, z, voxelSize);
            return (x * voxelSize.Z + z) * voxelSize.Y + y; // y-axis major for speed
        }
    }
}
