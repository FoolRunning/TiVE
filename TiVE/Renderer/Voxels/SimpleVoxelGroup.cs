using System;

namespace ProdigalSoftware.TiVE.Renderer.Voxels
{
    internal sealed class SimpleVoxelGroup : VoxelGroup
    {
        public SimpleVoxelGroup(int sizeX, int sizeY, int sizeZ) : base(sizeX, sizeY, sizeZ)
        {
        }

        public SimpleVoxelGroup(uint[, ,] voxels) : base(voxels)
        {
        }

        protected override int AddVoxel(MeshBuilder meshBuilder, VoxelSides sides, int x, int y, int z, byte cr, byte cg, byte cb, byte ca)
        {
            return CreateVoxel(meshBuilder, sides, x, y, z, cr, cg, cb, ca);
        }

        public static int CreateVoxel(MeshBuilder meshBuilder, VoxelSides sides, int x, int y, int z, byte cr, byte cg, byte cb, byte ca)
        {
            int polygonCount = 0;
            if ((sides & VoxelSides.Front) != 0)
            {
                meshBuilder.Add(x, y, z + 1, cr, cg, cb, ca);
                meshBuilder.Add(x + 1, y + 1, z + 1, cr, cg, cb, ca);
                meshBuilder.Add(x + 1, y, z + 1, cr, cg, cb, ca);

                meshBuilder.Add(x + 1, y + 1, z + 1, cr, cg, cb, ca);
                meshBuilder.Add(x, y, z + 1, cr, cg, cb, ca);
                meshBuilder.Add(x, y + 1, z + 1, cr, cg, cb, ca);
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
                byte crl = (byte)Math.Min(255, cr + SmallColorDiff);
                byte cgl = (byte)Math.Min(255, cg + SmallColorDiff);
                byte cbl = (byte)Math.Min(255, cb + SmallColorDiff);
                meshBuilder.Add(x, y, z, crl, cgl, cbl, ca);
                meshBuilder.Add(x, y + 1, z, crl, cgl, cbl, ca);
                meshBuilder.Add(x, y + 1, z + 1, crl, cgl, cbl, ca);

                meshBuilder.Add(x, y + 1, z + 1, crl, cgl, cbl, ca);
                meshBuilder.Add(x, y, z + 1, crl, cgl, cbl, ca);
                meshBuilder.Add(x, y, z, crl, cgl, cbl, ca);
                polygonCount += 2;
            }

            if ((sides & VoxelSides.Right) != 0)
            {
                byte crr = (byte)Math.Max(0, cr - SmallColorDiff);
                byte cgr = (byte)Math.Max(0, cg - SmallColorDiff);
                byte cbr = (byte)Math.Max(0, cb - SmallColorDiff);
                meshBuilder.Add(x + 1, y, z, crr, cgr, cbr, ca);
                meshBuilder.Add(x + 1, y + 1, z + 1, crr, cgr, cbr, ca);
                meshBuilder.Add(x + 1, y + 1, z, crr, cgr, cbr, ca);

                meshBuilder.Add(x + 1, y + 1, z + 1, crr, cgr, cbr, ca);
                meshBuilder.Add(x + 1, y, z, crr, cgr, cbr, ca);
                meshBuilder.Add(x + 1, y, z + 1, crr, cgr, cbr, ca);
                polygonCount += 2;
            }

            if ((sides & VoxelSides.Bottom) != 0)
            {
                byte crb = (byte)Math.Max(0, cr - BigColorDiff);
                byte cgb = (byte)Math.Max(0, cg - BigColorDiff);
                byte cbb = (byte)Math.Max(0, cb - BigColorDiff);
                meshBuilder.Add(x, y, z, crb, cgb, cbb, ca);
                meshBuilder.Add(x + 1, y, z + 1, crb, cgb, cbb, ca);
                meshBuilder.Add(x + 1, y, z, crb, cgb, cbb, ca);

                meshBuilder.Add(x, y, z, crb, cgb, cbb, ca);
                meshBuilder.Add(x, y, z + 1, crb, cgb, cbb, ca);
                meshBuilder.Add(x + 1, y, z + 1, crb, cgb, cbb, ca);
                polygonCount += 2;
            }

            if ((sides & VoxelSides.Top) != 0)
            {
                byte crt = (byte)Math.Min(255, cr + BigColorDiff);
                byte cgt = (byte)Math.Min(255, cg + BigColorDiff);
                byte cbt = (byte)Math.Min(255, cb + BigColorDiff);
                meshBuilder.Add(x, y + 1, z, crt, cgt, cbt, ca);
                meshBuilder.Add(x + 1, y + 1, z, crt, cgt, cbt, ca);
                meshBuilder.Add(x + 1, y + 1, z + 1, crt, cgt, cbt, ca);

                meshBuilder.Add(x, y + 1, z, crt, cgt, cbt, ca);
                meshBuilder.Add(x + 1, y + 1, z + 1, crt, cgt, cbt, ca);
                meshBuilder.Add(x, y + 1, z + 1, crt, cgt, cbt, ca);
                polygonCount += 2;
            }

            return polygonCount;
        }
    }
}
