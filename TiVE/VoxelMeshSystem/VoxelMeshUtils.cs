﻿using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.VoxelMeshSystem
{
    internal static class VoxelMeshUtils
    {
        public static void GenerateMesh(Voxel[,,] voxels, MeshBuilder meshBuilder, bool forInstances, 
            out int voxelCount, out int renderedVoxelCount, out int polygonCount)
        {
            voxelCount = 0;
            renderedVoxelCount = 0;
            polygonCount = 0;

            VoxelMeshHelper meshHelper = VoxelMeshHelper.Get(forInstances);
            meshBuilder.StartNewMesh();
            int xSize = voxels.GetLength(0);
            int ySize = voxels.GetLength(1);
            int zSize = voxels.GetLength(2);
            for (byte z = 0; z < zSize; z++)
            {
                for (byte x = 0; x < xSize; x++)
                {
                    for (byte y = 0; y < ySize; y++)
                    {
                        Voxel color = voxels[x, y, z];
                        if (color == 0)
                            continue;

                        voxelCount++;

                        VoxelSides sides = VoxelSides.None;
                        //if (z == 0 || voxels[x, y, z - 1] == 0) // The back face is never shown to the camera, so there is no need to create it
                        //    sides |= VoxelSides.Back;
                        if (z == zSize - 1 || voxels[x, y, z + 1] == 0)
                            sides |= VoxelSides.Front;
                        if (x == 0 || voxels[x - 1, y, z] == 0)
                            sides |= VoxelSides.Left;
                        if (x == xSize - 1 || voxels[x + 1, y, z] == 0)
                            sides |= VoxelSides.Right;
                        if (y == 0 || voxels[x, y - 1, z] == 0)
                            sides |= VoxelSides.Bottom;
                        if (y == ySize - 1 || voxels[x, y + 1, z] == 0) // Top face is never shown to the camera
                            sides |= VoxelSides.Top;

                        if (sides != VoxelSides.None)
                        {
                            polygonCount += meshHelper.AddVoxel(meshBuilder, sides, x, y, z, (Color4b)color);
                            renderedVoxelCount++;
                        }
                    }
                }
            }
        }
    }
}