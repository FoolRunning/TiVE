using System;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal sealed class WorldChunkRenderer : IGameWorldRenderer
    {
        private readonly GameWorld gameWorld;
        private readonly ChunkCache chunkCache;

        public WorldChunkRenderer(GameWorld gameWorld, BlockList blockList)
        {
            chunkCache = new ChunkCache(gameWorld, blockList);
            this.gameWorld = gameWorld;
        }

        public void Dispose()
        {
            chunkCache.Dispose();
        }

        public void Draw(Camera camera, out RenderStatistics stats)
        {
            int worldMinX, worldMaxX, worldMinY, worldMaxY;
            GetWorldView(camera, camera.Location.Z, out worldMinX, out worldMaxX, out worldMinY, out worldMaxY);

            worldMinX = Math.Max(worldMinX, 0);
            worldMinY = Math.Max(worldMinY, 0);
            worldMaxX = Math.Min(worldMaxX, gameWorld.Xsize);
            worldMaxY = Math.Min(worldMaxY, gameWorld.Ysize);

            int chunkMinX = worldMinX / Chunk.TileSize;
            int chunkMaxX = worldMaxX / Chunk.TileSize + 1;
            int chunkMinY = worldMinY / Chunk.TileSize;
            int chunkMaxY = worldMaxY / Chunk.TileSize + 1;
            int chunkMaxZ = Math.Max(gameWorld.Zsize / Chunk.TileSize, 1);

            int polygonCount = 0;
            int voxelCount = 0;
            int renderedVoxelCount = 0;
            int drawCount = 0;

            chunkCache.CleanupChunksOutside(worldMinX, worldMinY, worldMaxX, worldMaxY);
            chunkCache.InitializeChunks();

            Matrix4 viewProjectionMatrix = FastMult(camera.ViewMatrix, camera.ProjectionMatrix);
            for (int chunkZ = chunkMaxZ - 1; chunkZ >= 0; chunkZ--)
            {
                for (int chunkX = chunkMinX; chunkX < chunkMaxX; chunkX++)
                {
                    for (int chunkY = chunkMinY; chunkY < chunkMaxY; chunkY++)
                    {
                        Chunk chunk = chunkCache.GetOrCreateChunk(chunkX, chunkY, chunkZ);
                        if (chunk != null)
                        {
                            chunk.Render(ref viewProjectionMatrix);
                            var vData = chunk.VoxelData;
                            if (vData != null)
                            {
                                polygonCount += vData.PolygonCount;
                                voxelCount += vData.VoxelCount;
                                renderedVoxelCount += vData.RenderedVoxelCount;
                                drawCount++;
                            }
                        }
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
            stats = new RenderStatistics(drawCount, polygonCount, voxelCount, renderedVoxelCount);
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
