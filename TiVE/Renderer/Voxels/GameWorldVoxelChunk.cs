using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.Particles;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVE.Resources;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Particles;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.Voxels
{
    internal sealed class GameWorldVoxelChunk : IDisposable
    {
        #region Constants
        public const int TileSize = 5;
        public const int VoxelSize = BlockInformation.BlockSize * TileSize;
        private const int SmallColorDiff = 10;
        private const int BigColorDiff = 20;
        #endregion

        private readonly List<ParticleSystem> particleSystems = new List<ParticleSystem>();
        private readonly object syncLock = new object();
        private readonly int chunkX;
        private readonly int chunkY;
        private readonly int chunkZ;
        private readonly bool useInstancing;
        private Matrix4 translationMatrix;
        private bool deleted;
        private IVertexDataCollection mesh;
        private MeshBuilder meshBuilder;
        private int chunkPolygonCount;
        private int chunkVoxelCount;
        private int chunkRenderedVoxelCount;

        public GameWorldVoxelChunk(int chunkX, int chunkY, int chunkZ, bool useInstancing)
        {
            this.chunkX = chunkX;
            this.chunkY = chunkY;
            this.chunkZ = chunkZ;
            this.useInstancing = useInstancing;

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

            for (int i = 0; i < particleSystems.Count; i++)
                ResourceManager.ParticleManager.RemoveParticleSystem(particleSystems[i]);
            particleSystems.Clear();
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

        public void Load(MeshBuilder newMeshBuilder)
        {
            Debug.Assert(newMeshBuilder.IsLocked);

            GameWorld gameWorld = ResourceManager.GameWorldManager.GameWorld;

            //Debug.WriteLine("Started chunk ({0},{1},{2})", chunkStartX, chunkStartY, chunkStartZ);
            particleSystems.Clear();

            int maxVoxelX = gameWorld.WorldSizeX;
            int maxVoxelY = gameWorld.WorldSizeY;
            int maxVoxelZ = gameWorld.WorldSizeZ;

            int worldVoxelStartX = chunkX * TileSize * BlockInformation.BlockSize;
            int worldVoxelEndX = Math.Min((chunkX + 1) * TileSize * BlockInformation.BlockSize, maxVoxelX);
            int worldVoxelStartY = chunkY * TileSize * BlockInformation.BlockSize;
            int worldVoxelEndY = Math.Min((chunkY + 1) * TileSize * BlockInformation.BlockSize, maxVoxelY);
            int worldVoxelStartZ = chunkZ * TileSize * BlockInformation.BlockSize;
            int worldVoxelEndZ = Math.Min((chunkZ + 1) * TileSize * BlockInformation.BlockSize, maxVoxelZ);

            int voxelCount = 0;
            int renderedVoxelCount = 0;
            int polygonCount = 0;

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

                        VoxelSides sides = VoxelSides.None;
                        if (z == maxVoxelZ - 1 || gameWorld.GetVoxel(x, y, z + 1) == 0)
                            sides |= VoxelSides.Front;
                        //if (!IsZLineSet(x, y, z, 1)) // The back face is never shown to the camera, so there is no need to create it
                        //    sizes |= VoxelSides.Back;
                        if (x == 0 || gameWorld.GetVoxel(x - 1, y, z) == 0)
                            sides |= VoxelSides.Left;
                        if (x == maxVoxelX - 1 || gameWorld.GetVoxel(x + 1, y, z) == 0)
                            sides |= VoxelSides.Right;
                        if (y == 0 || gameWorld.GetVoxel(x, y - 1, z) == 0)
                            sides |= VoxelSides.Bottom;
                        if (y == maxVoxelY - 1 || gameWorld.GetVoxel(x, y + 1, z) == 0)
                            sides |= VoxelSides.Top;

                        if (sides != VoxelSides.None)
                        {
                            byte a = (byte)((color >> 24) & 0xFF);
                            byte r = (byte)(((color >> 16) & 0xFF));
                            byte g = (byte)(((color >> 8) & 0xFF));
                            byte b = (byte)(((color >> 0) & 0xFF));
                            polygonCount += AddVoxel(newMeshBuilder, sides, cx, y - worldVoxelStartY, cz, r, g, b, a);
                            renderedVoxelCount++;
                        }
                    }
                }
            }

            //            BlockInformation block = gameWorld.GetBlock(worldX, worldY, worldZ);
            //            SetVoxelsFromBlock(voxels, voxelX, voxelY, voxelZ, block);

            //            ParticleSystemInformation particleInfo = block.ParticleSystem;
            //            if (particleInfo != null)
            //            {
            //                Vector3b loc = particleInfo.Location;
            //                ParticleSystem system = new ParticleSystem(particleInfo, (worldX * BlockInformation.BlockSize) + loc.X,
            //                    (worldY * BlockInformation.BlockSize) + loc.Y, (worldZ * BlockInformation.BlockSize) + loc.Z);
            //                particleSystems.Add(system);
            //            }

            using (new PerformanceLock(syncLock))
            {
                meshBuilder = newMeshBuilder;
                chunkPolygonCount = polygonCount;
                chunkVoxelCount = voxelCount;
                chunkRenderedVoxelCount = renderedVoxelCount;

                for (int i = 0; i < particleSystems.Count; i++)
                    ResourceManager.ParticleManager.AddParticleSystem(particleSystems[i]);
                
                if (deleted)
                    Dispose(); // Chunk was deleted while loading
            }
        }

        public RenderStatistics Render(ref Matrix4 viewProjectionMatrix)
        {
            IVertexDataCollection meshData;
            using (new PerformanceLock(syncLock))
            {
                if (meshBuilder != null)
                {
                    if (mesh != null)
                        mesh.Dispose();

                    mesh = meshBuilder.GetMesh();
                    mesh.Initialize();
                    
                    meshBuilder.DropMesh(); // Release builder - Must be called after initializing the mesh
                    meshBuilder = null;
                }
                meshData = mesh;
            }

            if (meshData == null || chunkPolygonCount == 0)
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

        private static int AddVoxel(MeshBuilder meshBuilder, VoxelSides sides, int x, int y, int z, byte cr, byte cg, byte cb, byte ca)
        {
            int polygonCount = 0;
            if ((sides & VoxelSides.Front) != 0)
            {
                int v3 = meshBuilder.Add(x + 1, y + 1, z + 1, cr, cg, cb, ca);
                int v4 = meshBuilder.Add(x, y + 1, z + 1, cr, cg, cb, ca);
                int v7 = meshBuilder.Add(x + 1, y, z + 1, cr, cg, cb, ca);
                int v8 = meshBuilder.Add(x, y, z + 1, cr, cg, cb, ca);

                meshBuilder.AddIndex(v8);
                meshBuilder.AddIndex(v3);
                meshBuilder.AddIndex(v7);

                meshBuilder.AddIndex(v3);
                meshBuilder.AddIndex(v8);
                meshBuilder.AddIndex(v4);
                polygonCount += 2;
            }

            // The back face is never shown to the camera, so there is no need to create it
            //if ((sides & VoxelSides.Back) != 0)
            //{
            //    GL.Vertex3(x * World_Block_Size + World_Block_Size, y * World_Block_Size + World_Block_Size, z * World_Block_Size + World_Block_Size);
            //    GL.Vertex3(x * World_Block_Size + World_Block_Size, y * World_Block_Size, z * World_Block_Size + World_Block_Size);
            //    GL.Vertex3(x * World_Block_Size, y * World_Block_Size, z * World_Block_Size + World_Block_Size);

            //    GL.Vertex3(x * World_Block_Size, y * World_Block_Size, z * World_Block_Size + World_Block_Size);
            //    GL.Vertex3(x * World_Block_Size, y * World_Block_Size + World_Block_Size, z * World_Block_Size + World_Block_Size);
            //    GL.Vertex3(x * World_Block_Size + World_Block_Size, y * World_Block_Size + World_Block_Size, z * World_Block_Size + World_Block_Size);
            //    PolygonCount += 2;
            //}

            if ((sides & VoxelSides.Left) != 0)
            {
                byte crr = (byte)Math.Min(255, cr + SmallColorDiff);
                byte cgr = (byte)Math.Min(255, cg + SmallColorDiff);
                byte cbr = (byte)Math.Min(255, cb + SmallColorDiff);
                int v1 = meshBuilder.Add(x, y + 1, z, crr, cgr, cbr, ca);
                int v4 = meshBuilder.Add(x, y + 1, z + 1, crr, cgr, cbr, ca);
                int v5 = meshBuilder.Add(x, y, z, crr, cgr, cbr, ca);
                int v8 = meshBuilder.Add(x, y, z + 1, crr, cgr, cbr, ca);

                meshBuilder.AddIndex(v5);
                meshBuilder.AddIndex(v1);
                meshBuilder.AddIndex(v4);

                meshBuilder.AddIndex(v4);
                meshBuilder.AddIndex(v8);
                meshBuilder.AddIndex(v5);
                polygonCount += 2;
            }

            if ((sides & VoxelSides.Right) != 0)
            {
                byte crl = (byte)Math.Max(0, cr - SmallColorDiff);
                byte cgl = (byte)Math.Max(0, cg - SmallColorDiff);
                byte cbl = (byte)Math.Max(0, cb - SmallColorDiff);
                int v2 = meshBuilder.Add(x + 1, y + 1, z, crl, cgl, cbl, ca);
                int v3 = meshBuilder.Add(x + 1, y + 1, z + 1, crl, cgl, cbl, ca);
                int v6 = meshBuilder.Add(x + 1, y, z, crl, cgl, cbl, ca);
                int v7 = meshBuilder.Add(x + 1, y, z + 1, crl, cgl, cbl, ca);

                meshBuilder.AddIndex(v6);
                meshBuilder.AddIndex(v3);
                meshBuilder.AddIndex(v2);

                meshBuilder.AddIndex(v3);
                meshBuilder.AddIndex(v6);
                meshBuilder.AddIndex(v7);
                polygonCount += 2;
            }

            if ((sides & VoxelSides.Bottom) != 0)
            {
                byte crb = (byte)Math.Max(0, cr - BigColorDiff);
                byte cgb = (byte)Math.Max(0, cg - BigColorDiff);
                byte cbb = (byte)Math.Max(0, cb - BigColorDiff);
                int v5 = meshBuilder.Add(x, y, z, crb, cgb, cbb, ca);
                int v6 = meshBuilder.Add(x + 1, y, z, crb, cgb, cbb, ca);
                int v7 = meshBuilder.Add(x + 1, y, z + 1, crb, cgb, cbb, ca);
                int v8 = meshBuilder.Add(x, y, z + 1, crb, cgb, cbb, ca);

                meshBuilder.AddIndex(v5);
                meshBuilder.AddIndex(v7);
                meshBuilder.AddIndex(v6);

                meshBuilder.AddIndex(v5);
                meshBuilder.AddIndex(v8);
                meshBuilder.AddIndex(v7);
                polygonCount += 2;
            }

            if ((sides & VoxelSides.Top) != 0)
            {
                byte crt = (byte)Math.Min(255, cr + BigColorDiff);
                byte cgt = (byte)Math.Min(255, cg + BigColorDiff);
                byte cbt = (byte)Math.Min(255, cb + BigColorDiff);
                int v1 = meshBuilder.Add(x, y + 1, z, crt, cgt, cbt, ca);
                int v2 = meshBuilder.Add(x + 1, y + 1, z, crt, cgt, cbt, ca);
                int v3 = meshBuilder.Add(x + 1, y + 1, z + 1, crt, cgt, cbt, ca);
                int v4 = meshBuilder.Add(x, y + 1, z + 1, crt, cgt, cbt, ca);

                meshBuilder.AddIndex(v1);
                meshBuilder.AddIndex(v2);
                meshBuilder.AddIndex(v3);

                meshBuilder.AddIndex(v1);
                meshBuilder.AddIndex(v3);
                meshBuilder.AddIndex(v4);
                polygonCount += 2;
            }

            return polygonCount;
        }
    }
}
