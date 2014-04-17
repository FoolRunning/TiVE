using System;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.Voxels;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVE.Resources;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal sealed class WorldChunkRenderer : IGameWorldRenderer
    {
        public void Update(Camera camera, float timeSinceLastFrame)
        {
            WorldChunkManager chunkManager = ResourceManager.ChunkManager;
            GameWorld gameWorld = ResourceManager.GameWorldManager.GameWorld;

            int worldMinX, worldMaxX, worldMinY, worldMaxY;
            GetWorldView(camera, gameWorld, camera.Location.Z, out worldMinX, out worldMaxX, out worldMinY, out worldMaxY);

            int chunkMinX = worldMinX / GameWorldVoxelChunk.TileSize - 1;
            int chunkMaxX = (int)Math.Ceiling(worldMaxX / (float)GameWorldVoxelChunk.TileSize) + 1;
            int chunkMinY = worldMinY / GameWorldVoxelChunk.TileSize - 1;
            int chunkMaxY = (int)Math.Ceiling(worldMaxY / (float)GameWorldVoxelChunk.TileSize) + 1;
            int chunkMaxZ = Math.Max((int)Math.Ceiling(gameWorld.Zsize / (float)GameWorldVoxelChunk.TileSize), 1);

            //for (int chunkZ = chunkMaxZ - 1; chunkZ >= 0; chunkZ--)
            for (int chunkZ = 0; chunkZ < chunkMaxZ; chunkZ++)
            {
                for (int chunkX = chunkMinX; chunkX < chunkMaxX; chunkX++)
                {
                    for (int chunkY = chunkMinY; chunkY < chunkMaxY; chunkY++)
                    {
                        GameWorldVoxelChunk chunk = chunkManager.GetOrCreateChunk(chunkX, chunkY, chunkZ);
                        if (chunk != null)
                            chunk.Update(timeSinceLastFrame);
                    }
                }
            }
        }

        public void Draw(Camera camera, out RenderStatistics stats)
        {
            WorldChunkManager chunkManager = ResourceManager.ChunkManager;
            GameWorld gameWorld = ResourceManager.GameWorldManager.GameWorld;

            int worldMinX, worldMaxX, worldMinY, worldMaxY;
            GetWorldView(camera, gameWorld, camera.Location.Z, out worldMinX, out worldMaxX, out worldMinY, out worldMaxY);

            int chunkMinX = worldMinX / GameWorldVoxelChunk.TileSize - 1;
            int chunkMaxX = (int)Math.Ceiling(worldMaxX / (float)GameWorldVoxelChunk.TileSize) + 1;
            int chunkMinY = worldMinY / GameWorldVoxelChunk.TileSize - 1;
            int chunkMaxY = (int)Math.Ceiling(worldMaxY / (float)GameWorldVoxelChunk.TileSize) + 1;
            int chunkMaxZ = Math.Max((int)Math.Ceiling(gameWorld.Zsize / (float)GameWorldVoxelChunk.TileSize), 1);

            int polygonCount = 0;
            int voxelCount = 0;
            int renderedVoxelCount = 0;
            int drawCount = 0;

            chunkManager.CleanupChunksOutside(worldMinX, worldMinY, worldMaxX, worldMaxY);
            chunkManager.InitializeChunks();

            Matrix4 viewProjectionMatrix = Matrix4.Mult(camera.ViewMatrix, camera.ProjectionMatrix);
            for (int chunkZ = chunkMaxZ - 1; chunkZ >= 0; chunkZ--)
            {
                for (int chunkX = chunkMinX; chunkX < chunkMaxX; chunkX++)
                {
                    for (int chunkY = chunkMinY; chunkY < chunkMaxY; chunkY++)
                    {
                        GameWorldVoxelChunk chunk = chunkManager.GetOrCreateChunk(chunkX, chunkY, chunkZ);
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
                        GameWorldVoxelChunk chunk = chunkManager.GetOrCreateChunk(chunkX, chunkY, chunkZ);
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

        private static void GetWorldView(Camera camera, IGameWorld gameWorld, float distance, out int minX, out int maxX, out int minY, out int maxY)
        {
            Vector3 topLeft, bottomRight;
            camera.GetViewPlane(distance, out topLeft, out bottomRight);

            minX = (int)Math.Floor(topLeft.X / BlockInformation.BlockSize);
            maxX = (int)Math.Ceiling(bottomRight.X / BlockInformation.BlockSize);
            minY = (int)Math.Floor(bottomRight.Y / BlockInformation.BlockSize);
            maxY = (int)Math.Ceiling(topLeft.Y / BlockInformation.BlockSize);

            minX = Math.Max(minX, 0);
            minY = Math.Max(minY, 0);
            maxX = Math.Min(maxX, gameWorld.Xsize);
            maxY = Math.Min(maxY, gameWorld.Ysize);
        }
    }
}
