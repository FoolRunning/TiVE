using System;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.Voxels;
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

        public void Update(Camera camera, float timeSinceLastFrame)
        {
            int worldMinX, worldMaxX, worldMinY, worldMaxY;
            GetWorldView(camera, camera.Location.Z, out worldMinX, out worldMaxX, out worldMinY, out worldMaxY);

            worldMinX = Math.Max(worldMinX, 0);
            worldMinY = Math.Max(worldMinY, 0);
            worldMaxX = Math.Min(worldMaxX, gameWorld.Xsize);
            worldMaxY = Math.Min(worldMaxY, gameWorld.Ysize);

            int chunkMinX = worldMinX / GameWorldVoxelChunk.TileSize - 1;
            int chunkMaxX = worldMaxX / GameWorldVoxelChunk.TileSize + 1;
            int chunkMinY = worldMinY / GameWorldVoxelChunk.TileSize - 1;
            int chunkMaxY = worldMaxY / GameWorldVoxelChunk.TileSize + 1;
            int chunkMaxZ = Math.Max(gameWorld.Zsize / GameWorldVoxelChunk.TileSize, 1);

            //for (int chunkZ = chunkMaxZ - 1; chunkZ >= 0; chunkZ--)
            for (int chunkZ = 0; chunkZ < chunkMaxZ; chunkZ++)
            {
                for (int chunkX = chunkMinX; chunkX < chunkMaxX; chunkX++)
                {
                    for (int chunkY = chunkMinY; chunkY < chunkMaxY; chunkY++)
                    {
                        GameWorldVoxelChunk chunk = chunkCache.GetOrCreateChunk(chunkX, chunkY, chunkZ);
                        if (chunk != null)
                            chunk.Update(timeSinceLastFrame);
                    }
                }
            }
        }

        public void Draw(Camera camera, out RenderStatistics stats)
        {
            int worldMinX, worldMaxX, worldMinY, worldMaxY;
            GetWorldView(camera, camera.Location.Z, out worldMinX, out worldMaxX, out worldMinY, out worldMaxY);

            worldMinX = Math.Max(worldMinX, 0);
            worldMinY = Math.Max(worldMinY, 0);
            worldMaxX = Math.Min(worldMaxX, gameWorld.Xsize);
            worldMaxY = Math.Min(worldMaxY, gameWorld.Ysize);

            int chunkMinX = worldMinX / GameWorldVoxelChunk.TileSize - 1;
            int chunkMaxX = worldMaxX / GameWorldVoxelChunk.TileSize + 1;
            int chunkMinY = worldMinY / GameWorldVoxelChunk.TileSize - 1;
            int chunkMaxY = worldMaxY / GameWorldVoxelChunk.TileSize + 1;
            int chunkMaxZ = Math.Max(gameWorld.Zsize / GameWorldVoxelChunk.TileSize, 1);

            int polygonCount = 0;
            int voxelCount = 0;
            int renderedVoxelCount = 0;
            int drawCount = 0;

            chunkCache.CleanupChunksOutside(worldMinX, worldMinY, worldMaxX, worldMaxY);
            chunkCache.InitializeChunks();

            Matrix4 viewProjectionMatrix = Matrix4.Mult(camera.ViewMatrix, camera.ProjectionMatrix);
            for (int chunkZ = chunkMaxZ - 1; chunkZ >= 0; chunkZ--)
            {
                for (int chunkX = chunkMinX; chunkX < chunkMaxX; chunkX++)
                {
                    for (int chunkY = chunkMinY; chunkY < chunkMaxY; chunkY++)
                    {
                        GameWorldVoxelChunk chunk = chunkCache.GetOrCreateChunk(chunkX, chunkY, chunkZ);
                        if (chunk != null)
                        {
                            RenderStatistics chunkStats = chunk.RenderOpaque(ref viewProjectionMatrix);
                            polygonCount += chunkStats.PolygonCount;
                            voxelCount += chunkStats.VoxelCount;
                            renderedVoxelCount += chunkStats.RenderedVoxelCount;
                            drawCount += chunkStats.DrawCount;
                        }
                    }
                }
            }

            TiVEController.Backend.DisableDepthWriting();
            for (int chunkZ = chunkMaxZ - 1; chunkZ >= 0; chunkZ--)
            {
                for (int chunkX = chunkMinX; chunkX < chunkMaxX; chunkX++)
                {
                    for (int chunkY = chunkMinY; chunkY < chunkMaxY; chunkY++)
                    {
                        GameWorldVoxelChunk chunk = chunkCache.GetOrCreateChunk(chunkX, chunkY, chunkZ);
                        if (chunk != null)
                        {
                            RenderStatistics chunkStats = chunk.RenderTransparent(ref viewProjectionMatrix);
                            polygonCount += chunkStats.PolygonCount;
                            voxelCount += chunkStats.VoxelCount;
                            renderedVoxelCount += chunkStats.RenderedVoxelCount;
                            drawCount += chunkStats.DrawCount;
                        }
                    }
                }
            }
            TiVEController.Backend.EnableDepthWriting();

            //for (int s = 0; s < sprites.Count; s++)
            //{
            //    Sprite sprite = sprites[s];

            //    translationMatrix.M41 = sprite.X;
            //    translationMatrix.M42 = sprite.Y;
            //    translationMatrix.M43 = sprite.Z;
            //    Matrix4 viewProjectionModelMatrix = translationMatrix * viewProjectionMatrix;

            //    sprites[s].RenderOpaque(ref viewProjectionModelMatrix);
            //    drawCount++;
            //    polygonCount += sprites[s].PolygonCount;
            //}
            stats = new RenderStatistics(drawCount, polygonCount, voxelCount, renderedVoxelCount);
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
