using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.Lighting;
using ProdigalSoftware.TiVE.Renderer.Particles;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal sealed class WorldChunkRenderer : IGameWorldRenderer
    {
        private readonly HashSet<GameWorldVoxelChunk> chunksToRender = new HashSet<GameWorldVoxelChunk>();
        private readonly int maxChunkCreationThreads;
        private Camera camera;
        private WorldChunkManager chunkManager;
        private ParticleSystemManager particleManager;
        private ShaderManager shaderManager;
        private Matrix4 viewProjectionMatrix;
        private ChunkRenderTree renderTree;

        public WorldChunkRenderer(int maxChunkCreationThreads)
        {
            this.maxChunkCreationThreads = maxChunkCreationThreads;
            camera = new Camera();
        }

        public void Dispose()
        {
            Debug.Assert(renderTree == null || Thread.CurrentThread.Name == "Main UI");

            chunksToRender.Clear();

            if (particleManager != null)
                particleManager.Dispose();
            if (chunkManager != null)
                chunkManager.Dispose();
            if (renderTree != null)
                renderTree.Dispose();
            if (BlockList != null)
                BlockList.Dispose();
            if (shaderManager != null)
                shaderManager.Dispose();

            BlockList = null;
            GameWorld = null;
            LightProvider = null;
            particleManager = null;
            chunkManager = null;
            renderTree = null;
            shaderManager = null;

            GC.Collect();
        }

        public Camera Camera 
        {
            get { return camera; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                camera = value;
            }
        }

        public GameWorld GameWorld { get; private set; }

        public BlockList BlockList { get; private set; }

        public LightProvider LightProvider { get; private set; }

        public void SetGameWorld(BlockList newBlockList, GameWorld newGameWorld)
        {
            if (newBlockList == null)
                throw new ArgumentNullException("newBlockList");
            if (newGameWorld == null)
                throw new ArgumentNullException("newGameWorld");

            Dispose();

            shaderManager = new ShaderManager();
            shaderManager.Initialize();

            BlockList = newBlockList;
            GameWorld = newGameWorld;
            LightProvider = LightProvider.Get(newGameWorld);

            renderTree = new ChunkRenderTree(newGameWorld);
            chunkManager = new WorldChunkManager(this, maxChunkCreationThreads);
            particleManager = new ParticleSystemManager(this);
        }

        public void RefreshLevel()
        {
            if (chunkManager != null)
                chunkManager.ReloadAllChunks();
        }
        
        public void Update(float timeSinceLastFrame)
        {
            camera.Update();

            viewProjectionMatrix = Matrix4.Mult(camera.ViewMatrix, camera.ProjectionMatrix);

            chunksToRender.Clear();
            renderTree.FillChunksToRender(chunksToRender, camera);

            particleManager.UpdateCameraPos(chunksToRender);
            if (!TiVEController.UserSettings.Get(UserSettings.UseThreadedParticlesKey))
                particleManager.UpdateParticles(timeSinceLastFrame);

            chunkManager.Update(chunksToRender, camera);
            BlockList.UpdateAnimations(timeSinceLastFrame);
        }

        public RenderStatistics Draw()
        {
            chunkManager.CleanUpChunks();

            RenderStatistics stats = new RenderStatistics();
            foreach (GameWorldVoxelChunk chunk in chunksToRender)
                stats += chunk.Render(shaderManager, ref viewProjectionMatrix);

            //stats += renderTree.Render(shaderManager, ref viewProjectionMatrix, camera);
            //stats += blockList.RenderAnimatedBlocks(gameWorld, shaderManager, ref viewProjectionMatrix, blockMinX, blockMaxX, blockMinY, blockMaxY);
            return stats + particleManager.Render(shaderManager, ref viewProjectionMatrix);
        }
    }
}
