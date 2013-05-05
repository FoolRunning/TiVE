using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ProdigalSoftware.TiVEBlockEditor
{
    internal static class ImmediateBlockRenderer
    {
        public static void Draw(uint[,,] voxels)
        {
            int voxelCountX = voxels.GetLength(0);
            int voxelCountY = voxels.GetLength(1);
            int voxelCountZ = voxels.GetLength(2);

            GL.Begin(BeginMode.Triangles);
            for (int z = voxelCountZ - 1; z >= 0; z--)
            {
                for (int x = 0; x < voxelCountX; x++)
                {
                    for (int y = 0; y < voxelCountY; y++)
                    {
                        uint color = voxels[x, y, z];
                        if (color == 0)
                            continue;

                        byte cr = (byte)((color >> 0) & 0xFF);
                        byte cg = (byte)((color >> 8) & 0xFF);
                        byte cb = (byte)((color >> 16) & 0xFF);
                        byte ca = (byte)((color >> 24) & 0xFF);
                        GL.Color4(cr, cg, cb, ca);

                        Vector3 v1 = new Vector3(x       , y + 1 , z);
                        Vector3 v2 = new Vector3(x + 1   , y + 1 , z);
                        Vector3 v3 = new Vector3(x + 1   , y + 1 , z + 1);
                        Vector3 v4 = new Vector3(x       , y + 1 , z + 1);
                        Vector3 v5 = new Vector3(x       , y     , z);
                        Vector3 v6 = new Vector3(x + 1   , y     , z);
                        Vector3 v7 = new Vector3(x + 1   , y     , z + 1);
                        Vector3 v8 = new Vector3(x       , y     , z + 1);

                        if (z == voxelCountZ - 1 || voxels[x, y, z + 1] != 0)
                        {
                            GL.Vertex3(v8);
                            GL.Vertex3(v3);
                            GL.Vertex3(v7);

                            GL.Vertex3(v3);
                            GL.Vertex3(v8);
                            GL.Vertex3(v4);
                        }

                        //if (!IsZLineSet(x, y, z, 1))
                        //{
                        //    GL.Vertex3(x * World_Block_Size + World_Block_Size, y * World_Block_Size + World_Block_Size, z * World_Block_Size + World_Block_Size);
                        //    GL.Vertex3(x * World_Block_Size + World_Block_Size, y * World_Block_Size, z * World_Block_Size + World_Block_Size);
                        //    GL.Vertex3(x * World_Block_Size, y * World_Block_Size, z * World_Block_Size + World_Block_Size);

                        //    GL.Vertex3(x * World_Block_Size, y * World_Block_Size, z * World_Block_Size + World_Block_Size);
                        //    GL.Vertex3(x * World_Block_Size, y * World_Block_Size + World_Block_Size, z * World_Block_Size + World_Block_Size);
                        //    GL.Vertex3(x * World_Block_Size + World_Block_Size, y * World_Block_Size + World_Block_Size, z * World_Block_Size + World_Block_Size);
                        //    PolygonCount += 2;
                        //}

                        if (x == 0 || voxels[x - 1, y, z] != 0)
                        {
                            GL.Vertex3(v5);
                            GL.Vertex3(v1);
                            GL.Vertex3(v4);

                            GL.Vertex3(v4);
                            GL.Vertex3(v8);
                            GL.Vertex3(v5);
                        }

                        if (x == voxelCountX - 1 || voxels[x + 1, y, z] != 0)
                        {
                            GL.Vertex3(v6);
                            GL.Vertex3(v3);
                            GL.Vertex3(v2);

                            GL.Vertex3(v3);
                            GL.Vertex3(v6);
                            GL.Vertex3(v7);
                        }

                        if (y == 0 || voxels[x, y - 1, z] != 0)
                        {
                            GL.Vertex3(v5);
                            GL.Vertex3(v7);
                            GL.Vertex3(v6);

                            GL.Vertex3(v5);
                            GL.Vertex3(v8);
                            GL.Vertex3(v7);
                        }

                        if (y == voxelCountY - 1 || voxels[x, y + 1, z] != 0)
                        {
                            GL.Vertex3(v1);
                            GL.Vertex3(v2);
                            GL.Vertex3(v3);

                            GL.Vertex3(v1);
                            GL.Vertex3(v3);
                            GL.Vertex3(v4);
                        }
                    }
                }
            }
            GL.End();
        }
    }
}
