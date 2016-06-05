using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVE.Debugging;
using ProdigalSoftware.TiVE.VoxelMeshSystem;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Internal;
using ProdigalSoftware.TiVEPluginFramework.Components;
using ProdigalSoftware.Utils;
//using ProdigalSoftware.TiVE.Utils;
//#define DEBUG_NODES

namespace ProdigalSoftware.TiVE.RenderSystem
{
    /// <summary>
    /// Engine system that handles entities that contain a render component
    /// </summary>
    internal sealed class RenderSystem : EngineSystem
    {
        #region Constants
        private const int DeletedItemCacheSize = 10000;
        private static readonly int timeBetweenTimingUpdates = (int)(Stopwatch.Frequency / 2); // 1/2 second
        #endregion

        #region Member variables
        private readonly EntityMeshDeleteQueue deleteQueue = new EntityMeshDeleteQueue(DeletedItemCacheSize);
        private readonly ItemCountsHelper drawCount = new ItemCountsHelper(4, false);
        private readonly ItemCountsHelper voxelCount = new ItemCountsHelper(8, false);
        private readonly ItemCountsHelper renderedVoxelCount = new ItemCountsHelper(8, false);
        private readonly ItemCountsHelper polygonCount = new ItemCountsHelper(8, false);
        private readonly ShaderManager shaderManager = new ShaderManager();
        private readonly HashSet<IEntity> loadedEntities = new HashSet<IEntity>();
        private int ticksSinceLastStatUpdate;
        private volatile bool initializeForNewScene;
        #endregion

        #region Constructor
        public RenderSystem() : base("Renderer")
        {
        }
        #endregion

        #region Implementation of EngineSystem
        public override void Dispose()
        {
            DeleteAllLoadedMeshes();
            shaderManager.Dispose();
        }

        public override bool Initialize()
        {
            return shaderManager.Initialize();
        }

        public override void ChangeScene(Scene oldScene, Scene newScene)
        {
            initializeForNewScene = true;
        }

        protected override bool UpdateInternal(int ticksSinceLastFrame, float timeBlendFactor, Scene currentScene)
        {
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

            if (initializeForNewScene)
            {
                DeleteAllLoadedMeshes();
                initializeForNewScene = false;
            }

            CameraComponent cameraData = currentScene.FindCamera();
            if (cameraData == null)
                return true; // No camera to render with or it probably hasn't been initialized yet

            HandleNewlyVisibleEntities(cameraData);
            HandleNewlyHiddenEntities(cameraData);
            HandleEntitiesWithUninitializedMeshes();

            if (currentScene.LoadingInitialChunks)
                return true; // Let chunks load before rendering scene

            RenderStatistics stats = new RenderStatistics();
#if DEBUG_NODES
            stats += RenderSceneDebug(cameraData, currentScene.RenderNode, -1);
#endif
            foreach (IEntity entity in loadedEntities)
            {
                VoxelMeshComponent renderData = entity.GetComponent<VoxelMeshComponent>();
                if (renderData == null)
                    continue;

                IVertexDataCollection meshData;
                int voxelSize;
                using (new PerformanceLock(renderData.SyncLock))
                {
                    meshData = (IVertexDataCollection)renderData.MeshData;
                    voxelSize = 1 << renderData.VisibleVoxelDetailLevel;
                    stats += new RenderStatistics(1, renderData.PolygonCount, renderData.VoxelCount, renderData.RenderedVoxelCount);
                }

                if (meshData == null)
                    continue; // No data to render with

                RenderVoxelMesh(renderData, meshData, voxelSize, ref cameraData.ViewProjectionMatrix);
            }

            drawCount.PushCount(stats.DrawCount);
            voxelCount.PushCount(stats.VoxelCount);
            polygonCount.PushCount(stats.PolygonCount);
            renderedVoxelCount.PushCount(stats.RenderedVoxelCount);

            return true;
        }
        #endregion

        #region Private helper methods
        private void HandleNewlyVisibleEntities(CameraComponent cameraData)
        {
            deleteQueue.Sort(cameraData);

            foreach (IEntity entity in cameraData.NewlyVisibleEntitites.Where(e => e.HasComponent<VoxelMeshComponent>()))
            {
                deleteQueue.Remove(entity);
                loadedEntities.Add(entity);
            }
        }

        private void HandleNewlyHiddenEntities(CameraComponent cameraData)
        {
            foreach (IEntity entity in cameraData.NewlyHiddenEntitites.Where(e => e.HasComponent<VoxelMeshComponent>()))
            {
                VoxelMeshComponent renderData = entity.GetComponent<VoxelMeshComponent>();
                using (new PerformanceLock(renderData.SyncLock))
                {
                    if (renderData.MeshBuilder != null)
                        renderData.MeshBuilder.DropMesh();
                    renderData.MeshBuilder = null;
                    renderData.Visible = false;
                    renderData.VoxelDetailLevelToLoad = VoxelMeshComponent.BlankDetailLevel;
                    renderData.ShadowTypeToLoad = VoxelMeshComponent.BlankShadowType;
                }

                loadedEntities.Remove(entity);
                deleteQueue.Enqueue(new EntityDeleteQueueItem(entity));
            }

            while (deleteQueue.Size > DeletedItemCacheSize)
                DeleteEntityMesh(deleteQueue.Dequeue().Entity);
        }

