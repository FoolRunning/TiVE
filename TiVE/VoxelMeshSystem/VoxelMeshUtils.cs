using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Internal;

namespace ProdigalSoftware.TiVE.VoxelMeshSystem
{
    internal static class VoxelMeshUtils
    {
        public static void GenerateMesh(VoxelSprite sprite, MeshBuilder meshBuilder, out int voxelCount, out int renderedVoxelCount)
        {
            voxelCount = 0;
            renderedVoxelCount = 0;

            meshBuilder.StartNewMesh();
            int xSize = sprite.Size.X;
            int ySize = sprite.Size.Y;
            int zSize = sprite.Size.Z;
            for (byte z = 0; z < zSize; z++)
            {
                for (byte x = 0; x < xSize; x++)
                {
                    for (byte y = 0; y < ySize; y++)
                    {
                        Voxel vox = sprite[x, y, z];
                        if (vox == Voxel.Empty)
                            continue;

                        voxelCount++;

                        CubeSides sides = CubeSides.None;
                        if (z == 0 || sprite[x, y, z - 1] == Voxel.Empty)
                            sides |= CubeSides.ZMinus;
                        if (z == zSize - 1 || sprite[x, y, z + 1] == Voxel.Empty)
                            sides |= CubeSides.ZPlus;
                        if (x == 0 || sprite[x - 1, y, z] == Voxel.Empty)
                            sides |= CubeSides.XMinus;
                        if (x == xSize - 1 || sprite[x + 1, y, z] == Voxel.Empty)
                            sides |= CubeSides.XPlus;
                        if (y == 0 || sprite[x, y - 1, z] == Voxel.Empty)
                            sides |= CubeSides.YMinus;
                        if (y == ySize - 1 || sprite[x, y + 1, z] == Voxel.Empty)
                            sides |= CubeSides.YPlus;

                        if (sides != CubeSides.None)
                        {
                            meshBuilder.AddVoxel(sides, x, y, z, (Color4b)vox);
                            renderedVoxelCount++;
                        }
                    }
                }
            }
        }
    }
}
