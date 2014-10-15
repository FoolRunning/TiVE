using System;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVE.Resources;

namespace ProdigalSoftware.TiVE.Renderer.Voxels
{
    internal class InstancedGameWorldVoxelChunk : GameWorldVoxelChunk
    {
        public InstancedGameWorldVoxelChunk(int chunkX, int chunkY, int chunkZ) : base(chunkX, chunkY, chunkZ)
        {
        }

        #region Overrides of GameWorldVoxelChunk
        protected override string ShaderName
        {
            get { return "MainWorldInstanced"; }
        }

        protected void CreateMesh(MeshBuilder newMeshBuilder, GameWorld gameWorld, 
            int voxelStartX, int voxelEndX, int voxelStartY, int voxelEndY, int voxelStartZ, int voxelEndZ, 
            out int voxelCount, out int renderedVoxelCount, out int polygonCount)
        {
            voxelCount = 0;
            renderedVoxelCount = 0;
            int maxVoxelX = gameWorld.VoxelSize.X;
            int maxVoxelY = gameWorld.VoxelSize.Y;
            int maxVoxelZ = gameWorld.VoxelSize.Z;

            for (int x = voxelStartX; x < voxelEndX; x++)
            {
                int cx = x - voxelStartX;

                for (int z = voxelStartZ; z < voxelEndZ; z++)
                {
                    int cz = z - voxelStartZ;

                    for (int y = voxelStartY; y < voxelEndY; y++)
                    {
                        uint color = gameWorld.GetVoxel(x, y, z);
                        if (color == 0)
                            continue;

                        voxelCount++;

                        if (z == maxVoxelZ - 1 || gameWorld.GetVoxel(x, y, z + 1) == 0 ||
                            x == 0 || gameWorld.GetVoxel(x - 1, y, z) == 0 ||
                            x == maxVoxelX - 1 || gameWorld.GetVoxel(x + 1, y, z) == 0 ||
                            y == 0 || gameWorld.GetVoxel(x, y - 1, z) == 0 ||
                            y == maxVoxelY - 1 || gameWorld.GetVoxel(x, y + 1, z) == 0)
                        {
                            float percentR;
                            float percentG;
                            float percentB;
                            gameWorld.GetLightAt(x, y, z, out percentR, out percentG, out percentB);
                            byte a = (byte)((color >> 24) & 0xFF);
                            byte r = (byte)Math.Min(255, (int)(((color >> 16) & 0xFF) * percentR));
                            byte g = (byte)Math.Min(255, (int)(((color >> 8) & 0xFF) * percentG));
                            byte b = (byte)Math.Min(255, (int)(((color >> 0) & 0xFF) * percentB));

                            newMeshBuilder.Add(cx, y - voxelStartY, cz, r, g, b, a);
                            renderedVoxelCount++;
                        }
                    }
                }
            }
            polygonCount = renderedVoxelCount * 10;
        }

        protected override IVertexDataCollection GetMesh(IRendererData voxelInstanceLocationData)
        {
            return meshBuilder.GetInstanceData(voxelInstanceLocationData);
        }
        #endregion
    }
}
