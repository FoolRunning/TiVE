using System.Collections.Generic;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.World;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal sealed class WorldChunkRenderer : IGameWorldRenderer
    {
        private Matrix4 viewProjectionMatrix;
        private readonly HashSet<GameWorldVoxelChunk> chunksToRender = new HashSet<GameWorldVoxelChunk>();

        public void Update(Camera camera, float timeSinceLastFrame)
        {
            viewProjectionMatrix = Matrix4.Mult(camera.ViewMatrix, camera.ProjectionMatrix);

            GameWorld gameWorld = ResourceManager.GameWorldManager.GameWorld;
            chunksToRender.Clear();
            gameWorld.RenderTree.FillChunksToRender(chunksToRender, camera);

            ResourceManager.BlockListManager.UpdateAnimations(timeSinceLastFrame);
            ResourceManager.ChunkManager.Update(chunksToRender);
            ResourceManager.ParticleManager.UpdateCameraPos(chunksToRender);
        }

        public RenderStatistics Draw(Camera camera)
        {
            ResourceManager.ChunkManager.CleanUpChunks();

            RenderStatistics stats = new RenderStatistics();
            foreach (GameWorldVoxelChunk chunk in chunksToRender)
                stats += chunk.Render(ref viewProjectionMatrix);

            //stats += ResourceManager.GameWorldManager.GameWorld.RenderChunks(ref viewProjectionMatrix, camera);
            //stats += ResourceManager.BlockListManager.RenderAnimatedBlocks(ref viewProjectionMatrix, blockMinX, blockMaxX, blockMinY, blockMaxY);
            return stats + ResourceManager.ParticleManager.Render(ref viewProjectionMatrix);
        }
    }
}
