using System;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVE.Resources;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal sealed class WorldChunkRenderer : IGameWorldRenderer
    {
        private int worldMinX;
        private int worldMaxX;
        private int worldMinY;
        private int worldMaxY;
        private Matrix4 viewProjectionMatrix;

        public void Update(Camera camera, float timeSinceLastFrame)
        {
            GetWorldView(camera, camera.Location.Z, out worldMinX, out worldMaxX, out worldMinY, out worldMaxY);

            GameWorld gameWorld = ResourceManager.GameWorldManager.GameWorld;
            worldMinX = Math.Max(worldMinX, 0);
            worldMinY = Math.Max(worldMinY, 0);
            worldMaxX = Math.Min(worldMaxX, gameWorld.Xsize);
            worldMaxY = Math.Min(worldMaxY, gameWorld.Ysize);
            viewProjectionMatrix = Matrix4.Mult(camera.ViewMatrix, camera.ProjectionMatrix);

            ResourceManager.LightManager.UpdateCameraPos(worldMinX, worldMaxX, worldMinY, worldMaxY);
            ResourceManager.ChunkManager.UpdateCameraPos(worldMinX, worldMaxX, worldMinY, worldMaxY);
            ResourceManager.ParticleManager.UpdateCameraPos(worldMinX, worldMaxX, worldMinY, worldMaxY);
        }

        public void Draw(Camera camera, out RenderStatistics stats)
        {
            stats = ResourceManager.ChunkManager.Render(ref viewProjectionMatrix);
            stats += ResourceManager.ParticleManager.Render(ref viewProjectionMatrix);

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
        }

        private static void GetWorldView(Camera camera, float distance, out int minX, out int maxX, out int minY, out int maxY)
        {
            Vector3 topLeft, bottomRight;
            camera.GetViewPlane(distance, out topLeft, out bottomRight);

            minX = (int)Math.Floor(topLeft.X / BlockInformation.BlockSize);
            maxX = (int)Math.Ceiling(bottomRight.X / BlockInformation.BlockSize);
            minY = (int)Math.Floor(bottomRight.Y / BlockInformation.BlockSize);
            maxY = (int)Math.Ceiling(topLeft.Y / BlockInformation.BlockSize);
        }
    }
}
