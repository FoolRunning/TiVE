using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.Voxels
{
    internal sealed class IndexedVoxelGroup : VoxelGroup
    {
        public IndexedVoxelGroup(int sizeX, int sizeY, int sizeZ) : base(sizeX, sizeY, sizeZ)
        {
        }

        public IndexedVoxelGroup(uint[, ,] voxels) : base(voxels)
        {
        }

        protected override int AddVoxel(MeshBuilder meshBuilder, VoxelSides sides, int x, int y, int z, Color4b color)
        {
            return CreateVoxel(meshBuilder, sides, x, y, z, color);
        }
        
        public static int CreateVoxel(MeshBuilder meshBuilder, VoxelSides sides, int x, int y, int z, Color4b color)
        {
            int v1 = meshBuilder.Add(x, y + 1, z, color);
            int v2 = meshBuilder.Add(x + 1, y + 1, z, color);
            int v3 = meshBuilder.Add(x + 1, y + 1, z + 1, color);
            int v4 = meshBuilder.Add(x, y + 1, z + 1, color);
            int v5 = meshBuilder.Add(x, y, z, color);
            int v6 = meshBuilder.Add(x + 1, y, z, color);
            int v7 = meshBuilder.Add(x + 1, y, z + 1, color);
            int v8 = meshBuilder.Add(x, y, z + 1, color);

            int polygonCount = 0;
            if ((sides & VoxelSides.Front) != 0)
            {
                meshBuilder.AddIndex(v8);
                meshBuilder.AddIndex(v3);
                meshBuilder.AddIndex(v7);

                meshBuilder.AddIndex(v3);
                meshBuilder.AddIndex(v8);
                meshBuilder.AddIndex(v4);
                polygonCount += 2;
            }

            // The back face is never shown to the camera, so there is no need to create it
            //if ((sides & VoxelSides.Back) != 0)
            //{
            //    GL.Vertex3(x * World_Block_Size + World_Block_Size, y * World_Block_Size + World_Block_Size, z * World_Block_Size + World_Block_Size);
            //    GL.Vertex3(x * World_Block_Size + World_Block_Size, y * World_Block_Size, z * World_Block_Size + World_Block_Size);
            //    GL.Vertex3(x * World_Block_Size, y * World_Block_Size, z * World_Block_Size + World_Block_Size);

            //    GL.Vertex3(x * World_Block_Size, y * World_Block_Size, z * World_Block_Size + World_Block_Size);
            //    GL.Vertex3(x * World_Block_Size, y * World_Block_Size + World_Block_Size, z * World_Block_Size + World_Block_Size);
            //    GL.Vertex3(x * World_Block_Size + World_Block_Size, y * World_Block_Size + World_Block_Size, z * World_Block_Size + World_Block_Size);
            //    PolygonCount += 2;
            //}

            if ((sides & VoxelSides.Left) != 0)
            {
                meshBuilder.AddIndex(v5);
                meshBuilder.AddIndex(v1);
                meshBuilder.AddIndex(v4);

                meshBuilder.AddIndex(v4);
                meshBuilder.AddIndex(v8);
                meshBuilder.AddIndex(v5);
                polygonCount += 2;
            }

            if ((sides & VoxelSides.Right) != 0)
            {
                meshBuilder.AddIndex(v6);
                meshBuilder.AddIndex(v3);
                meshBuilder.AddIndex(v2);

                meshBuilder.AddIndex(v3);
                meshBuilder.AddIndex(v6);
                meshBuilder.AddIndex(v7);
                polygonCount += 2;
            }

            if ((sides & VoxelSides.Bottom) != 0)
            {
                meshBuilder.AddIndex(v5);
                meshBuilder.AddIndex(v7);
                meshBuilder.AddIndex(v6);

                meshBuilder.AddIndex(v5);
                meshBuilder.AddIndex(v8);
                meshBuilder.AddIndex(v7);
                polygonCount += 2;
            }

            if ((sides & VoxelSides.Top) != 0)
            {
                meshBuilder.AddIndex(v1);
                meshBuilder.AddIndex(v2);
                meshBuilder.AddIndex(v3);

                meshBuilder.AddIndex(v1);
                meshBuilder.AddIndex(v3);
                meshBuilder.AddIndex(v4);
                polygonCount += 2;
            }

            return polygonCount;
        }
    }
}