        private void HandleEntitiesWithUninitializedMeshes()
        {
            foreach (IEntity entity in loadedEntities)
            {
                VoxelMeshComponent renderData = entity.GetComponent<VoxelMeshComponent>();
                using (new PerformanceLock(renderData.SyncLock))
                {
                    IMeshBuilder meshBuilder = renderData.MeshBuilder;
                    if (meshBuilder != null)
                    {
                        if (renderData.MeshData != null)
                            ((IVertexDataCollection)renderData.MeshData).Dispose();

                        if (renderData.PolygonCount == 0)
                            renderData.MeshData = null;
                        else
                        {
                            renderData.MeshData = meshBuilder.GetMesh();
                            ((IVertexDataCollection)renderData.MeshData).Initialize();
                        }

                        meshBuilder.DropMesh(); // Release builder - Must be called after initializing the mesh
                        renderData.MeshBuilder = null;
                        
                        renderData.VisibleVoxelDetailLevel = renderData.VoxelDetailLevelToLoad;
                        renderData.VisibleShadowType = renderData.ShadowTypeToLoad;
                        renderData.VoxelDetailLevelToLoad = VoxelMeshComponent.BlankDetailLevel;
                        renderData.ShadowTypeToLoad = VoxelMeshComponent.BlankShadowType;
                    }
                }
            }
        }

        private void DeleteAllLoadedMeshes()
        {
            foreach (IEntity entity in loadedEntities)
                DeleteEntityMesh(entity);
            loadedEntities.Clear();

            while (deleteQueue.Size > 0)
                DeleteEntityMesh(deleteQueue.Dequeue().Entity);
        }

        /// <summary>
        /// Deletes the mesh data and cached render information for the specified entity. This method does nothing if the specified entitiy has
        /// not had a mesh created yet.
        /// </summary>
        private static void DeleteEntityMesh(IEntity entity)
        {
            VoxelMeshComponent renderData = entity.GetComponent<VoxelMeshComponent>();
            using (new PerformanceLock(renderData.SyncLock))
            {
                if (renderData.MeshData != null)
                    ((IVertexDataCollection)renderData.MeshData).Dispose();

                if (renderData.MeshBuilder != null)
                    renderData.MeshBuilder.DropMesh();

                renderData.MeshData = null;
                renderData.MeshBuilder = null;
                renderData.Visible = false;
                renderData.VisibleVoxelDetailLevel = VoxelMeshComponent.BlankDetailLevel;
                renderData.VisibleShadowType = VoxelMeshComponent.BlankShadowType;
                renderData.VoxelDetailLevelToLoad = VoxelMeshComponent.BlankDetailLevel;
                renderData.ShadowTypeToLoad = VoxelMeshComponent.BlankShadowType;
                renderData.PolygonCount = 0;
                renderData.VoxelCount = 0;
                renderData.RenderedVoxelCount = 0;
            }

            //ChunkComponent chunkData = entity.GetComponent<ChunkComponent>();
            //if (chunkData != null)
            //    loadedScene.LightProvider.RemoveLightsForChunk(chunkData);
        }

        private void RenderVoxelMesh(VoxelMeshComponent renderData, IVertexDataCollection meshData, int voxelSize, ref Matrix4f viewProjectionMatrix)
        {
            if (voxelSize < 1 || voxelSize > Block.VoxelSize)
                throw new ArgumentException("voxelsSize was out-of-range: " + voxelSize);

            Debug.Assert(meshData.IsInitialized);

            IShaderProgram shader = shaderManager.GetShaderProgram(VoxelMeshHelper.Get(false).ShaderName);
            shader.Bind();

            Matrix4f translationMatrix = Matrix4f.CreateTranslation((int)renderData.Location.X, (int)renderData.Location.Y, (int)renderData.Location.Z);

            Matrix4f viewProjectionModelMatrix;
            Matrix4f.Mult(ref translationMatrix, ref viewProjectionMatrix, out viewProjectionModelMatrix);
            shader.SetUniform("matrix_ModelViewProjection", ref viewProjectionModelMatrix);
            shader.SetUniform("voxelSize", voxelSize);

            TiVEController.Backend.Draw(PrimitiveType.Points, meshData);
        }
        #endregion

        #region Debug code
#if DEBUG_NODES
        private RenderStatistics RenderSceneDebug(CameraComponent cameraData, RenderNodeBase node, int locationInParent)
        {
            node.RenderDebugOutline(shaderManager, ref cameraData.ViewProjectionMatrix, locationInParent);

            RenderStatistics stats = new RenderStatistics(1, 12, 0, 0);
            RenderNode renderNode = node as RenderNode;
            if (renderNode != null)
            {
                RenderNodeBase[] childrenLocal = renderNode.ChildNodes;
                for (int i = 0; i < childrenLocal.Length; i++)
                {
                    RenderNodeBase childBox = childrenLocal[i];
                    if (childBox != null && TiVEUtils.BoxInView(cameraData, childBox))
                        stats += RenderSceneDebug(cameraData, childBox, i);
                }
            }
            return stats;
        }
#endif
        #endregion
    }
}
