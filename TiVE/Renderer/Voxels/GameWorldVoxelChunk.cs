using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.Particles;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVE.Resources;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.Voxels
{
    internal sealed class GameWorldVoxelChunk : IDisposable
    {
        #region Constants
        public const int TileSize = 6;
        public const int VoxelSize = BlockInformation.BlockSize * TileSize;
        private const int SmallColorDiff = 10;
        private const int BigColorDiff = 20;
        #endregion

        private readonly int worldStartX;
        private readonly int worldStartY;
        private readonly int worldStartZ;
        private Matrix4 translationMatrix;
        private bool deleted;
        private IVertexDataCollection mesh;
        private readonly List<ParticleSystem> particleSystems = new List<ParticleSystem>();
        private readonly object syncLock = new object();
        private int chunkPolygonCount;
        private int chunkVoxelCount;
        private int chunkRenderedVoxelCount;

        public GameWorldVoxelChunk(int worldStartX, int worldStartY, int worldStartZ)
        {
            this.worldStartX = worldStartX;
            this.worldStartY = worldStartY;
            this.worldStartZ = worldStartZ;
        }

        public void Dispose()
        {
            IVertexDataCollection meshData;
            lock (syncLock)
            {
                meshData = mesh;
                mesh = null;
                deleted = true;
            }
            if (meshData != null)
                meshData.Dispose();

            for (int i = 0; i < particleSystems.Count; i++)
                ResourceManager.ParticleManager.RemoveParticleSystem(particleSystems[i]);
            particleSystems.Clear();
        }

        public bool IsDeleted
        {
            get
            {
                lock (syncLock)
                    return deleted;
            }
        }

        public bool IsInside(int checkWorldStartX, int checkWorldStartY, int checkWorldEndX, int checkWorldEndY)
        {
            return checkWorldStartX <= worldStartX + TileSize && checkWorldEndX >= worldStartX &&
                checkWorldStartY <= worldStartY + TileSize && checkWorldEndY >= worldStartY;
        }

        public void DetermineVoxelVisibility(uint[] voxels)
        {
            uint[] voxelData = voxels;
            for (int z = 0; z < VoxelSize; z++)
            {
                for (int x = 0; x < VoxelSize; x++)
                {
                    for (int y = 0; y < VoxelSize; y++)
                    {
                        uint color = voxelData[GetOffset(x, y, z)];
                        if (color == 0)
                            continue;

                        VoxelSides sides = VoxelSides.None;

                        if (z == VoxelSize - 1 || voxelData[GetOffset(x, y, z + 1)] == 0)
                            sides |= VoxelSides.Front;
                        //if (!IsZLineSet(x, y, z, 1)) // The back face is never shown to the camera, so there is no need to create it
                        //    sizes |= VoxelSides.Back;
                        if (x == 0 || voxelData[GetOffset(x - 1, y, z)] == 0)
                            sides |= VoxelSides.Left;
                        if (x == VoxelSize - 1 || voxelData[GetOffset(x + 1, y, z)] == 0)
                            sides |= VoxelSides.Right;
                        if (y == 0 || voxelData[GetOffset(x, y - 1, z)] == 0)
                            sides |= VoxelSides.Bottom;
                        if (y == VoxelSize - 1 || voxelData[GetOffset(x, y + 1, z)] == 0)
                            sides |= VoxelSides.Top;

                        voxelData[GetOffset(x, y, z)] = ((color & 0xFFFFFF) | (uint)((int)sides << 26));
                    }
                }
            }
        }

        public void Load(ChunkLoadInfo info, MeshBuilder meshBuilder)
        {
            GameWorld gameWorld = info.GameWorld;
            BlockList blockList = info.BlockList;

            //Debug.WriteLine("Started chunk ({0},{1},{2})", chunkStartX, chunkStartY, chunkStartZ);
            int tileMaxX = Math.Min(gameWorld.Xsize, TileSize);
            int tileMaxY = Math.Min(gameWorld.Ysize, TileSize);
            int tileMaxZ = Math.Min(gameWorld.Zsize, TileSize);

            particleSystems.Clear();
            uint[] voxels = meshBuilder.Voxels;
            for (int tileX = 0; tileX < tileMaxX; tileX++)
            {
                int worldX = worldStartX + tileX;
                int voxelX = tileX * BlockInformation.BlockSize;

                for (int tileY = 0; tileY < tileMaxY; tileY++)
                {
                    int worldY = worldStartY + tileY;
                    int voxelY = tileY * BlockInformation.BlockSize;

                    for (int tileZ = 0; tileZ < tileMaxZ; tileZ++)
                    {
                        int worldZ = worldStartZ + tileZ;
                        int voxelZ = tileZ * BlockInformation.BlockSize;

                        BlockInformation block;
                        if (worldX >= 0 && worldX < gameWorld.Xsize && worldY >= 0 && worldY < gameWorld.Ysize && worldZ >= 0 && worldZ < gameWorld.Zsize)
                            block = blockList[gameWorld.GetBlock(worldX, worldY, worldZ)] ?? BlockInformation.Empty;
                        else
                            block = BlockInformation.Empty;

                        SetVoxelsFromBlock(voxels, voxelX, voxelY, voxelZ, block);

                        ParticleSystemInformation particleInfo = block.ParticleSystem;
                        if (particleInfo != null)
                        {
                            Vector3b loc = particleInfo.Location;
                            ParticleSystem system = new ParticleSystem(particleInfo, (worldX * BlockInformation.BlockSize) + loc.X,
                                (worldY * BlockInformation.BlockSize) + loc.Y, (worldZ * BlockInformation.BlockSize) + loc.Z);
                            particleSystems.Add(system);
                            ResourceManager.ParticleManager.AddParticleSystem(system);
                        }
                    }
                }
            }
            //voxelGroup.DetermineVoxelVisibility();

            //const uint color = 0xFF0F0F0F; //E0E0E0;
            //const int ChunkVoxelSize = TileSize * BlockInformation.BlockSize;
            //int maxVoxelZ = tileMaxZ * BlockInformation.BlockSize;
            //for (int x = 0; x < ChunkVoxelSize; x++)
            //{
            //    voxels[GetOffset(x, 0, maxVoxelZ - 1)] = color;
            //    voxels[GetOffset(x, ChunkVoxelSize - 1, maxVoxelZ - 1)] = color;
            //}

            //for (int y = 0; y < ChunkVoxelSize; y++)
            //{
            //    voxels[GetOffset(0, y, maxVoxelZ - 1)] = color;
            //    voxels[GetOffset(ChunkVoxelSize - 1, y, maxVoxelZ - 1)] = color;
            //}

            //for (int z = 0; z < maxVoxelZ; z++)
            //{
            //    voxels[GetOffset(0, 0, z)] = color;
            //    voxels[GetOffset(ChunkVoxelSize - 1, 0, z)] = color;
            //    voxels[GetOffset(ChunkVoxelSize - 1, ChunkVoxelSize - 1, z)] = color;
            //    voxels[GetOffset(0, ChunkVoxelSize - 1, z)] = color;
            //}

            Matrix4 newTranslationMatrix = Matrix4.CreateTranslation(worldStartX * BlockInformation.BlockSize, worldStartY * BlockInformation.BlockSize,
                worldStartZ * BlockInformation.BlockSize);

            int voxelCount, renderedVoxelCount, polygonCount;
            IVertexDataCollection meshData = GenerateMesh(voxels, meshBuilder, out voxelCount, out renderedVoxelCount, out polygonCount);

            lock (syncLock)
            {
                if (deleted)
                {
                    meshData.Dispose();
                    Dispose();
                }
                else
                {
                    mesh = meshData;
                    translationMatrix = newTranslationMatrix;
                    chunkPolygonCount = polygonCount;
                    chunkVoxelCount = voxelCount;
                    chunkRenderedVoxelCount = renderedVoxelCount;
                }
            }
        }

        public void Initialize()
        {
            IVertexDataCollection meshData;
            lock (syncLock)
                meshData = mesh;
            
            if (meshData != null)
                meshData.Initialize();
        }

        public RenderStatistics RenderOpaque(ref Matrix4 viewProjectionMatrix)
        {
            IVertexDataCollection meshData;
            lock (syncLock)
                meshData = mesh;

            if (meshData == null)
                return new RenderStatistics(); // Not loaded yet

            meshData.Initialize();

            IShaderProgram shader = ResourceManager.ShaderManager.GetShaderProgram("MainWorld");
            shader.Bind();

            Matrix4 viewProjectionModelMatrix;
            Matrix4.Mult(ref translationMatrix, ref viewProjectionMatrix, out viewProjectionModelMatrix);
            shader.SetUniform("matrix_ModelViewProjection", ref viewProjectionModelMatrix);

            TiVEController.Backend.Draw(PrimitiveType.Triangles, meshData);
            return new RenderStatistics(1, chunkPolygonCount, chunkVoxelCount, chunkRenderedVoxelCount);
        }

        private static void SetVoxelsFromBlock(uint[] voxels,int startX, int startY, int startZ, BlockInformation block)
        {
            uint[, ,] blockData = block.Voxels;

            for (int z = 0; z < BlockInformation.BlockSize; z++)
            {
                int zOff = startZ + z;
                for (int x = 0; x < BlockInformation.BlockSize; x++)
                {
                    int xOff = startX + x;
                    for (int y = 0; y < BlockInformation.BlockSize; y++)
                        voxels[GetOffset(xOff, startY + y, zOff)] = blockData[x, y, z];
                }
            }
        }

        private static IVertexDataCollection GenerateMesh(uint[] voxels, MeshBuilder meshBuilder, out int voxelCount, out int renderedVoxelCount, out int polygonCount)
        {
            voxelCount = 0;
            renderedVoxelCount = 0;
            polygonCount = 0;

            meshBuilder.StartNewMesh();
            for (int z = 0; z < VoxelSize; z++)
            {
                for (int x = 0; x < VoxelSize; x++)
                {
                    for (int y = 0; y < VoxelSize; y++)
                    {
                        uint color = voxels[GetOffset(x, y, z)];
                        if (color == 0)
                            continue;

                        voxelCount++;

                        VoxelSides sides = VoxelSides.None;
                        if (z == VoxelSize - 1 || voxels[GetOffset(x, y, z + 1)] == 0)
                            sides |= VoxelSides.Front;
                        //if (!IsZLineSet(x, y, z, 1)) // The back face is never shown to the camera, so there is no need to create it
                        //    sizes |= VoxelSides.Back;
                        if (x == 0 || voxels[GetOffset(x - 1, y, z)] == 0)
                            sides |= VoxelSides.Left;
                        if (x == VoxelSize - 1 || voxels[GetOffset(x + 1, y, z)] == 0)
                            sides |= VoxelSides.Right;
                        if (y == 0 || voxels[GetOffset(x, y - 1, z)] == 0)
                            sides |= VoxelSides.Bottom;
                        //if (y == VoxelSize - 1 || voxels[GetOffset(x, y + 1, z)] == 0) // Top face is never shown to the camera
                        //    sides |= VoxelSides.Top;

                        //VoxelSides sides = (VoxelSides)((color & 0xFC000000) >> 26);
                        if (sides != VoxelSides.None)
                        {
                            polygonCount += AddVoxel(meshBuilder, sides, x, y, z,
                                (byte)((color >> 16) & 0xFF), (byte)((color >> 8) & 0xFF), (byte)((color >> 0) & 0xFF), 255 /*(byte)((color >> 24) & 0xFF)*/);
                            renderedVoxelCount++;
                        }
                    }
                }
            }
            return meshBuilder.GetMesh();
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetOffset(int x, int y, int z)
        {
            return x * VoxelSize * VoxelSize + z * VoxelSize + y;
        }
    }

    #region ChunkLoadInfo class
    internal sealed class ChunkLoadInfo
    {
        private readonly GameWorldVoxelChunk chunk;

        public ChunkLoadInfo(GameWorldVoxelChunk chunk)
        {
            this.chunk = chunk;
            GameWorld = ResourceManager.GameWorldManager.GameWorld;
            BlockList = ResourceManager.BlockListManager.BlockList;
        }

        public GameWorld GameWorld { get; private set; }
        public BlockList BlockList { get; private set; }

        public GameWorldVoxelChunk Chunk
        {
            get { return chunk; }
        }

        public void Load(MeshBuilder meshBuilder)
        {
            chunk.Load(this, meshBuilder);
        }
    }
    #endregion
}
