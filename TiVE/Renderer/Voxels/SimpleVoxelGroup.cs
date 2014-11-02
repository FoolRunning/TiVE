using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.Voxels
{
    internal sealed class SimpleVoxelGroup : VoxelGroup
    {
        public SimpleVoxelGroup(int sizeX, int sizeY, int sizeZ) : base(sizeX, sizeY, sizeZ)
        {
        }

        protected override int AddVoxel(MeshBuilder meshBuilder, VoxelSides sides, byte x, byte y, byte z, Color4b color)
        {
            return CreateVoxel(meshBuilder, sides, x, y, z, color);
        }

        public static int CreateVoxel(MeshBuilder meshBuilder, VoxelSides sides, byte x, byte y, byte z, Color4b color)
        {
            int polygonCount = 0;
            byte x2 = (byte)(x + 1);
            byte y2 = (byte)(y + 1);
            byte z2 = (byte)(z + 1);
            if ((sides & VoxelSides.Front) != 0)
            {
                meshBuilder.Add(x, y, z2, color);
                meshBuilder.Add(x2, y2, z2, color);
                meshBuilder.Add(x2, y, z2, color);

                meshBuilder.Add(x2, y2, z2, color);
                meshBuilder.Add(x, y, z2, color);
                meshBuilder.Add(x, y2, z2, color);
                polygonCount += 2;
            }

            if ((sides & VoxelSides.Back) != 0)
            {
                meshBuilder.Add(x, y, z, color);
                meshBuilder.Add(x2, y, z, color);
                meshBuilder.Add(x2, y2, z, color);

                meshBuilder.Add(x2, y2, z, color);
                meshBuilder.Add(x, y2, z, color);
                meshBuilder.Add(x, y, z, color);
                polygonCount += 2;
            }

            if ((sides & VoxelSides.Left) != 0)
            {
                meshBuilder.Add(x, y, z, color);
                meshBuilder.Add(x, y2, z, color);
                meshBuilder.Add(x, y2, z2, color);

                meshBuilder.Add(x, y2, z2, color);
                meshBuilder.Add(x, y, z2, color);
                meshBuilder.Add(x, y, z, color);
                polygonCount += 2;
            }

            if ((sides & VoxelSides.Right) != 0)
            {
                meshBuilder.Add(x2, y, z, color);
                meshBuilder.Add(x2, y2, z2, color);
                meshBuilder.Add(x2, y2, z, color);

                meshBuilder.Add(x2, y2, z2, color);
                meshBuilder.Add(x2, y, z, color);
                meshBuilder.Add(x2, y, z2, color);
                polygonCount += 2;
            }

            if ((sides & VoxelSides.Bottom) != 0)
            {
                meshBuilder.Add(x, y, z, color);
                meshBuilder.Add(x2, y, z2, color);
                meshBuilder.Add(x2, y, z, color);

                meshBuilder.Add(x, y, z, color);
                meshBuilder.Add(x, y, z2, color);
                meshBuilder.Add(x2, y, z2, color);
                polygonCount += 2;
            }

            if ((sides & VoxelSides.Top) != 0)
            {
                meshBuilder.Add(x, y2, z, color);
                meshBuilder.Add(x2, y2, z, color);
                meshBuilder.Add(x2, y2, z2, color);

                meshBuilder.Add(x, y2, z, color);
                meshBuilder.Add(x2, y2, z2, color);
                meshBuilder.Add(x, y2, z2, color);
                polygonCount += 2;
            }

            return polygonCount;
        }
    }
}
