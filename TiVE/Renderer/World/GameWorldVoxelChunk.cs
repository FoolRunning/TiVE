using System;
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
        private readonly Vector3i chunkVoxelLoc;
        private Matrix4 translationMatrix;

        private bool deleted;
        private int loadedVoxelDetailLevel = -1;
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
            chunkVoxelLoc = new Vector3i(chunkBlockLoc.X * BlockInformation.VoxelSize, 
                chunkBlockLoc.Y * BlockInformation.VoxelSize, chunkBlockLoc.Z * BlockInformation.VoxelSize);
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
                loadedVoxelDetailLevel = -1;
                chunkPolygonCount = 0;
                chunkVoxelCount = 0;
                chunkRenderedVoxelCount = 0;
            }
        }

        public Vector3i ChunkBlockLocation
        {
            get { return chunkBlockLoc; }
        }

        public Vector3i ChunkVoxelLocation
        {
            get { return chunkVoxelLoc; }
        }

        public bool IsLoaded
        {
            get
            {
                using (new PerformanceLock(syncLock))
                    return mesh != null;
            }
        }

        public int LoadedVoxelDetailLevel
        {
            get { return loadedVoxelDetailLevel; }
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

        public void Load(MeshBuilder newMeshBuilder, IGameWorldRenderer renderer, int voxelDetailLevel)
        {
            loadedVoxelDetailLevel = voxelDetailLevel;

            Debug.Assert(newMeshBuilder.IsLocked);
            //Debug.WriteLine("Started chunk ({0},{1},{2})", chunkStartX, chunkStartY, chunkStartZ);

            GameWorld gameWorld = renderer.GameWorld;
            LightProvider lightProvider = renderer.LightProvider;
            VoxelMeshHelper meshHelper = VoxelMeshHelper.Get(false);

            int blockStartX = chunkLoc.X * BlockSize;
            int blockEndX = Math.Min((chunkLoc.X + 1) * BlockSize, gameWorld.BlockSize.X);
            int blockStartY = chunkLoc.Y * BlockSize;
            int blockEndY = Math.Min((chunkLoc.Y + 1) * BlockSize, gameWorld.BlockSize.Y);
            int blockStartZ = chunkLoc.Z * BlockSize;
            int blockEndZ = Math.Min((chunkLoc.Z + 1) * BlockSize, gameWorld.BlockSize.Z);

            int voxelStartX = blockStartX * BlockInformation.VoxelSize;
            int voxelStartY = blockStartY * BlockInformation.VoxelSize;
            int voxelStartZ = blockStartZ * BlockInformation.VoxelSize;

            int voxelSize = 1 << voxelDetailLevel;
            int maxVoxelX = gameWorld.VoxelSize.X - 1 - voxelSize;
            int maxVoxelY = gameWorld.VoxelSize.Y - 1 - voxelSize;
            int maxVoxelZ = gameWorld.VoxelSize.Z - 1 - voxelSize;

            int voxelCount = 0;
            int renderedVoxelCount = 0;
            int polygonCount = 0;
            int maxBlockVoxelSize = BlockInformation.VoxelSize - voxelSize;
            
            for (int blockZ = blockEndZ - 1; blockZ >= blockStartZ; blockZ--)
            {
                if (deleted)
                    break;

                for (int blockX = blockStartX; blockX < blockEndX; blockX++)
                {
                    for (int blockY = blockStartY; blockY < blockEndY; blockY++)
                    {
                        BlockInformation block = gameWorld[blockX, blockY, blockZ];
                        if (block == BlockInformation.Empty || block.NextBlock != null)
                            continue;

                        voxelCount += block.TotalVoxels;

                        //for (int bz = BlockInformation.VoxelSize - 1; bz >= 0; bz -= voxelSize)
                        for (int bz = 0; bz < BlockInformation.VoxelSize; bz += voxelSize)
                        {
                            int voxelZ = blockZ * BlockInformation.VoxelSize + bz;
                            byte chunkVoxelZ = (byte)(voxelZ - voxelStartZ);
                            for (int bx = 0; bx < BlockInformation.VoxelSize; bx += voxelSize)
                            {
                                int voxelX = blockX * BlockInformation.VoxelSize + bx;
                                byte chunkVoxelX = (byte)(voxelX - voxelStartX);
                                for (int by = 0; by < BlockInformation.VoxelSize; by += voxelSize)
                                {
                                    uint color = block[bx, by, bz];
                                    if (color == 0)
                                        continue;
                                    
                                    int voxelY = blockY * BlockInformation.VoxelSize + by;

                                    VoxelSides sides = VoxelSides.None;

                                    // Check to see if the back side is visible
                                    if (bz >= voxelSize)
                                    {
                                        if (block[bx, by, bz - voxelSize] == 0)
                                            sides |= VoxelSides.Back;
                                    }
                                    else if (voxelZ >= voxelSize && gameWorld.GetVoxel(voxelX, voxelY, voxelZ - voxelSize) == 0)
                                        sides |= VoxelSides.Back;

                                    // Check to see if the front side is visible
                                    if (bz < maxBlockVoxelSize)
                                    {
                                        if (block[bx, by, bz + voxelSize] == 0)
                                            sides |= VoxelSides.Front;
                                    }
                                    else if (voxelZ <= maxVoxelZ && gameWorld.GetVoxel(voxelX, voxelY, voxelZ + voxelSize) == 0)
                                        sides |= VoxelSides.Front;

                                    // Check to see if the left side is visible
                                    if (bx >= voxelSize)
                                    {
                                        if (block[bx - voxelSize, by, bz] == 0)
                                            sides |= VoxelSides.Left;
                                    }
                                    else if (voxelX >= voxelSize && gameWorld.GetVoxel(voxelX - voxelSize, voxelY, voxelZ) == 0)
                                        sides |= VoxelSides.Left;

                                    // Check to see if the right side is visible
                                    if (bx < maxBlockVoxelSize)
                                    {
                                        if (block[bx + voxelSize, by, bz] == 0)
                                            sides |= VoxelSides.Right;
                                    }
                                    else if (voxelX <= maxVoxelX && gameWorld.GetVoxel(voxelX + voxelSize, voxelY, voxelZ) == 0)
                                        sides |= VoxelSides.Right;

                                    // Check to see if the bottom side is visible
                                    if (by >= voxelSize)
                                    {
                                        if (block[bx, by - voxelSize, bz] == 0)
                                            sides |= VoxelSides.Bottom;
                                    }
                                    else if (voxelY >= voxelSize && gameWorld.GetVoxel(voxelX, voxelY - voxelSize, voxelZ) == 0)
                                        sides |= VoxelSides.Bottom;

                                    // Check to see if the top side is visible
                                    if (by < maxBlockVoxelSize)
                                    {
                                        if (block[bx, by + voxelSize, bz] == 0)
                                            sides |= VoxelSides.Top;
                                    }
                                    else if (voxelY <= maxVoxelY && gameWorld.GetVoxel(voxelX, voxelY + voxelSize, voxelZ) == 0)
                                        sides |= VoxelSides.Top;

                                    if (sides != VoxelSides.None)
                                    {
                                        Color3f lightColor = lightProvider.GetLightAt(voxelX, voxelY, voxelZ, blockX, blockY, blockZ, sides);
                                        byte a = (byte)((color >> 24) & 0xFF);
                                        byte r = (byte)Math.Min(255, (int)(((color >> 16) & 0xFF) * lightColor.R));
                                        byte g = (byte)Math.Min(255, (int)(((color >> 8) & 0xFF) * lightColor.G));
                                        byte b = (byte)Math.Min(255, (int)(((color >> 0) & 0xFF) * lightColor.B));

                                        polygonCount += meshHelper.AddVoxel(newMeshBuilder, sides,
                                            chunkVoxelX, (byte)(voxelY - voxelStartY), chunkVoxelZ, new Color4b(r, g, b, a), voxelSize);
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
                if (meshBuilder != null)
                    meshBuilder.DropMesh(); // Must have been loading this chunk while waiting to initialize a previous version of this chunk

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

        public override string ToString()
        {
            return string.Format("Chunk ({0}, {1}, {2}) {3}v", chunkLoc.X, chunkLoc.Y, chunkLoc.Z, chunkVoxelCount);
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
