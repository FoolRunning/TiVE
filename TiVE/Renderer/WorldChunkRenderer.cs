using System;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.World;
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
            worldMaxX = Math.Min(worldMaxX, gameWorld.BlockSize.X);
            worldMaxY = Math.Min(worldMaxY, gameWorld.BlockSize.Y);
            viewProjectionMatrix = Matrix4.Mult(camera.ViewMatrix, camera.ProjectionMatrix);

            ResourceManager.BlockListManager.UpdateAnimations(timeSinceLastFrame);
            ResourceManager.ChunkManager.UpdateCameraPos(worldMinX, worldMaxX, worldMinY, worldMaxY);
            ResourceManager.ParticleManager.UpdateCameraPos(worldMinX, worldMaxX, worldMinY, worldMaxY);
        }

        public RenderStatistics Draw(Camera camera)
        {
            RenderStatistics stats = ResourceManager.ChunkManager.Render(ref viewProjectionMatrix);
            return stats + ResourceManager.ParticleManager.Render(ref viewProjectionMatrix);
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
