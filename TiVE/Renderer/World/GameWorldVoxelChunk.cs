using System;
using System.Diagnostics;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.Voxels;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    internal sealed class GameWorldVoxelChunk : IDisposable
    {
        #region Constants
        public const int TileSize = 5;
        #endregion

        private MeshBuilder meshBuilder;
        private readonly object syncLock = new object();
        private readonly int chunkX;
        private readonly int chunkY;
        private readonly int chunkZ;
        private bool deleted;
        private IVertexDataCollection mesh;
        private Matrix4 translationMatrix;
        private int chunkPolygonCount;
        private int chunkVoxelCount;
        private int chunkRenderedVoxelCount;

        public GameWorldVoxelChunk(int chunkX, int chunkY, int chunkZ)
        {
            this.chunkX = chunkX;
            this.chunkY = chunkY;
            this.chunkZ = chunkZ;

            translationMatrix = Matrix4.CreateTranslation(chunkX * TileSize * BlockInformation.BlockSize,
                chunkY * TileSize * BlockInformation.BlockSize, chunkZ * TileSize * BlockInformation.BlockSize);
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

        public MeshBuilder MeshBuilder 
        {
            get 
            { 
                using (new PerformanceLock(syncLock))
                    return meshBuilder; 
            }
        }

        public bool NeedsInitialization 
        {
            get
            {
                using (new PerformanceLock(syncLock))
                    return meshBuilder != null;
            }
        }

        public bool IsDeleted
        {
            get
            {
                using (new PerformanceLock(syncLock))
                    return deleted;
            }
        }

        public bool IsInside(int checkChunkStartX, int checkChunkStartY, int checkChunkEndX, int checkChunkEndY)
        {
            return checkChunkStartX <= chunkX && checkChunkEndX >= chunkX && checkChunkStartY <= chunkY && checkChunkEndY >= chunkY;
        }

        public void PrepareForLoad()
        {
            using (new PerformanceLock(syncLock))
                deleted = false;
        }

        public bool Initialize()
        {
            using (new PerformanceLock(syncLock))
            {
                if (meshBuilder == null)
                    return false;
                
                if (mesh != null)
                    mesh.Dispose();

                if (chunkPolygonCount > 0)
                {
                    mesh = meshBuilder.GetMesh();
                    mesh.Initialize();
                }

                meshBuilder.DropMesh(); // Release builder - Must be called after initializing the mesh
                meshBuilder = null;
            }
            return chunkPolygonCount > 0;
        }

        public RenderStatistics Render(ref Matrix4 viewProjectionMatrix)
        {
            if (chunkPolygonCount == 0)
                return new RenderStatistics(); // Nothing to render for this chunk

            IVertexDataCollection meshData;
            using (new PerformanceLock(syncLock))
                meshData = mesh;

            if (meshData == null)
                return new RenderStatistics(); // Nothing to render for this chunk

            Debug.Assert(meshData.IsInitialized);

            IShaderProgram shader = ResourceManager.ShaderManager.GetShaderProgram("MainWorld");
            shader.Bind();

            Matrix4 viewProjectionModelMatrix;
            Matrix4.Mult(ref translationMatrix, ref viewProjectionMatrix, out viewProjectionModelMatrix);
            shader.SetUniform("matrix_ModelViewProjection", ref viewProjectionModelMatrix);

            TiVEController.Backend.Draw(PrimitiveType.Triangles, meshData);
            return new RenderStatistics(1, chunkPolygonCount, chunkVoxelCount, chunkRenderedVoxelCount);
        }

        public override string ToString()
        {
            return string.Format("Chunk ({0}, {1}, {2}) {3}v", chunkX, chunkY, chunkZ, chunkVoxelCount);
        }

        public void Load(MeshBuilder newMeshBuilder)
        {
            Debug.Assert(newMeshBuilder.IsLocked);

            GameWorld gameWorld = ResourceManager.GameWorldManager.GameWorld;
            BlockList blockList = ResourceManager.BlockListManager.BlockList;

            //Debug.WriteLine("Started chunk ({0},{1},{2})", chunkStartX, chunkStartY, chunkStartZ);

            int voxelStartX = chunkX * TileSize * BlockInformation.BlockSize;
            int voxelStartY = chunkY * TileSize * BlockInformation.BlockSize;
            int voxelStartZ = chunkZ * TileSize * BlockInformation.BlockSize;

            int blockStartX = chunkX * TileSize;
            int blockEndX = Math.Min((chunkX + 1) * TileSize, gameWorld.BlockSize.X);
            int blockStartY = chunkY * TileSize;
            int blockEndY = Math.Min((chunkY + 1) * TileSize, gameWorld.BlockSize.Y);
            int blockStartZ = chunkZ * TileSize;
            int blockEndZ = Math.Min((chunkZ + 1) * TileSize, gameWorld.BlockSize.Z);

            int maxVoxelX = gameWorld.VoxelSize.X;
            int maxVoxelY = gameWorld.VoxelSize.Y;
            int maxVoxelZ = gameWorld.VoxelSize.Z;

            int voxelCount = 0;
            int renderedVoxelCount = 0;
            int polygonCount = 0;

            for (int z = blockEndZ - 1; z >= blockStartZ; z--)
            {
                for (int x = blockStartX; x < blockEndX; x++)
                {
                    for (int y = blockStartY; y < blockEndY; y++)
                    {
                        BlockInformation block = gameWorld[x, y, z];
                        if (block == BlockInformation.Empty || blockList.BelongsToAnimation(block))
                            continue;

                        for (int bz = BlockInformation.BlockSize - 1; bz >= 0; bz--)
                        {
                            int voxelZ = z * BlockInformation.BlockSize + bz;
                            int chunkVoxelZ = voxelZ - voxelStartZ;
                            for (int bx = 0; bx < BlockInformation.BlockSize; bx++)
                            {
                                int voxelX = x * BlockInformation.BlockSize + bx;
                                int chunkVoxelX = voxelX - voxelStartX;
                                for (int by = 0; by < BlockInformation.BlockSize; by++)
                                {
                                    uint color = block[bx, by, bz];
                                    if (color == 0)
                                        continue;
                                    
                                    int voxelY = y * BlockInformation.BlockSize + by;

                                    voxelCount++;

                                    VoxelSides sides = VoxelSides.None;

                                    // Check to see if the front side is visible
                                    if (bz < BlockInformation.BlockSize - 1)
                                    {
                                        if (block[bx, by, bz + 1] == 0)
                                            sides |= VoxelSides.Front;
                                    }
                                    else if (bz == BlockInformation.BlockSize - 1)
                                    {
                                        if (voxelZ == maxVoxelZ - 1 || gameWorld.GetVoxel(voxelX, voxelY, voxelZ + 1) == 0)
                                            sides |= VoxelSides.Front;
                                    }

                                    // The back face is never shown to the camera, so there is no need to create it
                                    //if (bz > 0)
                                    //{
                                    //    if (block[bx, by, bz - 1] == 0)
                                    //        sides |= VoxelSides.Back;
                                    //}
                                    //else if (bz == 0)
                                    //{
                                    //    if (voxelZ > 0 && gameWorld.GetVoxel(voxelX, voxelY, voxelZ - 1) == 0)
                                    //        sides |= VoxelSides.Back;
                                    //}

                                    // Check to see if the left side is visible
                                    if (bx > 0)
                                    {
                                        if (block[bx - 1, by, bz] == 0)
                                            sides |= VoxelSides.Left;
                                    }
                                    else if (bx == 0)
                                    {
                                        if (voxelX == 0 || gameWorld.GetVoxel(voxelX - 1, voxelY, voxelZ) == 0)
                                            sides |= VoxelSides.Left;
                                    }

                                    // Check to see if the right side is visible
                                    if (bx < BlockInformation.BlockSize - 1)
                                    {
                                        if (block[bx + 1, by, bz] == 0)
                                            sides |= VoxelSides.Right;
                                    }
                                    else if (bx == BlockInformation.BlockSize - 1)
                                    {
                                        if (voxelX == maxVoxelX - 1 || gameWorld.GetVoxel(voxelX + 1, voxelY, voxelZ) == 0)
                                            sides |= VoxelSides.Right;
                                    }

                                    // Check to see if the bottom side is visible
                                    if (by > 0)
                                    {
                                        if (block[bx, by - 1, bz] == 0)
                                            sides |= VoxelSides.Bottom;
                                    }
                                    else if (by == 0)
                                    {
                                        if (voxelY == 0 || gameWorld.GetVoxel(voxelX, voxelY - 1, voxelZ) == 0)
                                            sides |= VoxelSides.Bottom;
                                    }

                                    // Check to see if the top side is visible
                                    if (by < BlockInformation.BlockSize - 1)
                                    {
                                        if (block[bx, by + 1, bz] == 0)
                                            sides |= VoxelSides.Top;
                                    }
                                    else if (by == BlockInformation.BlockSize - 1)
                                    {
                                        if (voxelY == maxVoxelY - 1 || gameWorld.GetVoxel(voxelX, voxelY + 1, voxelZ) == 0)
                                            sides |= VoxelSides.Top;
                                    }

                                    if (sides != VoxelSides.None)
                                    {
                                        Color3f lightColor = gameWorld.GetLightAt(voxelX, voxelY, voxelZ);
                                        byte a = (byte)((color >> 24) & 0xFF);
                                        byte r = (byte)Math.Min(255, (int)(((color >> 16) & 0xFF) * lightColor.R));
                                        byte g = (byte)Math.Min(255, (int)(((color >> 8) & 0xFF) * lightColor.G));
                                        byte b = (byte)Math.Min(255, (int)(((color >> 0) & 0xFF) * lightColor.B));

                                        //byte a = (byte)((color >> 24) & 0xFF);
                                        //byte r = (byte)((color >> 16) & 0xFF);
                                        //byte g = (byte)((color >> 8) & 0xFF);
                                        //byte b = (byte)((color >> 0) & 0xFF);

                                        polygonCount += IndexedVoxelGroup.CreateVoxel(newMeshBuilder, sides, 
                                            chunkVoxelX, voxelY - voxelStartY, chunkVoxelZ, new Color4b(r, g, b, a));
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
    }
}
