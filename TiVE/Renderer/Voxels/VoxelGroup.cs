using System;
using System.Runtime.CompilerServices;
using OpenTK;
using ProdigalSoftware.TiVE.Utils;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.Voxels
{
    [Flags]
    internal enum VoxelSides
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

    internal abstract class VoxelGroup
    {
        private static readonly MeshBuilder meshBuilder = new MeshBuilder(10000, 50000);

        protected readonly uint[] voxels;

        private readonly Vector3i size;
        private int polygonCount;
        private int voxelCount;
        private int renderedVoxelCount;
        private IVertexDataCollection mesh;

        protected VoxelGroup(int sizeX, int sizeY, int sizeZ)
        {
            size = new Vector3i(sizeX, sizeY, sizeZ);
            voxels = new uint[sizeX * sizeY * sizeZ];
        }

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

        public uint this[int x, int y, int z]
        {
            get { return voxels[GetBlockOffset(x, y, z)]; }
            set { voxels[GetBlockOffset(x, y, z)] = value; }
        }

        public RenderStatistics Render(ShaderManager shaderManager, ref Matrix4 matrixMVP)
        {
            if (mesh == null)
            {
                mesh = CreateVoxelMesh();
                mesh.Initialize();
                meshBuilder.DropMesh();
            }

            IShaderProgram shader = shaderManager.GetShaderProgram("MainWorld");
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
                        uint color = this[x, y, z];
                        if (color == 0)
                            continue;

                        voxelCount++;

                        VoxelSides sides = 0;
                        if (z == 0 || this[x, y, z - 1] == 0) // The back face is never shown to the camera, so there is no need to create it
                            sides |= VoxelSides.Back;
                        if (z == size.Z - 1 || this[x, y, z + 1] == 0)
                            sides |= VoxelSides.Front;
                        if (x == 0 || this[x - 1, y, z] == 0)
                            sides |= VoxelSides.Left;
                        if (x == size.X - 1 || this[x + 1, y, z] == 0)
                            sides |= VoxelSides.Right;
                        if (y == 0 || this[x, y - 1, z] == 0)
                            sides |= VoxelSides.Bottom;
                        if (y == size.Y - 1 || this[x, y + 1, z] == 0)
                            sides |= VoxelSides.Top;

                        if (sides != VoxelSides.None)
                        {
                            polygonCount += AddVoxel(meshBuilder, sides, x, y, z, 
                                new Color4b((byte)((color >> 16) & 0xFF), (byte)((color >> 8) & 0xFF), (byte)((color >> 0) & 0xFF), 
                                    (byte)((color >> 24) & 0xFF)));
                            renderedVoxelCount++;
                        }
                    }
                }
            }

            return meshBuilder.GetMesh();
        }

        protected abstract int AddVoxel(MeshBuilder meshBuilder, VoxelSides sides, byte x, byte y, byte z, Color4b color);

        /// <summary>
        /// Gets the offset into the voxel array for the voxel at the specified location
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlockOffset(int x, int y, int z)
        {
            TiVEUtils.CheckConstraints(x, y, z, size);
            return (x * size.Z + z) * size.Y + y; // y-axis major for speed
        }
    }
}
