#define USE_INSTANCED_RENDERING

using System;
using System.Diagnostics;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal sealed class WorldChunkRenderer : IGameWorldRenderer
    {
        private const int ChunkSize = 8; // must be a power of 2
        private const int ChunkVoxelSize = BlockInformation.BlockSize * ChunkSize;

        private readonly GameWorld gameWorld;
        private readonly BlockList blockList;
        private readonly Chunk[,] worldChunks;
      

        public WorldChunkRenderer(GameWorld gameWorld, BlockList blockList)
        {
            this.gameWorld = gameWorld;
            this.blockList = blockList;
            worldChunks = new Chunk[gameWorld.Xsize / ChunkSize, gameWorld.Ysize / ChunkSize];
        }

        public void CleanUp()
        {
            for (int x = 0; x <= worldChunks.GetUpperBound(0); x++)
            {
                for (int y = 0; y <= worldChunks.GetUpperBound(1); y++)
                {
                    if (worldChunks[x, y] != null)
                        worldChunks[x, y].Delete();
                    worldChunks[x, y] = null;
                }
            }
        }

        public void Draw(Camera camera, out int drawCount, out int polygonCount, out int voxelCount, out int renderedVoxelCount)
        {
            int worldMinX, worldMaxX, worldMinY, worldMaxY;
            GetWorldView(camera, camera.Location.Z, out worldMinX, out worldMaxX, out worldMinY, out worldMaxY);

            worldMinX = Math.Max(worldMinX, 0);
            worldMinY = Math.Max(worldMinY, 0);
            worldMaxX = Math.Min(worldMaxX, gameWorld.Xsize);
            worldMaxY = Math.Min(worldMaxY, gameWorld.Ysize);

            int chunkMinX = worldMinX / ChunkSize;
            int chunkMaxX = worldMaxX / ChunkSize + 1;
            int chunkMinY = worldMinY / ChunkSize;
            int chunkMaxY = worldMaxY / ChunkSize + 1;

            polygonCount = 0;
            voxelCount = 0;
            renderedVoxelCount = 0;
            drawCount = 0;

            int createdChunks = 0;

            Matrix4 viewProjectionMatrix = FastMult(camera.ViewMatrix, camera.ProjectionMatrix);
            for (int chunkX = chunkMinX; chunkX < chunkMaxX; chunkX++)
            {
                for (int chunkY = chunkMinY; chunkY < chunkMaxY; chunkY++)
                {
                    Chunk chunk = worldChunks[chunkX, chunkY];
                    if (chunk == null && createdChunks < 2)
                    {
                        worldChunks[chunkX, chunkY] = chunk = CreateChunk(chunkX * ChunkSize, chunkY * ChunkSize);
                        createdChunks++;
                    }

                    if (chunk != null)
                    {
                        chunk.Render(ref viewProjectionMatrix);
                        polygonCount += chunk.VoxelData.PolygonCount;
                        voxelCount += chunk.VoxelData.VoxelCount;
                        renderedVoxelCount += chunk.VoxelData.RenderedVoxelCount;
                        drawCount++;
                    }
                }
            }

            //for (int s = 0; s < sprites.Count; s++)
            //{
            //    Sprite sprite = sprites[s];

            //    translationMatrix.M41 = sprite.X;
            //    translationMatrix.M42 = sprite.Y;
            //    translationMatrix.M43 = sprite.Z;
            //    Matrix4 viewProjectionModelMatrix = translationMatrix * viewProjectionMatrix;

            //    sprites[s].Render(ref viewProjectionModelMatrix);
            //    drawCount++;
            //    polygonCount += sprites[s].PolygonCount;
            //}
        }

        private int totalChunkCount;
        private Chunk CreateChunk(int chunkX, int chunkY)
        {
            int worldMaxZ = Math.Min(gameWorld.Zsize, ChunkSize);
#if USE_INSTANCED_RENDERING
            InstancedVoxelGroup voxels = new InstancedVoxelGroup(ChunkVoxelSize, ChunkVoxelSize, worldMaxZ * BlockInformation.BlockSize);
#else
            VoxelGroup voxels = new VoxelGroup(ChunkVoxelSize, ChunkVoxelSize, worldMaxZ * BlockInformation.BlockSize);
#endif

            for (int x = 0; x < ChunkSize; x++)
            {
                int worldX = chunkX + x;
                for (int y = 0; y < ChunkSize; y++)
                {
                    int worldY = chunkY + y;
                    for (int z = 0; z < worldMaxZ; z++)
                    {
                        BlockInformation block = blockList[gameWorld.GetBlock(worldX, worldY, z)];
                        if (block != null)
                            voxels.SetVoxels(x * BlockInformation.BlockSize, y * BlockInformation.BlockSize, z * BlockInformation.BlockSize, block.Voxels);
                    }
                }
            }

            totalChunkCount++;
            Debug.WriteLine(totalChunkCount);
            const uint color = 0xFFE0E0E0;
            for (int x = 0; x < ChunkVoxelSize; x++)
            {
                voxels.SetVoxel(x, 0, ChunkVoxelSize - 1, color);
                voxels.SetVoxel(x, ChunkVoxelSize - 1, ChunkVoxelSize - 1, color);
            }

            for (int y = 0; y < ChunkVoxelSize; y++)
            {
                voxels.SetVoxel(0, y, ChunkVoxelSize - 1, color);
                voxels.SetVoxel(ChunkVoxelSize - 1, y, ChunkVoxelSize - 1, color);
            }

            for (int z = 0; z < ChunkVoxelSize; z++)
            {
                voxels.SetVoxel(0, 0, z, color);
                voxels.SetVoxel(ChunkVoxelSize - 1, 0, z, color);
                voxels.SetVoxel(ChunkVoxelSize - 1, ChunkVoxelSize - 1, z, color);
                voxels.SetVoxel(0, ChunkVoxelSize - 1, z, color);
            }

            Matrix4 translationMatrix = Matrix4.CreateTranslation(chunkX * BlockInformation.BlockSize, chunkY * BlockInformation.BlockSize, 0);
            return new Chunk(voxels, ref translationMatrix, chunkX * ChunkSize, chunkY * ChunkSize);
        }

        private static Matrix4 FastMult(Matrix4 left, Matrix4 right)
        {
            return new Matrix4(
                left.M11 * right.M11 + left.M12 * right.M21 + left.M13 * right.M31 + left.M14 * right.M41,
                left.M11 * right.M12 + left.M12 * right.M22 + left.M13 * right.M32 + left.M14 * right.M42,
                left.M11 * right.M13 + left.M12 * right.M23 + left.M13 * right.M33 + left.M14 * right.M43,
                left.M11 * right.M14 + left.M12 * right.M24 + left.M13 * right.M34 + left.M14 * right.M44,
                left.M21 * right.M11 + left.M22 * right.M21 + left.M23 * right.M31 + left.M24 * right.M41,
                left.M21 * right.M12 + left.M22 * right.M22 + left.M23 * right.M32 + left.M24 * right.M42,
                left.M21 * right.M13 + left.M22 * right.M23 + left.M23 * right.M33 + left.M24 * right.M43,
                left.M21 * right.M14 + left.M22 * right.M24 + left.M23 * right.M34 + left.M24 * right.M44,
                left.M31 * right.M11 + left.M32 * right.M21 + left.M33 * right.M31 + left.M34 * right.M41,
                left.M31 * right.M12 + left.M32 * right.M22 + left.M33 * right.M32 + left.M34 * right.M42,
                left.M31 * right.M13 + left.M32 * right.M23 + left.M33 * right.M33 + left.M34 * right.M43,
                left.M31 * right.M14 + left.M32 * right.M24 + left.M33 * right.M34 + left.M34 * right.M44,
                left.M41 * right.M11 + left.M42 * right.M21 + left.M43 * right.M31 + left.M44 * right.M41,
                left.M41 * right.M12 + left.M42 * right.M22 + left.M43 * right.M32 + left.M44 * right.M42,
                left.M41 * right.M13 + left.M42 * right.M23 + left.M43 * right.M33 + left.M44 * right.M43,
                left.M41 * right.M14 + left.M42 * right.M24 + left.M43 * right.M34 + left.M44 * right.M44);
        }

        private static void GetWorldView(ICamera camera, float distance, out int minX, out int maxX, out int minY, out int maxY)
        {
            float hfar = 2.0f * (float)Math.Tan(camera.FoV / 2) * distance;
            float wfar = hfar * camera.AspectRatio;

            Vector3 fc = camera.Location + new Vector3(0, 0, -1) * distance;
            Vector3 topLeft = fc + (Vector3.UnitY * hfar / 2) - (Vector3.UnitX * wfar / 2);
            Vector3 bottomRight = fc - (Vector3.UnitY * hfar / 2) + (Vector3.UnitX * wfar / 2);

            minX = (int)Math.Floor(topLeft.X / BlockInformation.BlockSize);
            maxX = (int)Math.Ceiling(bottomRight.X / BlockInformation.BlockSize);
            minY = (int)Math.Floor(bottomRight.Y / BlockInformation.BlockSize);
            maxY = (int)Math.Ceiling(topLeft.Y / BlockInformation.BlockSize);
        }
    }
}
