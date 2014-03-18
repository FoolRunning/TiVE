#define USE_INSTANCED_RENDERING
using System;
using System.Diagnostics;
using OpenTK;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    internal sealed class Chunk
    {
        public const int TileSize = 4; // must be a power of 2
        private const int ChunkVoxelSize = BlockInformation.BlockSize * TileSize;

        private Matrix4 translationMatrix;

        private readonly object syncLock = new object();

#if USE_INSTANCED_RENDERING
        private InstancedVoxelGroup voxels;
        
        public InstancedVoxelGroup VoxelData
#else
        private VoxelGroup voxels;

        public VoxelGroup VoxelData
#endif
        {
            get 
            { 
                lock (syncLock)
                    return voxels; 
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

        public void Delete()
        {
            lock (syncLock)
                voxels.Delete();
        }

        public void Load(object threadContext)
        {
            ChunkLoadInfo info = (ChunkLoadInfo)threadContext;
            GameWorld gameWorld = info.GameWorld;
            BlockList blockList = info.BlockList;
            int chunkStartX = info.ChunkStartX;
            int chunkStartY = info.ChunkStartY;
            int chunkStartZ = info.ChunkStartZ;

            //Debug.WriteLine("Started chunk ({0},{1},{2})", chunkStartX, chunkStartY, chunkStartZ);
            int tileMaxX = Math.Min(gameWorld.Xsize, TileSize);
            int tileMaxY = Math.Min(gameWorld.Ysize, TileSize);
            int tileMaxZ = Math.Min(gameWorld.Zsize, TileSize);

#if USE_INSTANCED_RENDERING
            InstancedVoxelGroup voxelGroup = new InstancedVoxelGroup(tileMaxX * BlockInformation.BlockSize, tileMaxY * BlockInformation.BlockSize, 
                tileMaxZ * BlockInformation.BlockSize);
#else
            VoxelGroup voxelGroup = new VoxelGroup(tileMaxX * BlockInformation.BlockSize, tileMaxY * BlockInformation.BlockSize, 
                tileMaxZ * BlockInformation.BlockSize);
#endif

            for (int tileX = 0; tileX < tileMaxX; tileX++)
            {
                int worldX = chunkStartX + tileX;
                for (int tileY = 0; tileY < tileMaxY; tileY++)
                {
                    int worldY = chunkStartY + tileY;
                    for (int tileZ = 0; tileZ < tileMaxZ; tileZ++)
                    {
                        int worldZ = chunkStartZ + tileZ;
                        BlockInformation block = blockList[gameWorld.GetBlock(worldX, worldY, worldZ)];
                        if (block != null)
                        {
                            voxelGroup.SetVoxels(tileX * BlockInformation.BlockSize, tileY * BlockInformation.BlockSize,
                                tileZ * BlockInformation.BlockSize, block.Voxels);
                        }
                    }
                }
            }

            const uint color = 0xFFE0E0E0;
            int maxVoxelZ = tileMaxZ * BlockInformation.BlockSize;
            for (int x = 0; x < ChunkVoxelSize; x++)
            {
                voxelGroup.SetVoxel(x, 0, maxVoxelZ - 1, color);
                voxelGroup.SetVoxel(x, ChunkVoxelSize - 1, maxVoxelZ - 1, color);
            }

            for (int y = 0; y < ChunkVoxelSize; y++)
            {
                voxelGroup.SetVoxel(0, y, maxVoxelZ - 1, color);
                voxelGroup.SetVoxel(ChunkVoxelSize - 1, y, maxVoxelZ - 1, color);
            }

            for (int z = 0; z < maxVoxelZ; z++)
            {
                voxelGroup.SetVoxel(0, 0, z, color);
                voxelGroup.SetVoxel(ChunkVoxelSize - 1, 0, z, color);
                voxelGroup.SetVoxel(ChunkVoxelSize - 1, ChunkVoxelSize - 1, z, color);
                voxelGroup.SetVoxel(0, ChunkVoxelSize - 1, z, color);
            }

            Matrix4 newTranslationMatrix = Matrix4.CreateTranslation(chunkStartX * BlockInformation.BlockSize, chunkStartY * BlockInformation.BlockSize,
                chunkStartZ * BlockInformation.BlockSize);

            voxelGroup.GenerateMesh();

            lock (syncLock)
            {
                voxels = voxelGroup;
                translationMatrix = newTranslationMatrix;
            }
        }

        internal sealed class ChunkLoadInfo
        {
            public ChunkLoadInfo(GameWorld gameWorld, BlockList blockList, int chunkStartX, int chunkStartY, int chunkStartZ)
            {
                GameWorld = gameWorld;
                BlockList = blockList;
                ChunkStartX = chunkStartX;
                ChunkStartY = chunkStartY;
                ChunkStartZ = chunkStartZ;
            }

            public GameWorld GameWorld { get; private set; }
            public BlockList BlockList { get; private set; }
            public int ChunkStartX { get; private set; }
            public int ChunkStartY { get; private set; }
            public int ChunkStartZ { get; private set; }
        }
    }

}
