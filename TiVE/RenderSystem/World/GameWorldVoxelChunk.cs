using System;
using System.Diagnostics;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVE.RenderSystem.Lighting;
using ProdigalSoftware.TiVE.RenderSystem.Meshes;
using ProdigalSoftware.TiVE.RenderSystem.Voxels;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.RenderSystem.World
{
    internal sealed class GameWorldVoxelChunk : IDisposable
    {
        #region Constants
        public const int BlockSize = 4;
        public const int VoxelSize = BlockSize * Block.VoxelSize;
        #endregion

        #region Member variables
        private readonly object syncLock = new object();
        private readonly Vector3i chunkLoc;
        private readonly Vector3i chunkBlockLoc;
        private readonly Vector3i chunkVoxelLoc;
        private Matrix4f translationMatrix;

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
            chunkVoxelLoc = new Vector3i(chunkX * VoxelSize, chunkY * VoxelSize, chunkZ * VoxelSize);
            translationMatrix = Matrix4f.CreateTranslation(chunkX * VoxelSize, chunkY * VoxelSize, chunkZ * VoxelSize);
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

        public RenderStatistics Render(ShaderManager shaderManager, ref Matrix4f viewProjectionMatrix)
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

            Matrix4f viewProjectionModelMatrix;
            Matrix4f.Mult(ref translationMatrix, ref viewProjectionMatrix, out viewProjectionModelMatrix);
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
            BlockList blockList = renderer.BlockList;

            int blockStartX = chunkLoc.X * BlockSize;
            int blockEndX = Math.Min((chunkLoc.X + 1) * BlockSize, gameWorld.BlockSize.X);
            int blockStartY = chunkLoc.Y * BlockSize;
            int blockEndY = Math.Min((chunkLoc.Y + 1) * BlockSize, gameWorld.BlockSize.Y);
            int blockStartZ = chunkLoc.Z * BlockSize;
            int blockEndZ = Math.Min((chunkLoc.Z + 1) * BlockSize, gameWorld.BlockSize.Z);

            int voxelStartX = blockStartX * Block.VoxelSize;
            int voxelStartY = blockStartY * Block.VoxelSize;
            int voxelStartZ = blockStartZ * Block.VoxelSize;

            int voxelCount = 0;
            int renderedVoxelCount = 0;
            int polygonCount = 0;
            for (int blockZ = blockEndZ - 1; blockZ >= blockStartZ; blockZ--)
            {
                if (deleted)
                    break;

                for (int blockX = blockStartX; blockX < blockEndX; blockX++)
                {
                    for (int blockY = blockStartY; blockY < blockEndY; blockY++)
                    {
                        ushort blockIndex = gameWorld[blockX, blockY, blockZ];
                        if (blockIndex == 0)
                            continue;

                        BlockImpl block = (BlockImpl)blockList[blockIndex];
                        //if (block.NextBlock != null)
                        //    continue;

                        voxelCount += block.TotalVoxels;

                        CreateMeshForBlock(block, blockX, blockY, blockZ, voxelStartX, voxelStartY, voxelStartZ, voxelDetailLevel,
                            renderer, newMeshBuilder, ref polygonCount, ref renderedVoxelCount);
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

        private static void CreateMeshForBlock(BlockImpl block, int blockX, int blockY, int blockZ,
            int voxelStartX, int voxelStartY, int voxelStartZ, int voxelDetailLevel, IGameWorldRenderer renderer, 
            MeshBuilder newMeshBuilder, ref int polygonCount, ref int renderedVoxelCount)
        {
            GameWorld gameWorld = renderer.GameWorld;
            LightProvider lightProvider = renderer.LightProvider;
            VoxelMeshHelper meshHelper = VoxelMeshHelper.Get(false);

            int voxelSize = 1 << voxelDetailLevel;
            int maxVoxelX = gameWorld.VoxelSize.X - 1 - voxelSize;
            int maxVoxelY = gameWorld.VoxelSize.Y - 1 - voxelSize;
            int maxVoxelZ = gameWorld.VoxelSize.Z - 1 - voxelSize;
            int maxBlockVoxelSize = Block.VoxelSize - voxelSize;

            bool blockIsLit = !block.HasComponent<UnlitComponent>();
            //for (int bz = BlockInformation.VoxelSize - 1; bz >= 0; bz -= voxelSize)
            for (int bvz = 0; bvz < Block.VoxelSize; bvz += voxelSize)
            {
                int voxelZ = blockZ * Block.VoxelSize + bvz;
                byte chunkVoxelZ = (byte)(voxelZ - voxelStartZ);
                for (int bvx = 0; bvx < Block.VoxelSize; bvx += voxelSize)
                {
                    int voxelX = blockX * Block.VoxelSize + bvx;
                    byte chunkVoxelX = (byte)(voxelX - voxelStartX);
                    for (int bvy = 0; bvy < Block.VoxelSize; bvy += voxelSize)
                    {
                        uint color = voxelSize == 1 ? block[bvx, bvy, bvz] : GetLODVoxelColor(block, bvz, bvx, bvy, voxelSize);
                        if (color == 0)
                            continue;

                        int voxelY = blockY * Block.VoxelSize + bvy;

                        VoxelSides sides = VoxelSides.None;

                        // Check to see if the back side is visible
                        if (bvz >= voxelSize)
                        {
                            if (block[bvx, bvy, bvz - voxelSize] == 0)
                                sides |= VoxelSides.Back;
                        }
                        else if (voxelZ >= voxelSize && gameWorld.GetVoxel(voxelX, voxelY, voxelZ - voxelSize) == 0)
                            sides |= VoxelSides.Back;

                        // Check to see if the front side is visible
                        if (bvz < maxBlockVoxelSize)
                        {
                            if (block[bvx, bvy, bvz + voxelSize] == 0)
                                sides |= VoxelSides.Front;
                        }
                        else if (voxelZ <= maxVoxelZ && gameWorld.GetVoxel(voxelX, voxelY, voxelZ + voxelSize) == 0)
                            sides |= VoxelSides.Front;

                        // Check to see if the left side is visible
                        if (bvx >= voxelSize)
                        {
                            if (block[bvx - voxelSize, bvy, bvz] == 0)
                                sides |= VoxelSides.Left;
                        }
                        else if (voxelX >= voxelSize && gameWorld.GetVoxel(voxelX - voxelSize, voxelY, voxelZ) == 0)
                            sides |= VoxelSides.Left;

                        // Check to see if the right side is visible
                        if (bvx < maxBlockVoxelSize)
                        {
                            if (block[bvx + voxelSize, bvy, bvz] == 0)
                                sides |= VoxelSides.Right;
                        }
                        else if (voxelX <= maxVoxelX && gameWorld.GetVoxel(voxelX + voxelSize, voxelY, voxelZ) == 0)
                            sides |= VoxelSides.Right;

                        // Check to see if the bottom side is visible
                        if (bvy >= voxelSize)
                        {
                            if (block[bvx, bvy - voxelSize, bvz] == 0)
                                sides |= VoxelSides.Bottom;
                        }
                        else if (voxelY >= voxelSize && gameWorld.GetVoxel(voxelX, voxelY - voxelSize, voxelZ) == 0)
                            sides |= VoxelSides.Bottom;

                        // Check to see if the top side is visible
                        if (bvy < maxBlockVoxelSize)
                        {
                            if (block[bvx, bvy + voxelSize, bvz] == 0)
                                sides |= VoxelSides.Top;
                        }
                        else if (voxelY <= maxVoxelY && gameWorld.GetVoxel(voxelX, voxelY + voxelSize, voxelZ) == 0)
                            sides |= VoxelSides.Top;

                        if (sides != VoxelSides.None)
                        {
                            Color3f lightColor = blockIsLit ?
                                lightProvider.GetLightAt(voxelX, voxelY, voxelZ, voxelSize, blockX, blockY, blockZ, sides) : Color3f.White;
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

        private static uint GetLODVoxelColor(BlockImpl block, int bvz, int bvx, int bvy, int voxelSize)
        {
            uint color;
            int voxelsFound = 0;
            int maxA = 0;
            int totalR = 0;
            int totalG = 0;
            int totalB = 0;
            int maxX = bvx + voxelSize;
            int maxY = bvy + voxelSize;
            int maxZ = bvz + voxelSize;
            //for (int z = bvz; z > bvz - voxelSize; z--)
            for (int z = bvz; z < maxZ; z++)
            {
                for (int x = bvx; x < maxX; x++)
                {
                    for (int y = bvy; y < maxY; y++)
                    {
                        uint otherColor = block[x, y, z];
                        if (otherColor == 0)
                            continue;

                        voxelsFound++;
                        int a = (int)((otherColor >> 24) & 0xFF);
                        totalR += (int)((otherColor >> 16) & 0xFF);
                        totalG += (int)((otherColor >> 8) & 0xFF);
                        totalB += (int)((otherColor >> 0) & 0xFF);
                        if (a > maxA)
                            maxA = a;
                    }
                }
            }

            if (voxelsFound == 0) // Prevent divide-by-zero
                color = 0;
            else
            {
                color = (uint)((maxA & 0xFF) << 24 |
                    ((totalR / voxelsFound) & 0xFF) << 16 |
                    ((totalG / voxelsFound) & 0xFF) << 8 |
                    ((totalB / voxelsFound) & 0xFF));
            }
            return color;
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
