using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Diagnostics;
using OpenTK;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal class VoxelGroup
    {
        #region Constants
        private const string VertexShaderSource = @"
            #version 150 core 
 
            // premultiplied model to projection transformation
            uniform mat4 matrix_ModelViewProjection;
 
            // incoming vertex information
            in vec3 in_Position;
            in vec4 in_Color;

            flat out vec4 fragment_color;
 
            void main(void)
            {
                fragment_color = in_Color;

                // transforming the incoming vertex position
                gl_Position = matrix_ModelViewProjection * vec4(in_Position, 1.0);
            }";

        private const string GeometryShaderSource = @"
                #version 150 core

                layout (points) in;
                layout (triangle_strip, max_vertices=20) out;

                uniform mat4 matrix_ModelViewProjection;

                flat in vec4 geom_color[];

                flat out vec4 fragment_color;

                void EmitFace(vec4 v1, vec4 v2, vec4 v3, vec4 v4, vec4 color)
                {
                    fragment_color = color;
                    gl_Position = v1;
                    EmitVertex();

                    fragment_color = color;
                    gl_Position = v2;
                    EmitVertex();

                    fragment_color = color;
                    gl_Position = v3;
                    EmitVertex();

                    fragment_color = color;
                    gl_Position = v4;
                    EmitVertex();

                    EndPrimitive();
                }

                void main()
                {
                    vec4 color = vec4(geom_color[0].rgb, 1);

                    vec4 pos = gl_in[0].gl_Position;
                    vec4 v1 = matrix_ModelViewProjection * vec4(pos.x       , pos.y + 1 , pos.z      , 1);
                    vec4 v2 = matrix_ModelViewProjection * vec4(pos.x + 1   , pos.y + 1 , pos.z      , 1);
                    vec4 v3 = matrix_ModelViewProjection * vec4(pos.x + 1   , pos.y + 1 , pos.z + 1  , 1);
                    vec4 v4 = matrix_ModelViewProjection * vec4(pos.x       , pos.y + 1 , pos.z + 1  , 1);
                    vec4 v5 = matrix_ModelViewProjection * pos;
                    vec4 v6 = matrix_ModelViewProjection * vec4(pos.x + 1   , pos.y     , pos.z      , 1);
                    vec4 v7 = matrix_ModelViewProjection * vec4(pos.x + 1   , pos.y     , pos.z + 1  , 1);
                    vec4 v8 = matrix_ModelViewProjection * vec4(pos.x       , pos.y     , pos.z + 1  , 1);

                    int cf = int(geom_color[0].a * 255.0);
                    if ((cf & 16) != 0)
                        EmitFace(v2, v3, v1, v4, color); // top
                    if ((cf & 4) != 0)
                        EmitFace(v2, v6, v3, v7, color); // right
                    if ((cf & 1) != 0)
                        EmitFace(v3, v7, v4, v8, color); // front
                    if ((cf & 8) != 0)
                        EmitFace(v7, v6, v8, v5, color); // bottom
                    if ((cf & 2) != 0)
                        EmitFace(v4, v8, v1, v5, color); // left
                }
            ";

        private const string FragmentShaderSource = @"
                #version 150 core

                flat in vec4 fragment_color;

                out vec4 color;

				void main(void)
				{
					color = fragment_color;
				}
			";
        #endregion

        private static readonly MeshBuilder voxelMeshBuilder = new MeshBuilder(BeginMode.Triangles);

        private readonly uint[, ,] voxels;

        private ShaderProgram shader;
        private Mesh mesh;

        public VoxelGroup(int sizeX, int sizeY, int sizeZ)
        {
            voxels = new uint[sizeX, sizeY, sizeZ];
        }

        public VoxelGroup(uint[,,] voxels)
        {
            this.voxels = voxels;
        }

        ~VoxelGroup()
        {
            Debug.Assert(shader == null);
            Debug.Assert(mesh == null);
        }

        public int PolygonCount { get; private set; }

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
                shader = CreateShader();

            shader.Bind();
            shader.SetMVPMatrix(ref matrixMVP);

            mesh.Draw();
        }

        public void Delete()
        {
            if (shader != null)
                shader.Delete();

            if (mesh != null)
                mesh.Delete();

            shader = null;
            mesh = null;
        }

        private static ShaderProgram CreateShader()
        {
            ShaderProgram program = new ShaderProgram(VertexShaderSource, FragmentShaderSource, null);
            if (!program.Initialize())
                Debug.WriteLine("Failed to initialize shader");
            return program;
        }

        private Mesh CreateVoxelMesh()
        {
            voxelMeshBuilder.BeginNewMesh();
            PolygonCount = 0;

            int voxelCountX = voxels.GetLength(0);
            int voxelCountY = voxels.GetLength(1);
            int voxelCountZ = voxels.GetLength(2);

            for (int z = 0; z < voxelCountZ; z++)
            {
                for (int x = 0; x < voxelCountX; x++)
                {
                    for (int y = 0; y < voxelCountY; y++)
                    {
                        uint color = voxels[x, y, z];
                        if (color == 0)
                            continue;

                        if (z < voxelCountZ - 1 && IsVoxelSet(x, y, z + 1) &&
                            x > 0 && IsVoxelSet(x - 1, y, z) &&
                            x < voxelCountX - 1 && IsVoxelSet(x + 1, y, z) &&
                            y > 0 && IsVoxelSet(x, y - 1, z) &&
                            y < voxelCountY - 1 && IsVoxelSet(x, y + 1, z))
                        {
                            continue; // Voxel is completely covered so no need to process it
                        }

                        byte cr = (byte)((color >> 16) & 0xFF);
                        byte cg = (byte)((color >> 8) & 0xFF);
                        byte cb = (byte)((color >> 0) & 0xFF);
                        byte ca = (byte)((color >> 24) & 0xFF);
                        //Debug.WriteLine(string.Format("Color value: {0} - ({1}, {2}, {3})", color, (int)(color & 0xFF), (int)((color >> 8) & 0xFF), (int)((color >> 16) & 0xFF)));

                        uint v1 = voxelMeshBuilder.AddVertex(x, y + 1, z, cr, cg, cb, ca);
                        uint v2 = voxelMeshBuilder.AddVertex(x + 1, y + 1, z, cr, cg, cb, ca);
                        uint v3 = voxelMeshBuilder.AddVertex(x + 1, y + 1, z + 1, cr, cg, cb, ca);
                        uint v4 = voxelMeshBuilder.AddVertex(x, y + 1, z + 1, cr, cg, cb, ca);
                        uint v5 = voxelMeshBuilder.AddVertex(x, y, z, cr, cg, cb, ca);
                        uint v6 = voxelMeshBuilder.AddVertex(x + 1, y, z, cr, cg, cb, ca);
                        uint v7 = voxelMeshBuilder.AddVertex(x + 1, y, z + 1, cr, cg, cb, ca);
                        uint v8 = voxelMeshBuilder.AddVertex(x, y, z + 1, cr, cg, cb, ca);

                        if (z == voxelCountZ - 1 || !IsVoxelSet(x, y, z + 1))
                        {
                            voxelMeshBuilder.AddIndex(v8);
                            voxelMeshBuilder.AddIndex(v3);
                            voxelMeshBuilder.AddIndex(v7);

                            voxelMeshBuilder.AddIndex(v3);
                            voxelMeshBuilder.AddIndex(v8);
                            voxelMeshBuilder.AddIndex(v4);
                            PolygonCount += 2;
                        }

                        // The back face is never shown to the camera, so there is no need to create it
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
                            voxelMeshBuilder.AddIndex(v5);
                            voxelMeshBuilder.AddIndex(v1);
                            voxelMeshBuilder.AddIndex(v4);

                            voxelMeshBuilder.AddIndex(v4);
                            voxelMeshBuilder.AddIndex(v8);
                            voxelMeshBuilder.AddIndex(v5);
                            PolygonCount += 2;
                        }

                        if (x == voxelCountX - 1 || !IsVoxelSet(x + 1, y, z))
                        {
                            voxelMeshBuilder.AddIndex(v6);
                            voxelMeshBuilder.AddIndex(v3);
                            voxelMeshBuilder.AddIndex(v2);

                            voxelMeshBuilder.AddIndex(v3);
                            voxelMeshBuilder.AddIndex(v6);
                            voxelMeshBuilder.AddIndex(v7);
                            PolygonCount += 2;
                        }

                        if (y == 0 || !IsVoxelSet(x, y - 1, z))
                        {
                            voxelMeshBuilder.AddIndex(v5);
                            voxelMeshBuilder.AddIndex(v7);
                            voxelMeshBuilder.AddIndex(v6);

                            voxelMeshBuilder.AddIndex(v5);
                            voxelMeshBuilder.AddIndex(v8);
                            voxelMeshBuilder.AddIndex(v7);
                            PolygonCount += 2;
                        }

                        if (y == voxelCountY - 1 || !IsVoxelSet(x, y + 1, z))
                        {
                            voxelMeshBuilder.AddIndex(v1);
                            voxelMeshBuilder.AddIndex(v2);
                            voxelMeshBuilder.AddIndex(v3);

                            voxelMeshBuilder.AddIndex(v1);
                            voxelMeshBuilder.AddIndex(v3);
                            voxelMeshBuilder.AddIndex(v4);
                            PolygonCount += 2;
                        }
                    }
                }
            }

            return voxelMeshBuilder.GetMesh();
        }
    }
}
