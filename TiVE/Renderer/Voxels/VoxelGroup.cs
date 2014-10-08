//#define USE_JAGGED_ARRAYS
using System;
using System.Diagnostics;
using OpenTK;

namespace ProdigalSoftware.TiVE.Renderer.Voxels
{
    [Flags]
    public enum VoxelSides
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

#if USE_JAGGED_ARRAYS
        private readonly uint[][][] voxels;
#else
        private readonly uint[, ,] voxels;
#endif

        private static IShaderProgram shader;
        private IVertexDataCollection mesh;

        protected VoxelGroup(int sizeX, int sizeY, int sizeZ)
        {
#if USE_JAGGED_ARRAYS
            voxels = new uint[sizeX][][];
            for (int x = 0; x < sizeX; x++)
            {
                voxels[x] = new uint[sizeY][];
                for (int y = 0; y < sizeY; y++)
                    voxels[x][y] = new uint[sizeZ];
            }
#else
            voxels = new uint[sizeX, sizeY, sizeZ];
#endif
        }

        protected VoxelGroup(uint[, ,] voxels)
        {
#if USE_JAGGED_ARRAYS
#else
            this.voxels = voxels;
#endif
        }

        public void Dispose()
        {
            //if (shader != null)
            //    shader.Delete();

            if (mesh != null)
                mesh.Dispose();

            mesh = null;
        }

        public int PolygonCount { get; private set; }

        public int VoxelCount { get; private set; }

        public int RenderedVoxelCount { get; private set; }

        public void SetVoxel(int x, int y, int z, uint newColor)
        {
#if USE_JAGGED_ARRAYS
            voxels[x][y][z] = newColor;
#else
            voxels[x, y, z] = newColor;
#endif
        }

        public void SetVoxels(int startX, int startY, int startZ, uint[, ,] voxelsToInsert)
        {
#if USE_JAGGED_ARRAYS
            uint[][][] voxelData = voxels;
#else
            uint[, ,] voxelData = voxels;
#endif
            int voxelCountX = voxelsToInsert.GetLength(0);
            int voxelCountY = voxelsToInsert.GetLength(1);
            int voxelCountZ = voxelsToInsert.GetLength(2);

            for (int x = 0; x < voxelCountX; x++)
            {
                for (int y = 0; y < voxelCountY; y++)
                {
                    for (int z = 0; z < voxelCountZ; z++)
                    {
#if USE_JAGGED_ARRAYS
                        voxelData[startX + x][startY + y][startZ + z] = voxelsToInsert[x, y, z];
#else
                        voxelData[startX + x, startY + y, startZ + z] = voxelsToInsert[x, y, z];
#endif
                    }
                }
            }
        }

        public void DetermineVoxelVisibility()
        {
#if USE_JAGGED_ARRAYS
            uint[][][] voxelData = voxels;
            int voxelCountX = voxelData.Length;
            int voxelCountY = voxelData[0].Length;
            int voxelCountZ = voxelData[0][0].Length;
            for (int z = 0; z < voxelCountZ; z++)
            {
                for (int x = 0; x < voxelCountX; x++)
                {
                    for (int y = 0; y < voxelCountY; y++)
                    {
                        uint color = voxelData[x][y][z];
                        if (color == 0)
                            continue;

                        VoxelSides sides = VoxelSides.None;

                        if (z == voxelCountZ - 1 || voxelData[x][y][z + 1] == 0U)
                            sides |= VoxelSides.Front;
                        //if (!IsZLineSet(x, y, z, 1)) // The back face is never shown to the camera, so there is no need to create it
                        //    sizes |= VoxelSides.Back;
                        if (x == 0 || voxelData[x - 1][y][z] == 0U)
                            sides |= VoxelSides.Left;
                        if (x == voxelCountX - 1 || voxelData[x + 1][y][z] == 0U)
                            sides |= VoxelSides.Right;
                        if (y == 0 || voxelData[x][y - 1][z] == 0U)
                            sides |= VoxelSides.Bottom;
                        if (y == voxelCountY - 1 || voxelData[x][y + 1][z] == 0U)
                            sides |= VoxelSides.Top;

                        voxelData[x][y][z] = ((color & 0xFFFFFF) | (uint)((int)sides << 26));
                    }
                }
            }
#else
            uint[, ,] voxelData = voxels;
            int voxelCountX = voxelData.GetLength(0);
            int voxelCountY = voxelData.GetLength(1);
            int voxelCountZ = voxelData.GetLength(2);
            for (int z = 0; z < voxelCountZ; z++)
            {
                for (int x = 0; x < voxelCountX; x++)
                {
                    for (int y = 0; y < voxelCountY; y++)
                    {
                        uint color = voxelData[x, y, z];
                        if (color == 0)
                            continue;

                        VoxelSides sides = VoxelSides.None;

                        if (z == voxelCountZ - 1 || voxelData[x, y, z + 1] == 0)
                            sides |= VoxelSides.Front;
                        //if (!IsZLineSet(x, y, z, 1)) // The back face is never shown to the camera, so there is no need to create it
                        //    sizes |= VoxelSides.Back;
                        if (x == 0 || voxelData[x - 1, y, z] == 0)
                            sides |= VoxelSides.Left;
                        if (x == voxelCountX - 1 || voxelData[x + 1, y, z] == 0)
                            sides |= VoxelSides.Right;
                        if (y == 0 || voxelData[x, y - 1, z] == 0)
                            sides |= VoxelSides.Bottom;
                        if (y == voxelCountY - 1 || voxelData[x, y + 1, z] == 0)
                            sides |= VoxelSides.Top;

                        voxelData[x, y, z] = ((color & 0xFFFFFF) | (uint)((int)sides << 26));
                    }
                }
            }
#endif
        }

