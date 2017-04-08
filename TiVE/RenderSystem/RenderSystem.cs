using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVE.Debugging;
using ProdigalSoftware.TiVE.RenderSystem.World;
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
        private const int DeletedItemCacheSize = 2000;
        private static readonly int timeBetweenTimingUpdates = (int)(Stopwatch.Frequency / 2); // 1/2 second
        #endregion

        #region Member variables
        private readonly EntityMeshDeleteQueue deleteQueue = new EntityMeshDeleteQueue(DeletedItemCacheSize);
        private readonly ItemCountsHelper drawCount = new ItemCountsHelper(4, false);
        private readonly ItemCountsHelper voxelCount = new ItemCountsHelper(8, false);
        private readonly ItemCountsHelper renderedVoxelCount = new ItemCountsHelper(8, false);
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
            DeleteAllLoadedMeshes(true);
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

        protected override bool UpdateInternal(int ticksSinceLastFrame, Scene currentScene)
        {
            ticksSinceLastStatUpdate += ticksSinceLastFrame;
            if (ticksSinceLastStatUpdate > timeBetweenTimingUpdates)
            {
                // Time to update render statistics
                drawCount.UpdateDisplayedTime();
                voxelCount.UpdateDisplayedTime();
                renderedVoxelCount.UpdateDisplayedTime();
                ticksSinceLastStatUpdate -= timeBetweenTimingUpdates;
            }

            if (initializeForNewScene)
            {
                DeleteAllLoadedMeshes(false);
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
                LODLevel detailLevel;
                using (new PerformanceLock(renderData.SyncLock))
                {
                    meshData = (IVertexDataCollection)renderData.MeshData;
                    detailLevel = renderData.VisibleVoxelDetailLevel;
                    stats += new RenderStatistics(1, renderData.VoxelCount, renderData.RenderedVoxelCount);
                }

                if (meshData == null)
                    continue; // No data to render with

                RenderVoxelMesh(renderData, currentScene, meshData, detailLevel, cameraData);
            }

            drawCount.PushCount(stats.DrawCount);
            voxelCount.PushCount(stats.VoxelCount);
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
                    renderData.MeshBuilder?.DropMesh();
                    renderData.MeshBuilder = null;
                    renderData.Visible = false;
                    renderData.VoxelDetailLevelToLoad = LODLevel.NotSet;
                }

                loadedEntities.Remove(entity);
                deleteQueue.Enqueue(new EntityDeleteQueueItem(entity));
            }

            while (deleteQueue.Size > DeletedItemCacheSize)
                DeleteEntityMesh(deleteQueue.Dequeue().Entity, true);
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
                        ((IVertexDataCollection)renderData.MeshData)?.Dispose();

                        if (renderData.RenderedVoxelCount == 0)
                            renderData.MeshData = null;
                        else
                        {
                            renderData.MeshData = meshBuilder.GetMesh();
                            ((IVertexDataCollection)renderData.MeshData).Initialize();
                        }

                        meshBuilder.DropMesh(); // Release builder - Must be called after initializing the mesh
                        renderData.MeshBuilder = null;
                        
                        renderData.VisibleVoxelDetailLevel = renderData.VoxelDetailLevelToLoad;
                        renderData.VoxelDetailLevelToLoad = LODLevel.NotSet;
                    }
                }
            }
        }

        private void DeleteAllLoadedMeshes(bool dropMeshes)
        {
            foreach (IEntity entity in loadedEntities)
                DeleteEntityMesh(entity, dropMeshes);
            loadedEntities.Clear();

            while (deleteQueue.Size > 0)
                DeleteEntityMesh(deleteQueue.Dequeue().Entity, dropMeshes);
        }

        /// <summary>
        /// Deletes the mesh data and cached render information for the specified entity. This method does nothing if the specified entitiy has
        /// not had a mesh created yet.
        /// </summary>
        private static void DeleteEntityMesh(IEntity entity, bool dropMesh)
        {
            VoxelMeshComponent renderData = entity.GetComponent<VoxelMeshComponent>();
            using (new PerformanceLock(renderData.SyncLock))
            {
                ((IVertexDataCollection)renderData.MeshData)?.Dispose();

                if (renderData.MeshBuilder != null && dropMesh)
                    renderData.MeshBuilder.DropMesh();

                renderData.MeshData = null;
                renderData.MeshBuilder = null;
                renderData.Visible = false;
                renderData.VisibleVoxelDetailLevel = LODLevel.NotSet;
                renderData.VoxelDetailLevelToLoad = LODLevel.NotSet;
                renderData.VoxelCount = 0;
                renderData.RenderedVoxelCount = 0;
            }

            //ChunkComponent chunkData = entity.GetComponent<ChunkComponent>();
            //if (chunkData != null)
            //    loadedScene.WorldLightProvider.RemoveLightsForChunk(chunkData);
        }
        
        private void RenderVoxelMesh(VoxelMeshComponent renderData, Scene currentScene, IVertexDataCollection meshData, LODLevel detailLevel, CameraComponent cameraData)
        {
            if (detailLevel == LODLevel.NotSet || detailLevel == LODLevel.NumOfLevels)
                throw new ArgumentException("detailLevel invalid: " + detailLevel);

            Debug.Assert(meshData.IsInitialized);

            ShaderProgram shader = shaderManager.GetShaderProgram(ShaderProgram.GetShaderName(false));
            shader.Bind();

            //Matrix4f translationMatrix = Matrix4f.CreateTranslation((int)renderData.Location.X, (int)renderData.Location.Y, (int)renderData.Location.Z);
            Matrix4f translationMatrix;
            Matrix4f.CreateTranslation((int)renderData.Location.X, (int)renderData.Location.Y, (int)renderData.Location.Z, out translationMatrix);

            Matrix4f viewProjectionModelMatrix;
            Matrix4f.Mult(ref translationMatrix, ref cameraData.ViewProjectionMatrix, out viewProjectionModelMatrix);
            shader.SetUniform("matrix_ModelViewProjection", ref viewProjectionModelMatrix);
            shader.SetUniform("modelTranslation", ref renderData.Location);
            shader.SetUniform("cameraLoc", ref cameraData.Location);
            shader.SetUniform("voxelSize", LODUtils.GetRenderedVoxelSize(detailLevel));

            RenderedLight[] lights = GetLightsForMesh(renderData, currentScene);
            shader.SetUniform("lightCount", lights.Length);
            shader.SetUniform("lights", lights);

            TiVEController.Backend.Draw(PrimitiveType.Points, meshData); // Geometry shader will change points into cubes of the correct size
        }

        private static RenderedLight[] GetLightsForMesh(VoxelMeshComponent renderData, Scene currentScene)
        {
            ChunkComponent chunkData = renderData as ChunkComponent;
            if (chunkData != null)
                return currentScene.LightData.GetLightsInChunk(chunkData.ChunkLoc.X, chunkData.ChunkLoc.Y, chunkData.ChunkLoc.Z);

            return currentScene.LightData.GetLightsInChunk(
                (int)renderData.Location.X / ChunkComponent.VoxelSize,
                (int)renderData.Location.Y / ChunkComponent.VoxelSize,
                (int)renderData.Location.Z / ChunkComponent.VoxelSize);
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
