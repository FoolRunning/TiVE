using System;

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

        protected override int AddVoxel(MeshBuilder meshBuilder, VoxelSides sides, int x, int y, int z, byte cr, byte cg, byte cb, byte ca)
        {
            int polygonCount = 0;
            if ((sides & VoxelSides.Front) != 0)
            {
                int v3 = meshBuilder.Add(x + 1, y + 1, z + 1, cr, cg, cb, ca);
                int v4 = meshBuilder.Add(x, y + 1, z + 1, cr, cg, cb, ca);
                int v7 = meshBuilder.Add(x + 1, y, z + 1, cr, cg, cb, ca);
                int v8 = meshBuilder.Add(x, y, z + 1, cr, cg, cb, ca);

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
                byte crr = (byte)Math.Min(255, cr + SmallColorDiff);
                byte cgr = (byte)Math.Min(255, cg + SmallColorDiff);
                byte cbr = (byte)Math.Min(255, cb + SmallColorDiff);
                int v1 = meshBuilder.Add(x, y + 1, z, crr, cgr, cbr, ca);
                int v4 = meshBuilder.Add(x, y + 1, z + 1, crr, cgr, cbr, ca);
                int v5 = meshBuilder.Add(x, y, z, crr, cgr, cbr, ca);
                int v8 = meshBuilder.Add(x, y, z + 1, crr, cgr, cbr, ca);

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
                byte crl = (byte)Math.Max(0, cr - SmallColorDiff);
                byte cgl = (byte)Math.Max(0, cg - SmallColorDiff);
                byte cbl = (byte)Math.Max(0, cb - SmallColorDiff);
                int v2 = meshBuilder.Add(x + 1, y + 1, z, crl, cgl, cbl, ca);
                int v3 = meshBuilder.Add(x + 1, y + 1, z + 1, crl, cgl, cbl, ca);
                int v6 = meshBuilder.Add(x + 1, y, z, crl, cgl, cbl, ca);
                int v7 = meshBuilder.Add(x + 1, y, z + 1, crl, cgl, cbl, ca);

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
                byte crb = (byte)Math.Max(0, cr - BigColorDiff);
                byte cgb = (byte)Math.Max(0, cg - BigColorDiff);
                byte cbb = (byte)Math.Max(0, cb - BigColorDiff);
                int v5 = meshBuilder.Add(x, y, z, crb, cgb, cbb, ca);
                int v6 = meshBuilder.Add(x + 1, y, z, crb, cgb, cbb, ca);
                int v7 = meshBuilder.Add(x + 1, y, z + 1, crb, cgb, cbb, ca);
                int v8 = meshBuilder.Add(x, y, z + 1, crb, cgb, cbb, ca);

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
                byte crt = (byte)Math.Min(255, cr + BigColorDiff);
                byte cgt = (byte)Math.Min(255, cg + BigColorDiff);
                byte cbt = (byte)Math.Min(255, cb + BigColorDiff);
                int v1 = meshBuilder.Add(x, y + 1, z, crt, cgt, cbt, ca);
                int v2 = meshBuilder.Add(x + 1, y + 1, z, crt, cgt, cbt, ca);
                int v3 = meshBuilder.Add(x + 1, y + 1, z + 1, crt, cgt, cbt, ca);
                int v4 = meshBuilder.Add(x, y + 1, z + 1, crt, cgt, cbt, ca);

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
