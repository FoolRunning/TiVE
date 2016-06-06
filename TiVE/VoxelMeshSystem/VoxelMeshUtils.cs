using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.VoxelMeshSystem
{
    internal static class VoxelMeshUtils
    {
        public static void GenerateMesh(VoxelSprite sprite, MeshBuilder meshBuilder, out int voxelCount, out int renderedVoxelCount, out int polygonCount)
        {
            voxelCount = 0;
            renderedVoxelCount = 0;
            polygonCount = 0;

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

                        VoxelSides sides = VoxelSides.None;
                        if (z == 0 || sprite[x, y, z - 1] == Voxel.Empty)
                            sides |= VoxelSides.Back;
                        if (z == zSize - 1 || sprite[x, y, z + 1] == Voxel.Empty)
                            sides |= VoxelSides.Front;
                        if (x == 0 || sprite[x - 1, y, z] == Voxel.Empty)
                            sides |= VoxelSides.Left;
                        if (x == xSize - 1 || sprite[x + 1, y, z] == Voxel.Empty)
                            sides |= VoxelSides.Right;
                        if (y == 0 || sprite[x, y - 1, z] == Voxel.Empty)
                            sides |= VoxelSides.Bottom;
                        if (y == ySize - 1 || sprite[x, y + 1, z] == Voxel.Empty)
                            sides |= VoxelSides.Top;

                        if (sides != VoxelSides.None)
                        {
                            polygonCount += meshBuilder.AddVoxel(sides, x, y, z, (Color4b)vox);
                            renderedVoxelCount++;
                        }
                    }
                }
            }
        }
    }
}
