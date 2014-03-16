using System;
using System.Diagnostics;
using OpenTK;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal class IndexedVoxelGroup
    {
        #region Constants
        private const int SmallColorDiff = 10;
        private const int BigColorDiff = 20;

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

        private static readonly IndexedMeshBuilder voxelMeshBuilder = new IndexedMeshBuilder();

        private readonly uint[, ,] voxels;

        private IShaderProgram shader;
        private IVertexDataCollection mesh;

        public IndexedVoxelGroup(int sizeX, int sizeY, int sizeZ)
        {
            voxels = new uint[sizeX, sizeY, sizeZ];
        }

        public IndexedVoxelGroup(uint[,,] voxels)
        {
            this.voxels = voxels;
        }

        ~IndexedVoxelGroup()
        {
            Debug.Assert(shader == null);
            Debug.Assert(mesh == null);
        }

        public int PolygonCount { get; private set; }

        public void SetVoxel(int x, int y, int z, uint newColor)
        {
            voxels[x, y, z] = newColor;
        }

        public void SetVoxels(int startX, int startY, int startZ, uint[,,] voxelsToInsert)
        {
            for (int x = 0; x <= voxelsToInsert.GetUpperBound(0); x++)
            {
                for (int y = 0; y <= voxelsToInsert.GetUpperBound(1); y++)
                {
                    for (int z = 0; z <= voxelsToInsert.GetUpperBound(2); z++)
                    {
                        voxels[startX + x, startY + y, startZ + z] = voxelsToInsert[x, y, z];
                    }
                }
            }
        }

        public void ClearVoxel(int x, int y, int z)
        {
            voxels[x, y, z] = 0;
        }

        public bool IsVoxelSet(int x, int y, int z)
        {
            return voxels[x, y, z] != 0;
        }

        public void Render(ref Matrix4 matrixMVP)
        {
            if (mesh == null)
            {
                mesh = CreateVoxelMesh();
                mesh.Initialize();
            }

            if (shader == null)
                shader = CreateShader();

            shader.Bind();
            shader.SetUniform("matrix_ModelViewProjection", ref matrixMVP);

            TiVEController.Backend.Draw(PrimitiveType.Triangles, mesh);
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

        private static IShaderProgram CreateShader()
        {
            IShaderProgram program = TiVEController.Backend.CreateShaderProgram();
            program.AddShader(VertexShaderSource, ShaderType.Vertex);
            program.AddShader(FragmentShaderSource, ShaderType.Fragment);
            program.AddAttribute("in_Position");
            program.AddAttribute("in_Color");
            program.AddKnownUniform("matrix_ModelViewProjection");

            if (!program.Initialize())
                Debug.WriteLine("Failed to initialize shader");
            return program;
        }

        private IVertexDataCollection CreateVoxelMesh()
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

                        VoxelSides sides = VoxelSides.None;
                        if (z == voxelCountZ - 1 || !IsVoxelSet(x, y, z + 1))
                            sides |= VoxelSides.Front;

                        // The back face is never shown to the camera, so there is no need to create it
                        //if (!IsZLineSet(x, y, z, 1))
                        //    sizes |= VoxelSides.Back;

                        if (x == 0 || !IsVoxelSet(x - 1, y, z))
                            sides |= VoxelSides.Left;

                        if (x == voxelCountX - 1 || !IsVoxelSet(x + 1, y, z))
                            sides |= VoxelSides.Right;

                        if (y == 0 || !IsVoxelSet(x, y - 1, z))
                            sides |= VoxelSides.Bottom;

                        if (y == voxelCountY - 1 || !IsVoxelSet(x, y + 1, z))
                            sides |= VoxelSides.Top;

                        PolygonCount += AddVoxel(voxelMeshBuilder, x, y, z, cr, cg, cb, ca, sides);
                    }
                }
            }

            return voxelMeshBuilder.GetMesh();
        }

        public static int AddVoxel(IndexedMeshBuilder meshBuilder, int x, int y, int z, byte cr, byte cg, byte cb, byte ca, VoxelSides sides)
        {
            int polygonCount = 0;
            if ((sides & VoxelSides.Front) != 0)
            {
                byte v3 = meshBuilder.AddVertex(x + 1, y + 1, z + 1, cr, cg, cb, ca);
                byte v4 = meshBuilder.AddVertex(x, y + 1, z + 1, cr, cg, cb, ca);
                byte v7 = meshBuilder.AddVertex(x + 1, y, z + 1, cr, cg, cb, ca);
                byte v8 = meshBuilder.AddVertex(x, y, z + 1, cr, cg, cb, ca);

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
                byte v1 = meshBuilder.AddVertex(x, y + 1, z, crr, cgr, cbr, ca);
                byte v4 = meshBuilder.AddVertex(x, y + 1, z + 1, crr, cgr, cbr, ca);
                byte v5 = meshBuilder.AddVertex(x, y, z, crr, cgr, cbr, ca);
                byte v8 = meshBuilder.AddVertex(x, y, z + 1, crr, cgr, cbr, ca);

                meshBuilder.AddIndex(v5);
                meshBuilder.AddIndex(v1);
                meshBuilder.AddIndex(v4);

                meshBuilder.AddIndex(v4);
                meshBuilder.AddIndex(v8);
                meshBuilder.AddIndex(v5);
                polygonCount += 2;
            }

            if ((sides & VoxelSides.Left) != 0)
            {
                byte crl = (byte)Math.Max(0, cr - SmallColorDiff);
                byte cgl = (byte)Math.Max(0, cg - SmallColorDiff);
                byte cbl = (byte)Math.Max(0, cb - SmallColorDiff);
                byte v2 = meshBuilder.AddVertex(x + 1, y + 1, z, crl, cgl, cbl, ca);
                byte v3 = meshBuilder.AddVertex(x + 1, y + 1, z + 1, crl, cgl, cbl, ca);
                byte v6 = meshBuilder.AddVertex(x + 1, y, z, crl, cgl, cbl, ca);
                byte v7 = meshBuilder.AddVertex(x + 1, y, z + 1, crl, cgl, cbl, ca);

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
                byte v5 = meshBuilder.AddVertex(x, y, z, crb, cgb, cbb, ca);
                byte v6 = meshBuilder.AddVertex(x + 1, y, z, crb, cgb, cbb, ca);
                byte v7 = meshBuilder.AddVertex(x + 1, y, z + 1, crb, cgb, cbb, ca);
                byte v8 = meshBuilder.AddVertex(x, y, z + 1, crb, cgb, cbb, ca);

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
                byte v1 = meshBuilder.AddVertex(x, y + 1, z, crt, cgt, cbt, ca);
                byte v2 = meshBuilder.AddVertex(x + 1, y + 1, z, crt, cgt, cbt, ca);
                byte v3 = meshBuilder.AddVertex(x + 1, y + 1, z + 1, crt, cgt, cbt, ca);
                byte v4 = meshBuilder.AddVertex(x, y + 1, z + 1, crt, cgt, cbt, ca);

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
