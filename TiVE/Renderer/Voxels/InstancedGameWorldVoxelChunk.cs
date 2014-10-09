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

        protected override void CreateMesh(MeshBuilder newMeshBuilder, GameWorld gameWorld, 
            int worldVoxelStartX, int worldVoxelEndX, int worldVoxelStartY, int worldVoxelEndY, int worldVoxelStartZ, int worldVoxelEndZ, 
            out int voxelCount, out int renderedVoxelCount, out int polygonCount)
        {
            voxelCount = 0;
            renderedVoxelCount = 0;
            int maxVoxelX = gameWorld.VoxelSize.X;
            int maxVoxelY = gameWorld.VoxelSize.Y;
            int maxVoxelZ = gameWorld.VoxelSize.Z;

            for (int x = worldVoxelStartX; x < worldVoxelEndX; x++)
            {
                int cx = x - worldVoxelStartX;

                for (int z = worldVoxelStartZ; z < worldVoxelEndZ; z++)
                {
                    int cz = z - worldVoxelStartZ;

                    for (int y = worldVoxelStartY; y < worldVoxelEndY; y++)
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

                            newMeshBuilder.Add(cx, y - worldVoxelStartY, cz, r, g, b, a);
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
