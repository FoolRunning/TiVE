﻿using System;
using System.Diagnostics;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.Lighting;
using ProdigalSoftware.TiVE.Renderer.Meshes;
using ProdigalSoftware.TiVE.Renderer.Voxels;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    internal sealed class GameWorldVoxelChunk : IDisposable
    {
        #region Constants
        public const int BlockSize = 4;
        public const int VoxelSize = BlockSize * BlockInformation.VoxelSize;
        #endregion

        #region Member variables
        private readonly object syncLock = new object();
        private readonly Vector3i chunkLoc;
        private readonly Vector3i chunkBlockLoc;
        private Matrix4 translationMatrix;
        private bool deleted;

        private int chunkPolygonCount;
        private int chunkVoxelCount;
        private int chunkRenderedVoxelCount;
        private MeshBuilder meshBuilder;
        private IVertexDataCollection mesh;
        #endregion

        public GameWorldVoxelChunk(int chunkX, int chunkY, int chunkZ)
        {
            chunkLoc = new Vector3i(chunkX, chunkY, chunkZ);
            chunkBlockLoc = new Vector3i(chunkX * BlockSize, chunkY * BlockSize, chunkZ * BlockSize);
            translationMatrix = Matrix4.CreateTranslation(chunkX * VoxelSize, chunkY * VoxelSize, chunkZ * VoxelSize);
        }

        public void Dispose()
        {
            using (new PerformanceLock(syncLock))
            {
                if (meshBuilder != null)
                    meshBuilder.DropMesh();

                if (mesh != null)
                    mesh.Dispose();

                meshBuilder = null;
                mesh = null;
                deleted = true;
            }
        }

        public Vector3i ChunkBlockLocation
        {
            get { return chunkBlockLoc; }
        }

        public bool IsDeleted
        {
            get
            {
                using (new PerformanceLock(syncLock))
                    return deleted;
            }
        }

        public void PrepareForLoad()
        {
            using (new PerformanceLock(syncLock))
                deleted = false;
        }

        public RenderStatistics Render(ShaderManager shaderManager, ref Matrix4 viewProjectionMatrix)
        {
            if (chunkPolygonCount == 0)
                return new RenderStatistics(); // Nothing to render for this chunk

            IVertexDataCollection meshData;
            using (new PerformanceLock(syncLock))
            {
                if (meshBuilder != null)
                    InitializeNewMesh();
                meshData = mesh;
            }

            if (meshData == null)
                return new RenderStatistics(); // Nothing to render for this chunk

            Debug.Assert(meshData.IsInitialized);

            IShaderProgram shader = shaderManager.GetShaderProgram(VoxelMeshHelper.Get(false).ShaderName);
            shader.Bind();

            Matrix4 viewProjectionModelMatrix;
            Matrix4.Mult(ref translationMatrix, ref viewProjectionMatrix, out viewProjectionModelMatrix);
            shader.SetUniform("matrix_ModelViewProjection", ref viewProjectionModelMatrix);

            TiVEController.Backend.Draw(PrimitiveType.Triangles, meshData);
            return new RenderStatistics(1, chunkPolygonCount, chunkVoxelCount, chunkRenderedVoxelCount);
        }

        public override string ToString()
        {
            return string.Format("Chunk ({0}, {1}, {2}) {3}v", chunkLoc.X, chunkLoc.Y, chunkLoc.Z, chunkVoxelCount);
        }

        public void Load(MeshBuilder newMeshBuilder, IGameWorldRenderer renderer)
        {
            Debug.Assert(newMeshBuilder.IsLocked);
            //Debug.WriteLine("Started chunk ({0},{1},{2})", chunkStartX, chunkStartY, chunkStartZ);

            GameWorld gameWorld = renderer.GameWorld;
            LightProvider lightProvider = renderer.LightProvider;
            VoxelMeshHelper meshHelper = VoxelMeshHelper.Get(false);

            int voxelStartX = chunkLoc.X * BlockSize * BlockInformation.VoxelSize;
            int voxelStartY = chunkLoc.Y * BlockSize * BlockInformation.VoxelSize;
            int voxelStartZ = chunkLoc.Z * BlockSize * BlockInformation.VoxelSize;

            int blockStartX = chunkLoc.X * BlockSize;
            int blockEndX = Math.Min((chunkLoc.X + 1) * BlockSize, gameWorld.BlockSize.X);
            int blockStartY = chunkLoc.Y * BlockSize;
            int blockEndY = Math.Min((chunkLoc.Y + 1) * BlockSize, gameWorld.BlockSize.Y);
            int blockStartZ = chunkLoc.Z * BlockSize;
            int blockEndZ = Math.Min((chunkLoc.Z + 1) * BlockSize, gameWorld.BlockSize.Z);

            int maxVoxelX = gameWorld.VoxelSize.X - 1;
            int maxVoxelY = gameWorld.VoxelSize.Y - 1;
            int maxVoxelZ = gameWorld.VoxelSize.Z - 1;

            int voxelCount = 0;
            int renderedVoxelCount = 0;
            int polygonCount = 0;

            for (int blockZ = blockEndZ - 1; blockZ >= blockStartZ; blockZ--)
            {
                for (int blockX = blockStartX; blockX < blockEndX; blockX++)
                {
                    for (int blockY = blockStartY; blockY < blockEndY; blockY++)
                    {
                        BlockInformation block = gameWorld[blockX, blockY, blockZ];
                        if (block == BlockInformation.Empty || block.NextBlock != null)
                            continue;

                        for (int bz = BlockInformation.VoxelSize - 1; bz >= 0; bz--)
                        {
                            int voxelZ = blockZ * BlockInformation.VoxelSize + bz;
                            byte chunkVoxelZ = (byte)(voxelZ - voxelStartZ);
                            for (int bx = 0; bx < BlockInformation.VoxelSize; bx++)
                            {
                                int voxelX = blockX * BlockInformation.VoxelSize + bx;
                                byte chunkVoxelX = (byte)(voxelX - voxelStartX);
                                for (int by = 0; by < BlockInformation.VoxelSize; by++)
                                {
                                    uint color = block[bx, by, bz];
                                    if (color == 0)
                                        continue;
                                    
                                    int voxelY = blockY * BlockInformation.VoxelSize + by;

                                    VoxelSides sides = VoxelSides.None;

                                    // Check to see if the front side is visible
                                    if (bz < BlockInformation.VoxelSize - 1)
                                    {
                                        if (block[bx, by, bz + 1] == 0)
                                            sides |= VoxelSides.Front;
                                    }
                                    else if (voxelZ < maxVoxelZ && gameWorld.GetVoxel(voxelX, voxelY, voxelZ + 1) == 0)
                                        sides |= VoxelSides.Front;

                                    // Check to see if the back side is visible
                                    if (bz > 0)
                                    {
                                        if (block[bx, by, bz - 1] == 0)
                                            sides |= VoxelSides.Back;
                                    }
                                    else if (voxelZ > 0 && gameWorld.GetVoxel(voxelX, voxelY, voxelZ - 1) == 0)
                                        sides |= VoxelSides.Back;

                                    // Check to see if the left side is visible
                                    if (bx > 0)
                                    {
                                        if (block[bx - 1, by, bz] == 0)
                                            sides |= VoxelSides.Left;
                                    }
                                    else if (voxelX > 0 && gameWorld.GetVoxel(voxelX - 1, voxelY, voxelZ) == 0)
                                        sides |= VoxelSides.Left;

                                    // Check to see if the right side is visible
                                    if (bx < BlockInformation.VoxelSize - 1)
                                    {
                                        if (block[bx + 1, by, bz] == 0)
                                            sides |= VoxelSides.Right;
                                    }
                                    else if (voxelX < maxVoxelX && gameWorld.GetVoxel(voxelX + 1, voxelY, voxelZ) == 0)
                                        sides |= VoxelSides.Right;

                                    // Check to see if the bottom side is visible
                                    if (by > 0)
                                    {
                                        if (block[bx, by - 1, bz] == 0)
                                            sides |= VoxelSides.Bottom;
                                    }
                                    else if (voxelY > 0 && gameWorld.GetVoxel(voxelX, voxelY - 1, voxelZ) == 0)
                                        sides |= VoxelSides.Bottom;

                                    // Check to see if the top side is visible
                                    if (by < BlockInformation.VoxelSize - 1)
                                    {
                                        if (block[bx, by + 1, bz] == 0)
                                            sides |= VoxelSides.Top;
                                    }
                                    else if (voxelY < maxVoxelY && gameWorld.GetVoxel(voxelX, voxelY + 1, voxelZ) == 0)
                                        sides |= VoxelSides.Top;

                                    voxelCount++;
                                    if (sides != VoxelSides.None)
                                    {
                                        Color3f lightColor = lightProvider.GetLightAt(voxelX, voxelY, voxelZ, blockX, blockY, blockZ, sides);
                                        byte a = (byte)((color >> 24) & 0xFF);
                                        byte r = (byte)Math.Min(255, (int)(((color >> 16) & 0xFF) * lightColor.R));
                                        byte g = (byte)Math.Min(255, (int)(((color >> 8) & 0xFF) * lightColor.G));
                                        byte b = (byte)Math.Min(255, (int)(((color >> 0) & 0xFF) * lightColor.B));

                                        polygonCount += meshHelper.AddVoxel(newMeshBuilder, sides,
                                            chunkVoxelX, (byte)(voxelY - voxelStartY), chunkVoxelZ, new Color4b(r, g, b, a));
                                        renderedVoxelCount++;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (polygonCount == 0)
            {
                // Loading resulted in no mesh data (i.e. all blocks were empty) so just release the lock on the mesh builder since we're done with it.
                newMeshBuilder.DropMesh();
                return;
            }

            using (new PerformanceLock(syncLock))
            {
                if (deleted)
                {
                    // Chunk was deleted during load so just release the lock on the mesh builder since we're done with it.
                    newMeshBuilder.DropMesh();
                }
                else
                {
                    meshBuilder = newMeshBuilder;
                    chunkPolygonCount = polygonCount;
                    chunkVoxelCount = voxelCount;
                    chunkRenderedVoxelCount = renderedVoxelCount;
                }
            }
        }

        private void InitializeNewMesh()
        {
            if (mesh != null)
                mesh.Dispose();

            if (chunkPolygonCount == 0)
                mesh = null;
            else
            {
                mesh = meshBuilder.GetMesh();
                mesh.Initialize();
            }

            meshBuilder.DropMesh(); // Release builder - Must be called after initializing the mesh
            meshBuilder = null;
        }
    }
}
