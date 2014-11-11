using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.Particles;
using ProdigalSoftware.TiVE.Renderer.World;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal sealed class WorldChunkRenderer : IGameWorldRenderer
    {
        private readonly HashSet<GameWorldVoxelChunk> chunksToRender = new HashSet<GameWorldVoxelChunk>();
        private readonly int maxChunkCreationThreads;
        private WorldChunkManager chunkManager;
        private ParticleSystemManager particleManager;
        private ShaderManager shaderManager;
        private GameWorld gameWorld;
        private BlockList blockList;
        private Matrix4 viewProjectionMatrix;

        public WorldChunkRenderer(int maxChunkCreationThreads)
        {
            this.maxChunkCreationThreads = maxChunkCreationThreads;
        }

        public void Dispose()
        {
            Debug.Assert(Thread.CurrentThread.Name == "Main UI");

            if (particleManager != null)
                particleManager.Dispose();
            if (chunkManager != null)
                chunkManager.Dispose();
            if (gameWorld != null)
                gameWorld.Dispose();
            if (blockList != null)
                blockList.Dispose();
            if (shaderManager != null)
                shaderManager.Dispose();
        }

        public GameWorld GameWorld
        {
            get { return gameWorld; }
        }

        public BlockList BlockList
        {
            get { return blockList; }
        }

        public void SetGameWorld(BlockList newBlockList, GameWorld newGameWorld)
        {
            if (newBlockList == null)
                throw new ArgumentNullException("newBlockList");
            if (newGameWorld == null)
                throw new ArgumentNullException("newGameWorld");

            Dispose();

            blockList = newBlockList;
            gameWorld = newGameWorld;
            chunkManager = new WorldChunkManager(newGameWorld, maxChunkCreationThreads);
            particleManager = new ParticleSystemManager(newGameWorld);
            shaderManager = new ShaderManager();
            shaderManager.Initialize();
        }

        public void RefreshLevel()
        {
            if (chunkManager != null)
                chunkManager.ReloadAllChunks();
        }

        public void Update(Camera camera, float timeSinceLastFrame)
        {
            viewProjectionMatrix = Matrix4.Mult(camera.ViewMatrix, camera.ProjectionMatrix);

            chunksToRender.Clear();
            gameWorld.RenderTree.FillChunksToRender(chunksToRender, camera);

            particleManager.UpdateCameraPos(chunksToRender);
            blockList.UpdateAnimations(timeSinceLastFrame);
            chunkManager.Update(chunksToRender);
        }

        public RenderStatistics Draw(Camera camera)
        {
            chunkManager.CleanUpChunks();

            RenderStatistics stats = new RenderStatistics();
            foreach (GameWorldVoxelChunk chunk in chunksToRender)
                stats += chunk.Render(shaderManager, ref viewProjectionMatrix);

            //stats += gameWorld.RenderChunkOutlines(shaderManager, ref viewProjectionMatrix, camera);
            //stats += blockList.RenderAnimatedBlocks(gameWorld, shaderManager, ref viewProjectionMatrix, blockMinX, blockMaxX, blockMinY, blockMaxY);
            return stats + particleManager.Render(shaderManager, ref viewProjectionMatrix);
        }
    }
}
