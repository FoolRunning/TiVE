//#define USE_INDEXED_VOXEL
using OpenTK;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal sealed class InstancedVoxelGroup
    {
        #region Constants
        private const string VertexShaderSource = @"
            #version 150 core 
 
            // premultiplied model to projection transformation
            uniform mat4 matrix_ModelViewProjection;
 
            // incoming vertex information
            in vec3 in_Position;
            in vec4 in_Color;

            // incoming vertex information for each instance
            in vec3 in_InstancePos;
            in vec4 in_InstanceColor;

            flat out vec4 fragment_color;
 
            void main(void)
            {
                fragment_color = in_Color * in_InstanceColor;

                // transforming the incoming vertex position
                gl_Position = matrix_ModelViewProjection * vec4(in_Position + in_InstancePos, 1);
            }";

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

        private readonly uint[, ,] voxels;

        private static IShaderProgram shader;
        private IVertexDataCollection voxelInstances;

        private static readonly int polysPerVoxel;
        private static IRendererData voxelInstanceLocationData;
        private static IRendererData voxelInstanceColorData;
#if USE_INDEXED_VOXEL
        private static IRendererData voxelInstanceIndexData;
#endif

        static InstancedVoxelGroup()
        {
#if USE_INDEXED_VOXEL
            IndexedMeshBuilder voxelInstanceBuilder = new IndexedMeshBuilder();
            IndexedVoxelGroup.AddVoxel(voxelInstanceBuilder, 0, 0, 0, 235, 235, 235, 255, VoxelSides.All);
#else
            MeshBuilder voxelInstanceBuilder = new MeshBuilder();
            polysPerVoxel = VoxelGroup.AddVoxel(voxelInstanceBuilder, 0, 0, 0, 235, 235, 235, 255, VoxelSides.All);
#endif
            voxelInstanceLocationData = voxelInstanceBuilder.GetLocationData();
            voxelInstanceColorData = voxelInstanceBuilder.GetColorData();
#if USE_INDEXED_VOXEL
            voxelInstanceIndexData = voxelInstanceBuilder.GetIndexData();
#endif
        }

        public InstancedVoxelGroup(int sizeX, int sizeY, int sizeZ)
        {
            voxels = new uint[sizeX, sizeY, sizeZ];
        }

        public InstancedVoxelGroup(uint[, ,] voxels)
        {
            this.voxels = voxels;
        }

        public int PolygonCount { get; private set; }

        public int VoxelCount { get; private set; }

        public int RenderedVoxelCount { get; private set; }

        public void SetVoxel(int x, int y, int z, uint newColor)
        {
            voxels[x, y, z] = newColor;
        }

        public void SetVoxels(int startX, int startY, int startZ, uint[, ,] voxelsToInsert)
        {
            for (int x = 0; x <= voxelsToInsert.GetUpperBound(0); x++)
            {
                for (int y = 0; y <= voxelsToInsert.GetUpperBound(1); y++)
                {
                    for (int z = 0; z <= voxelsToInsert.GetUpperBound(2); z++)
                        voxels[startX + x, startY + y, startZ + z] = voxelsToInsert[x, y, z];
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

        public void GenerateMesh()
        {
            if (voxelInstances == null)
                voxelInstances = CreateVoxelMesh();
        }

        public void Render(ref Matrix4 matrixMVP)
        {
            if (voxelInstances == null)
                return;

            voxelInstances.Initialize();

            if (shader == null)
                shader = CreateShader();

            shader.Bind();
            shader.SetUniform("matrix_ModelViewProjection", ref matrixMVP);

            TiVEController.Backend.Draw(PrimitiveType.Triangles, voxelInstances);
        }

        public void Delete()
        {
            if (shader != null)
                shader.Delete();

            if (voxelInstances != null)
                voxelInstances.Delete();

            shader = null;
            voxelInstances = null;
        }

        private static IShaderProgram CreateShader()
        {
            IShaderProgram program = TiVEController.Backend.CreateShaderProgram();
            program.AddShader(VertexShaderSource, ShaderType.Vertex);
            program.AddShader(FragmentShaderSource, ShaderType.Fragment);
            program.AddAttribute("in_Position");
            program.AddAttribute("in_Color");
            program.AddAttribute("in_InstancePos");
            program.AddAttribute("in_InstanceColor");
            program.AddKnownUniform("matrix_ModelViewProjection");

            if (!program.Initialize())
                Messages.AddWarning("Failed to initialize shader program for voxel group");
            return program;
        }

        private IVertexDataCollection CreateVoxelMesh()
        {
            InstancedItemBuilder voxelInstancesBuilder = new InstancedItemBuilder();
            PolygonCount = 0;
            VoxelCount = 0;
            RenderedVoxelCount = 0;

            int voxelCountX = voxels.GetLength(0);
            int voxelCountY = voxels.GetLength(1);
            int voxelCountZ = voxels.GetLength(2);

            for (int z = voxelCountZ - 1; z >= 0; z--) // Order front to back for early z-culling
            //for (int z = 0; z < voxelCountZ; z++)
            {
                for (int x = 0; x < voxelCountX; x++)
                {
                    for (int y = 0; y < voxelCountY; y++)
                    {
                        uint color = voxels[x, y, z];
                        if (color == 0)
                            continue;
                        
                        VoxelCount++;

                        if (z < voxelCountZ - 1 && IsVoxelSet(x, y, z + 1) &&
                            x > 0 && IsVoxelSet(x - 1, y, z) &&
                            x < voxelCountX - 1 && IsVoxelSet(x + 1, y, z) &&
                            y > 0 && IsVoxelSet(x, y - 1, z) &&
                            y < voxelCountY - 1 && IsVoxelSet(x, y + 1, z))
                        {
                            continue; // Voxel is completely covered so no need to process it
                        }

                        RenderedVoxelCount++;

                        byte cr = (byte)((color >> 16) & 0xFF);
                        byte cg = (byte)((color >> 8) & 0xFF);
                        byte cb = (byte)((color >> 0) & 0xFF);
                        byte ca = (byte)((color >> 24) & 0xFF);
                        //Debug.WriteLine(string.Format("Color value: {0} - ({1}, {2}, {3})", color, (int)(color & 0xFF), (int)((color >> 8) & 0xFF), (int)((color >> 16) & 0xFF)));

                        voxelInstancesBuilder.AddInstance(x, y, z, cr, cg, cb, ca);
                        PolygonCount += polysPerVoxel;
                    }
                }
            }

            return voxelInstancesBuilder.GetInstanceData(voxelInstanceLocationData, voxelInstanceColorData
#if USE_INDEXED_VOXEL
                , voxelInstanceIndexData
#endif
                );
        }
    }
}
