using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVE.Debugging;
using ProdigalSoftware.TiVE.RenderSystem.Voxels;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVE.Utils;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.RenderSystem
{
    /// <summary>
    /// Engine system that handles entities that contain a render component
    /// </summary>
    internal sealed class RenderSystem : EngineSystem
    {
        #region Constants
        private static readonly int timeBetweenTimingUpdates = (int)(Stopwatch.Frequency / 2); // 1/2 second
        #endregion

        #region Member variables
        private readonly ItemCountsHelper drawCount = new ItemCountsHelper(4, false);
        private readonly ItemCountsHelper voxelCount = new ItemCountsHelper(8, false);
        private readonly ItemCountsHelper renderedVoxelCount = new ItemCountsHelper(8, false);
        private readonly ItemCountsHelper polygonCount = new ItemCountsHelper(8, false);
        private readonly ShaderManager shaderManager = new ShaderManager();
        private Scene previousScene;
        private VoxelMeshManager meshManager;
        private int ticksSinceLastStatUpdate;
        #endregion

        #region Constructor
        public RenderSystem() : base("Renderer")
        {
        }
        #endregion

        #region Implementation of EngineSystem
        public override void Dispose()
        {
            if (meshManager != null)
                meshManager.Dispose();
            shaderManager.Dispose();

            meshManager = null;
            previousScene = null;
        }

        public override bool Initialize()
        {
            int maxThreads = TiVEController.UserSettings.Get(UserSettings.ChunkCreationThreadsKey);
            meshManager = new VoxelMeshManager(maxThreads);
            
            return shaderManager.Initialize();
        }

        protected override bool UpdateInternal(int ticksSinceLastFrame, float timeBlendFactor, Scene currentScene)
        {
            if (currentScene == null)
                return true;

            if (currentScene != previousScene)
            {
                currentScene.LoadingInitialChunks = true;
                previousScene = currentScene;
            }

            ticksSinceLastStatUpdate += ticksSinceLastFrame;
            if (ticksSinceLastStatUpdate > timeBetweenTimingUpdates)
            {
                // Time to update render statistics
                drawCount.UpdateDisplayedTime();
                voxelCount.UpdateDisplayedTime();
                renderedVoxelCount.UpdateDisplayedTime();
                polygonCount.UpdateDisplayedTime();
                ticksSinceLastStatUpdate -= timeBetweenTimingUpdates;
            }

            CameraComponent cameraData = FindCamera(currentScene);
            if (cameraData == null)
                return true; // No camera to render with or it probably hasn't been initialized yet

            HashSet<IEntity> entitiesToRender = cameraData.VisibleEntitites;
            meshManager.LoadMeshesForEntities(entitiesToRender, cameraData, currentScene);

            if (currentScene.LoadingInitialChunks)
            {
                if (meshManager.ChunkLoadCount < 10) // Let chunks load before showing scene
                    currentScene.LoadingInitialChunks = false;
                return true;
            }

            RenderStatistics stats = new RenderStatistics();
            //stats += RenderSceneDebug(cameraData, currentScene.RenderNode, -1);

            foreach (IEntity entity in entitiesToRender)
            {
                RenderComponent renderData = entity.GetComponent<RenderComponent>();
                if (renderData == null)
                    continue;

                IVertexDataCollection meshData;
                using (new PerformanceLock(renderData.SyncLock))
                    meshData = (IVertexDataCollection)renderData.MeshData;

                if (meshData == null)
                    continue; // No data to render with

                stats += RenderEntityMesh(renderData, meshData, ref cameraData.ViewProjectionMatrix);
            }

            drawCount.PushCount(stats.DrawCount);
            voxelCount.PushCount(stats.VoxelCount);
            polygonCount.PushCount(stats.PolygonCount);
            renderedVoxelCount.PushCount(stats.RenderedVoxelCount);

            return true;
        }
        #endregion

        #region Private helper methods
        /// <summary>
        /// Finds the first enabled camera in the specified scene
        /// </summary>
        private static CameraComponent FindCamera(Scene scene)
        {
            foreach (IEntity cameraEntity in scene.GetEntitiesWithComponent<CameraComponent>())
            {
                CameraComponent cameraData = cameraEntity.GetComponent<CameraComponent>();
                Debug.Assert(cameraData != null);

                if (cameraData.Enabled && cameraData.ViewProjectionMatrix != Matrix4f.Zero)
                    return cameraData;
            }
            return null;
        }

        private RenderStatistics RenderEntityMesh(RenderComponent renderData, IVertexDataCollection meshData, ref Matrix4f viewProjectionMatrix)
        {
            Debug.Assert(meshData.IsInitialized);

            IShaderProgram shader = shaderManager.GetShaderProgram(VoxelMeshHelper.Get(false).ShaderName);
            shader.Bind();

            Matrix4f translationMatrix = Matrix4f.CreateTranslation((int)renderData.Location.X, (int)renderData.Location.Y, (int)renderData.Location.Z);

            Matrix4f viewProjectionModelMatrix;
            Matrix4f.Mult(ref translationMatrix, ref viewProjectionMatrix, out viewProjectionModelMatrix);
            shader.SetUniform("matrix_ModelViewProjection", ref viewProjectionModelMatrix);

            TiVEController.Backend.Draw(PrimitiveType.Triangles, meshData);
            return new RenderStatistics(1, renderData.PolygonCount, renderData.VoxelCount, renderData.RenderedVoxelCount);
        }

        [UsedImplicitly]
        private RenderStatistics RenderSceneDebug(CameraComponent cameraData, RenderNode node, int locationInParent)
        {
            node.RenderDebugOutline(shaderManager, ref cameraData.ViewProjectionMatrix, locationInParent);

            RenderStatistics stats = new RenderStatistics(1, 12, 0, 0);
            RenderNode[] childrenLocal = node.ChildNodes;
            for (int i = 0; i < childrenLocal.Length; i++)
            {
                RenderNode childBox = childrenLocal[i];
                if (childBox != null && TiVEUtils.BoxInView(cameraData, childBox.BoundingBox))
                    stats += RenderSceneDebug(cameraData, childBox, i);
            }

            return stats;
        }
        #endregion
    }
}
