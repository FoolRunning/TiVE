﻿using ProdigalSoftware.TiVEPluginFramework;

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
                            // ENHANCE calculate ambient occlusion for the mesh

                            meshBuilder.AddVoxel(sides, x, y, z, (Color4b)vox, GetVoxelNormal(sides), 255);
                            renderedVoxelCount++;
                        }
                    }
                }
            }
        }

        public static Vector3f GetVoxelNormal(CubeSides visibleSides)
        {
            if (visibleSides == CubeSides.All)
                return Vector3f.Zero;

            Vector3f vector = new Vector3f();
            if ((visibleSides & CubeSides.XMinus) != 0)
                vector.X -= 1.0f;
            if ((visibleSides & CubeSides.YMinus) != 0)
                vector.Y -= 1.0f;
            if ((visibleSides & CubeSides.ZMinus) != 0)
                vector.Z -= 1.0f;

            if ((visibleSides & CubeSides.XPlus) != 0)
                vector.X += 1.0f;
            if ((visibleSides & CubeSides.YPlus) != 0)
                vector.Y += 1.0f;
            if ((visibleSides & CubeSides.ZPlus) != 0)
                vector.Z += 1.0f;

            vector.NormalizeFast();
            return vector;
        }

        public static bool IsVoxelNormalUndefined(CubeSides visibleSides)
        {
            if (visibleSides == CubeSides.All)
                return true;

            int x = 0;
            int y = 0;
            int z = 0;
            if ((visibleSides & CubeSides.XMinus) != 0)
                x--;
            if ((visibleSides & CubeSides.YMinus) != 0)
                y--;
            if ((visibleSides & CubeSides.ZMinus) != 0)
                z--;

            if ((visibleSides & CubeSides.XPlus) != 0)
                x++;
            if ((visibleSides & CubeSides.YPlus) != 0)
                y++;
            if ((visibleSides & CubeSides.ZPlus) != 0)
                z++;

            return x == 0 && y == 0 && z == 0;
        }
    }
}
