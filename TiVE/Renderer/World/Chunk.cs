//#define USE_INSTANCED_RENDERING
#define USE_INDEXED_RENDERING
using System;
using System.Diagnostics;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.Meshes;
using ProdigalSoftware.TiVE.Renderer.Voxels;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    internal sealed class Chunk : IDisposable
    {
        public const int TileSize = 4; // must be a power of 2

        private readonly int worldStartX;
        private readonly int worldStartY;
        private readonly int worldStartZ;
        private Matrix4 translationMatrix;
        private bool deleted;

        private readonly object syncLock = new object();

#if USE_INSTANCED_RENDERING
        private InstancedVoxelGroup voxels;
#else
#if USE_INDEXED_RENDERING
        private IndexedVoxelGroup voxels;
#else
        private SimpleVoxelGroup voxels;
#endif
#endif

        public Chunk(int worldStartX, int worldStartY, int worldStartZ)
        {
            this.worldStartX = worldStartX;
            this.worldStartY = worldStartY;
            this.worldStartZ = worldStartZ;
        }

        public void Dispose()
        {
            lock (syncLock)
            {
                if (voxels != null)
                    voxels.Dispose();
                deleted = true;
            }
        }

        public bool IsDeleted 
        {
            get
            {
                lock (syncLock)
                    return deleted;
            }
        }

        //public bool IsInitialized
        //{
        //    get
        //    {
        //        lock (syncLock)
        //            return voxels != null;
        //    }
        //}

#if USE_INSTANCED_RENDERING
        public InstancedVoxelGroup VoxelData
#else
#if USE_INDEXED_RENDERING
        public IndexedVoxelGroup VoxelData
#else
        public SimpleVoxelGroup VoxelData
#endif
#endif
        {
            get 
            { 
                lock (syncLock)
                    return voxels;
            }
        }

        public bool IsInside(int checkWorldStartX, int checkWorldStartY, int checkWorldEndX, int checkWorldEndY)
        {
            return checkWorldStartX <= worldStartX + TileSize && checkWorldEndX >= worldStartX &&
                checkWorldStartY <= worldStartY + TileSize && checkWorldEndY >= worldStartY;
        }

        public void Initialize()
        {
            lock (syncLock)
            {
                if (voxels != null)
                    voxels.Initialize();
            }
        }

        public void Render(ref Matrix4 viewProjectionMatrix)
        {
            lock (syncLock)
            {
                if (voxels == null)
                    return; // Not loaded yet

                Matrix4 viewProjectionModelMatrix;
                Matrix4.Mult(ref translationMatrix, ref viewProjectionMatrix, out viewProjectionModelMatrix);

                voxels.Render(ref viewProjectionModelMatrix);
            }
        }


        public void Load(ChunkLoadInfo info, MeshBuilder meshBuilder)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            GameWorld gameWorld = info.GameWorld;
            BlockList blockList = info.BlockList;

            //Debug.WriteLine("Started chunk ({0},{1},{2})", chunkStartX, chunkStartY, chunkStartZ);
            int tileMaxX = Math.Min(gameWorld.Xsize, TileSize);
            int tileMaxY = Math.Min(gameWorld.Ysize, TileSize);
            int tileMaxZ = Math.Min(gameWorld.Zsize, TileSize);

#if USE_INSTANCED_RENDERING
            InstancedVoxelGroup voxelGroup = new InstancedVoxelGroup(
#else
#if USE_INDEXED_RENDERING
            IndexedVoxelGroup voxelGroup = new IndexedVoxelGroup(
#else
            SimpleVoxelGroup voxelGroup = new SimpleVoxelGroup(
#endif
#endif
                tileMaxX * BlockInformation.BlockSize, tileMaxY * BlockInformation.BlockSize, tileMaxZ * BlockInformation.BlockSize);

            for (int tileX = 0; tileX < tileMaxX; tileX++)
            {
                int worldX = worldStartX + tileX;
                if (worldX < 0 || worldX >= gameWorld.Xsize)
                    continue;

                for (int tileY = 0; tileY < tileMaxY; tileY++)
                {
                    int worldY = worldStartY + tileY;
                    if (worldY < 0 || worldY >= gameWorld.Ysize)
                        continue;

                    for (int tileZ = 0; tileZ < tileMaxZ; tileZ++)
                    {
                        int worldZ = worldStartZ + tileZ;
                        if (worldZ < 0 || worldZ >= gameWorld.Zsize)
                            continue;

                        BlockInformation block = blockList[gameWorld.GetBlock(worldX, worldY, worldZ)];
                        if (block != null)
                        {
                            voxelGroup.SetVoxels(tileX * BlockInformation.BlockSize, tileY * BlockInformation.BlockSize,
                                tileZ * BlockInformation.BlockSize, block.Voxels);
                        }
                    }
                }
            }
            voxelGroup.DetermineVoxelVisibility();

            //const uint color = 0xFF000000;//E0E0E0;
            //const int ChunkVoxelSize = TileSize * BlockInformation.BlockSize;
            //int maxVoxelZ = tileMaxZ * BlockInformation.BlockSize;
            //for (int x = 0; x < ChunkVoxelSize; x++)
            //{
            //    voxelGroup.SetVoxel(x, 0, maxVoxelZ - 1, color);
            //    voxelGroup.SetVoxel(x, ChunkVoxelSize - 1, maxVoxelZ - 1, color);
            //}

            //for (int y = 0; y < ChunkVoxelSize; y++)
            //{
            //    voxelGroup.SetVoxel(0, y, maxVoxelZ - 1, color);
            //    voxelGroup.SetVoxel(ChunkVoxelSize - 1, y, maxVoxelZ - 1, color);
            //}

            //for (int z = 0; z < maxVoxelZ; z++)
            //{
            //    voxelGroup.SetVoxel(0, 0, z, color);
            //    voxelGroup.SetVoxel(ChunkVoxelSize - 1, 0, z, color);
            //    voxelGroup.SetVoxel(ChunkVoxelSize - 1, ChunkVoxelSize - 1, z, color);
            //    voxelGroup.SetVoxel(0, ChunkVoxelSize - 1, z, color);
            //}

            Matrix4 newTranslationMatrix = Matrix4.CreateTranslation(worldStartX * BlockInformation.BlockSize, worldStartY * BlockInformation.BlockSize,
                worldStartZ * BlockInformation.BlockSize);
            sw.Stop();
            float elapsedMs = sw.ElapsedTicks * 1000.0f / Stopwatch.Frequency;
            //if (elapsedMs > 5.0f)
            //    Debug.WriteLine("Voxel Load: " + elapsedMs + "ms");

            sw.Restart();
            voxelGroup.GenerateMesh(meshBuilder);
            sw.Stop();
            elapsedMs = sw.ElapsedTicks * 1000.0f / Stopwatch.Frequency;
            //if (elapsedMs > 5.0f)
            //   Debug.WriteLine("Mesh creation: " + elapsedMs + "ms for " + voxelGroup.RenderedVoxelCount + " voxels");
            lock (syncLock)
            {
                if (deleted)
                {
                    // Deleted while we were loading.
                    voxelGroup.Dispose();
                    return; 
                }
                voxels = voxelGroup;
                translationMatrix = newTranslationMatrix;
            }
        }

        internal sealed class ChunkLoadInfo
        {
            private readonly Chunk chunk;

            public ChunkLoadInfo(Chunk chunk, GameWorld gameWorld, BlockList blockList)
            {
                this.chunk = chunk;
                GameWorld = gameWorld;
                BlockList = blockList;
            }

            public GameWorld GameWorld { get; private set; }
            public BlockList BlockList { get; private set; }

            public Chunk Chunk
            {
                get { return chunk; }
            }

            public void Load(MeshBuilder meshBuilder)
            {
                chunk.Load(this, meshBuilder);
            }
        }
    }

}
