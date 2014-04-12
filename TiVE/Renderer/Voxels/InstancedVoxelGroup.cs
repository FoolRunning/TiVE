//#define USE_INDEXED_VOXEL

using System;
using OpenTK;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE.Renderer.Voxels
{
    internal sealed class InstancedVoxelGroup : IDisposable
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
            polysPerVoxel = IndexedVoxelGroup.AddVoxel(voxelInstanceBuilder, 0, 0, 0, 235, 235, 235, 255, VoxelSides.All);
#else
            MeshBuilder voxelInstanceBuilder = new MeshBuilder(10, 0);
            //polysPerVoxel = VoxelGroup.AddVoxel(voxelInstanceBuilder, 0, 0, 0, 235, 235, 235, 255, VoxelSides.All);
            polysPerVoxel = CreateVoxel(voxelInstanceBuilder, 235, 235, 235, 255);
#endif
            voxelInstanceLocationData = voxelInstanceBuilder.GetLocationData();
            voxelInstanceLocationData.Lock();
            voxelInstanceColorData = voxelInstanceBuilder.GetColorData();
            voxelInstanceColorData.Lock();
#if USE_INDEXED_VOXEL
            voxelInstanceIndexData = voxelInstanceBuilder.GetIndexData();
            voxelInstanceIndexData.Lock();
#endif
        }

        private static int CreateVoxel(MeshBuilder meshBuilder, byte cr, byte cg, byte cb, byte ca)
        {
            int polygonCount = 0;
            meshBuilder.Add(0, 0, 1, cr, cg, cb, ca);
            meshBuilder.Add(1, 1, 1, cr, cg, cb, ca);
            meshBuilder.Add(1, 0, 1, cr, cg, cb, ca);

            meshBuilder.Add(1, 1, 1, cr, cg, cb, ca);
            meshBuilder.Add(0, 0, 1, cr, cg, cb, ca);
            meshBuilder.Add(0, 1, 1, cr, cg, cb, ca);
            polygonCount += 2;

            return polygonCount;
        }


        public InstancedVoxelGroup(int sizeX, int sizeY, int sizeZ)
        {
            voxels = new uint[sizeX, sizeY, sizeZ];
        }

        public InstancedVoxelGroup(uint[, ,] voxels)
        {
            this.voxels = voxels;
        }

        public void Dispose()
        {
            //if (shader != null)
            //    shader.Delete();

            if (voxelInstances != null)
                voxelInstances.Dispose();

            //shader = null;
            voxelInstances = null;
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
            MeshBuilder voxelInstancesBuilder = new MeshBuilder(5000, 0);
            int voxelCount = 0;
            int renderedVoxelCount = 0;
            uint[,,] voxelData = voxels;

            int voxelCountX = voxelData.GetLength(0);
            int voxelCountY = voxelData.GetLength(1);
            int voxelCountZ = voxelData.GetLength(2);

            for (int z = voxelCountZ - 1; z >= 0; z--) // Order front to back for early z-culling
            {
                for (int x = 0; x < voxelCountX; x++)
                {
                    for (int y = 0; y < voxelCountY; y++)
                    {
                        uint color = voxelData[x, y, z];
                        if (color == 0)
                            continue;
                        
                        voxelCount++;

                        if (z < voxelCountZ - 1 && voxelData[x, y, z + 1] != 0 &&
                            x > 0 && voxelData[x - 1, y, z] != 0 &&
                            x < voxelCountX - 1 && voxelData[x + 1, y, z] != 0 &&
                            y > 0 && voxelData[x, y - 1, z] != 0 &&
                            y < voxelCountY - 1 && voxelData[x, y + 1, z] != 0)
                        {
                            continue; // Voxel is completely covered so no need to process it
                        }

                        renderedVoxelCount++;

                        byte cr = (byte)((color >> 16) & 0xFF);
                        byte cg = (byte)((color >> 8) & 0xFF);
                        byte cb = (byte)((color >> 0) & 0xFF);
                        byte ca = (byte)((color >> 24) & 0xFF);
                        //Debug.WriteLine(string.Format("Color value: {0} - ({1}, {2}, {3})", color, (int)(color & 0xFF), (int)((color >> 8) & 0xFF), (int)((color >> 16) & 0xFF)));

                        voxelInstancesBuilder.Add(x, y, z, cr, cg, cb, ca);
                    }
                }

            }

            VoxelCount = voxelCount;
            RenderedVoxelCount = renderedVoxelCount;
            PolygonCount = renderedVoxelCount * polysPerVoxel;

            return voxelInstancesBuilder.GetInstanceData(voxelInstanceLocationData, voxelInstanceColorData
#if USE_INDEXED_VOXEL
                , voxelInstanceIndexData
#endif
                );
        }
    }
}
