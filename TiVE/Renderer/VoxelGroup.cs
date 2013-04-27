using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Diagnostics;
using OpenTK;

namespace ProdigalSoftware.TiVE.Renderer
{
    public class VoxelGroup
    {
        private const string vertexShaderSource = @"
            #version 150 core 
 
            // incoming vertex position
            in vec3 in_Position;
            in vec4 in_Color;
 
            // object space to clip space transformation
            uniform mat4 matrix_ModelViewProjection;

            out vec4 fragment_color;
 
            void main(void)
            {
                fragment_color = in_Color;

                // transforming the incoming vertex position
                gl_Position = matrix_ModelViewProjection * vec4(in_Position, 1.0);
            }";

        private const string fragmentShaderSource = @"
                #version 150 core 
                //precision highp float;

                in vec4 fragment_color;

                out vec4 color;

				void main(void)
				{
					color = fragment_color;
				}	
			";

        private const float Voxel_Size = 1.0f;
        private const float Voxel_Offset = 1.0f;

        private readonly uint[, ,] voxels;

        private ShaderProgram shader;
        private Mesh mesh;

        public VoxelGroup(int sizeX, int sizeY, int sizeZ)
        {
            voxels = new uint[sizeX, sizeY, sizeZ];
        }

        public void SetVoxel(int x, int y, int z, uint newColor)
        {
            voxels[x, y, z] = newColor;
        }

        public void ClearVoxel(int x, int y, int z)
        {
            voxels[x, y, z] = 0;
        }

        public bool IsVoxelSet(int x, int y, int z)
        {
            return voxels[x, y, z] != 0;
        }

        public virtual void Render(ref Matrix4 matrixMVP)
        {
            if (mesh == null)
            {
                mesh = CreateVoxelMesh();
                mesh.Initialize();
            }

            if (shader == null)
                shader = CreateCubeShader();

            shader.Bind();
            shader.SetMVPMatrix(ref matrixMVP);

            mesh.Draw();
        }

        public int PolygonCount { get; private set; }

        public void Delete()
        {
            if (shader != null)
                shader.Delete();

            if (mesh != null)
                mesh.Delete();
        }

        protected static uint FromColor(Color color)
        {
            uint val = color.R;
            val |= (uint)color.G << 8;
            val |= (uint)color.B << 16;
            val |= (uint)color.A << 24;
            return val;
        }

        private static ShaderProgram CreateCubeShader()
        {
            ShaderProgram program = new ShaderProgram(vertexShaderSource, fragmentShaderSource, null);
            if (!program.Initialize())
                Debug.WriteLine("Failed to initialize shader");
            return program;
        }

        private Mesh CreateVoxelMesh()
        {
            MeshBuilder builder = new MeshBuilder(BeginMode.Triangles, true, false, false);

            PolygonCount = 0;

            int voxelCountX = voxels.GetLength(0);
            int voxelCountY = voxels.GetLength(1);
            int voxelCountZ = voxels.GetLength(2);

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
                        //Debug.WriteLine(string.Format("Color value: {0} - ({1}, {2}, {3})", color, (int)(color & 0xFF), (int)((color >> 8) & 0xFF), (int)((color >> 16) & 0xFF)));

                        uint v1 = builder.AddVertex(x * Voxel_Offset, y * Voxel_Offset + Voxel_Size, z * Voxel_Offset, cr, cg, cb, ca);
                        uint v2 = builder.AddVertex(x * Voxel_Offset + Voxel_Size, y * Voxel_Offset + Voxel_Size, z * Voxel_Offset, cr, cg, cb, ca);
                        uint v3 = builder.AddVertex(x * Voxel_Offset + Voxel_Size, y * Voxel_Offset + Voxel_Size, z * Voxel_Offset + Voxel_Size, cr, cg, cb, ca);
                        uint v4 = builder.AddVertex(x * Voxel_Offset, y * Voxel_Offset + Voxel_Size, z * Voxel_Offset + Voxel_Size, cr, cg, cb, ca);
                        uint v5 = builder.AddVertex(x * Voxel_Offset, y * Voxel_Offset, z * Voxel_Offset, cr, cg, cb, ca);
                        uint v6 = builder.AddVertex(x * Voxel_Offset + Voxel_Size, y * Voxel_Offset, z * Voxel_Offset, cr, cg, cb, ca);
                        uint v7 = builder.AddVertex(x * Voxel_Offset + Voxel_Size, y * Voxel_Offset, z * Voxel_Offset + Voxel_Size, cr, cg, cb, ca);
                        uint v8 = builder.AddVertex(x * Voxel_Offset, y * Voxel_Offset, z * Voxel_Offset + Voxel_Size, cr, cg, cb, ca);

                        if (z == voxelCountZ - 1 || !IsVoxelSet(x, y, z + 1))
                        {
                            builder.AddIndex(v8);
                            builder.AddIndex(v3);
                            builder.AddIndex(v7);

                            builder.AddIndex(v3);
                            builder.AddIndex(v8);
                            builder.AddIndex(v4);
                            PolygonCount += 2;
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

                        if (x == 0 || !IsVoxelSet(x - 1, y, z))
                        {
                            builder.AddIndex(v5);
                            builder.AddIndex(v1);
                            builder.AddIndex(v4);

                            builder.AddIndex(v4);
                            builder.AddIndex(v8);
                            builder.AddIndex(v5);
                            PolygonCount += 2;
                        }

                        if (x == voxelCountX - 1 || !IsVoxelSet(x + 1, y, z))
                        {
                            builder.AddIndex(v6);
                            builder.AddIndex(v3);
                            builder.AddIndex(v2);

                            builder.AddIndex(v3);
                            builder.AddIndex(v6);
                            builder.AddIndex(v7);
                            PolygonCount += 2;
                        }

                        if (y == 0 || !IsVoxelSet(x, y - 1, z))
                        {
                            builder.AddIndex(v5);
                            builder.AddIndex(v7);
                            builder.AddIndex(v6);

                            builder.AddIndex(v5);
                            builder.AddIndex(v8);
                            builder.AddIndex(v7);
                            PolygonCount += 2;
                        }

                        if (y == voxelCountY - 1 || !IsVoxelSet(x, y + 1, z))
                        {
                            builder.AddIndex(v1);
                            builder.AddIndex(v2);
                            builder.AddIndex(v3);

                            builder.AddIndex(v1);
                            builder.AddIndex(v3);
                            builder.AddIndex(v4);
                            PolygonCount += 2;
                        }
                    }
                }
            }

            return builder.GetMesh();
        }
    }
}