        public void ClearVoxel(int x, int y, int z)
        {
#if USE_JAGGED_ARRAYS
            voxels[x][y][z] = 0;
#else
            voxels[x, y, z] = 0;
#endif
        }

        public bool IsVoxelSet(int x, int y, int z)
        {
#if USE_JAGGED_ARRAYS
            return voxels[x][y][z] != 0;
#else
            return voxels[x, y, z] != 0;
#endif
        }

        public void GenerateMesh(MeshBuilder meshBuilder)
        {
            Debug.Assert(mesh == null, "Should not create a mesh for a voxel group more then once");
            mesh = CreateVoxelMesh(meshBuilder);
        }

        public void Initialize()
        {
            if (mesh != null)
                mesh.Initialize();
        }

        public void Render(ref Matrix4 matrixMVP)
        {
            if (mesh == null)
                return;
            mesh.Initialize();

            if (shader == null)
                shader = CreateShader();

            shader.Bind();
            shader.SetUniform("matrix_ModelViewProjection", ref matrixMVP);

            TiVEController.Backend.Draw(PrimitiveType.Triangles, mesh);
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

        private IVertexDataCollection CreateVoxelMesh(MeshBuilder meshBuilder)
        {
            int voxelCount = 0;
            int renderedVoxelCount = 0;
            int polygonCount = 0;

#if USE_JAGGED_ARRAYS
            uint[][][] voxelData = voxels;
            int voxelCountX = voxelData.Length;
            int voxelCountY = voxelData[0].Length;
            int voxelCountZ = voxelData[0][0].Length;
#else
            uint[, ,] voxelData = voxels;
            int voxelCountX = voxelData.GetLength(0);
            int voxelCountY = voxelData.GetLength(1);
            int voxelCountZ = voxelData.GetLength(2);
#endif
            Stopwatch sw = new Stopwatch();
            sw.Start();
            meshBuilder.StartNewMesh();
            for (int z = 0; z < voxelCountZ; z++)
            {
                for (int x = 0; x < voxelCountX; x++)
                {
                    for (int y = 0; y < voxelCountY; y++)
                    {
#if USE_JAGGED_ARRAYS
                        uint color = voxelData[x][y][z];
#else
                        uint color = voxelData[x, y, z];
#endif
                        if (color == 0)
                            continue;

                        voxelCount++;

                        VoxelSides sides = (VoxelSides)((color & 0xFC000000) >> 26);
                        if (sides != VoxelSides.None)
                        {
                            polygonCount += AddVoxel(meshBuilder, sides, x, y, z, 
                                (byte)((color >> 16) & 0xFF), (byte)((color >> 8) & 0xFF), (byte)((color >> 0) & 0xFF), 255 /*(byte)((color >> 24) & 0xFF)*/);
                            renderedVoxelCount++;
                        }
                    }
                }
            }
            sw.Stop();
            //Debug.WriteLine("Create loop: " + sw.ElapsedTicks * 1000.0f / Stopwatch.Frequency + "ms");

            PolygonCount = polygonCount;
            VoxelCount = voxelCount;
            RenderedVoxelCount = renderedVoxelCount;
            sw.Restart();
            IVertexDataCollection meshData = meshBuilder.GetMesh();
            sw.Stop();
            //Debug.WriteLine("Getting mesh data: " + sw.ElapsedTicks * 1000.0f / Stopwatch.Frequency + "ms");
            return meshData;
        }

        protected abstract int AddVoxel(MeshBuilder meshBuilder, VoxelSides sides, int x, int y, int z, byte cr, byte cg, byte cb, byte ca);
    }
}
