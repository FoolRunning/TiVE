using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.Voxels
{
    internal sealed class SimpleVoxelGroup : VoxelGroup
    {
        public SimpleVoxelGroup(int sizeX, int sizeY, int sizeZ) : base(sizeX, sizeY, sizeZ)
        {
        }
        
        protected override int AddVoxel(MeshBuilder meshBuilder, VoxelSides sides, int x, int y, int z, Color4b color)
        {
            return CreateVoxel(meshBuilder, sides, x, y, z, color);
        }

        public static int CreateVoxel(MeshBuilder meshBuilder, VoxelSides sides, int x, int y, int z, Color4b color)
        {
            int polygonCount = 0;
            if ((sides & VoxelSides.Front) != 0)
            {
                meshBuilder.Add(x, y, z + 1, color);
                meshBuilder.Add(x + 1, y + 1, z + 1, color);
                meshBuilder.Add(x + 1, y, z + 1, color);

                meshBuilder.Add(x + 1, y + 1, z + 1, color);
                meshBuilder.Add(x, y, z + 1, color);
                meshBuilder.Add(x, y + 1, z + 1, color);
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
            //    polygonCount += 2;
            //}

            if ((sides & VoxelSides.Left) != 0)
            {
                meshBuilder.Add(x, y, z, color);
                meshBuilder.Add(x, y + 1, z, color);
                meshBuilder.Add(x, y + 1, z + 1, color);

                meshBuilder.Add(x, y + 1, z + 1, color);
                meshBuilder.Add(x, y, z + 1, color);
                meshBuilder.Add(x, y, z, color);
                polygonCount += 2;
            }

            if ((sides & VoxelSides.Right) != 0)
            {
                meshBuilder.Add(x + 1, y, z, color);
                meshBuilder.Add(x + 1, y + 1, z + 1, color);
                meshBuilder.Add(x + 1, y + 1, z, color);

                meshBuilder.Add(x + 1, y + 1, z + 1, color);
                meshBuilder.Add(x + 1, y, z, color);
                meshBuilder.Add(x + 1, y, z + 1, color);
                polygonCount += 2;
            }

            if ((sides & VoxelSides.Bottom) != 0)
            {
                meshBuilder.Add(x, y, z, color);
                meshBuilder.Add(x + 1, y, z + 1, color);
                meshBuilder.Add(x + 1, y, z, color);

                meshBuilder.Add(x, y, z, color);
                meshBuilder.Add(x, y, z + 1, color);
                meshBuilder.Add(x + 1, y, z + 1, color);
                polygonCount += 2;
            }

            if ((sides & VoxelSides.Top) != 0)
            {
                meshBuilder.Add(x, y + 1, z, color);
                meshBuilder.Add(x + 1, y + 1, z, color);
                meshBuilder.Add(x + 1, y + 1, z + 1, color);

                meshBuilder.Add(x, y + 1, z, color);
                meshBuilder.Add(x + 1, y + 1, z + 1, color);
                meshBuilder.Add(x, y + 1, z + 1, color);
                polygonCount += 2;
            }

            return polygonCount;
        }
    }
}
