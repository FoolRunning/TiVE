using System;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal sealed class WorldChunkRenderer : IGameWorldRenderer
    {
        private int blockMinX;
        private int blockMaxX;
        private int blockMinY;
        private int blockMaxY;
        private Matrix4 viewProjectionMatrix;

        public void Update(Camera camera, float timeSinceLastFrame)
        {
            GetWorldView(camera, camera.Location.Z, out blockMinX, out blockMaxX, out blockMinY, out blockMaxY);

            GameWorld gameWorld = ResourceManager.GameWorldManager.GameWorld;
            blockMinX = Math.Max(blockMinX, 0);
            blockMinY = Math.Max(blockMinY, 0);
            blockMaxX = Math.Min(blockMaxX, gameWorld.BlockSize.X);
            blockMaxY = Math.Min(blockMaxY, gameWorld.BlockSize.Y);
            viewProjectionMatrix = Matrix4.Mult(camera.ViewMatrix, camera.ProjectionMatrix);

            ResourceManager.BlockListManager.UpdateAnimations(timeSinceLastFrame);
            ResourceManager.ChunkManager.UpdateCameraPos(blockMinX, blockMaxX, blockMinY, blockMaxY);
            ResourceManager.ParticleManager.UpdateCameraPos(blockMinX, blockMaxX, blockMinY, blockMaxY);
        }

        public RenderStatistics Draw(Camera camera)
        {
            RenderStatistics stats = ResourceManager.ChunkManager.Render(ref viewProjectionMatrix, blockMinX, blockMaxX, blockMinY, blockMaxY);
            //stats = stats + ResourceManager.BlockListManager.RenderAnimatedBlocks(ref viewProjectionMatrix, blockMinX, blockMaxX, blockMinY, blockMaxY);
            return stats + ResourceManager.ParticleManager.Render(ref viewProjectionMatrix);
        }

        private static void GetWorldView(Camera camera, float distance, out int minBlockX, out int maxBlockX, out int minBlockY, out int maxBlockY)
        {
            Vector3 topLeft, bottomRight;
            camera.GetViewPlane(distance, out topLeft, out bottomRight);

            minBlockX = (int)Math.Floor(topLeft.X / BlockInformation.BlockSize);
            maxBlockX = (int)Math.Ceiling(bottomRight.X / BlockInformation.BlockSize);
            minBlockY = (int)Math.Floor(bottomRight.Y / BlockInformation.BlockSize);
            maxBlockY = (int)Math.Ceiling(topLeft.Y / BlockInformation.BlockSize);
        }
    }
}
