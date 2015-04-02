using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private const float NearDist = 0.1f;

        private const int Top = 0;
        private const int Bottom = 1;
        private const int Left = 2;
        private const int Right = 3;
        private const int Near = 4;
        private const int Far = 5;
        #endregion

        #region Member variables
        private readonly ItemCountsHelper drawCount = new ItemCountsHelper(4, false);
        private readonly ItemCountsHelper voxelCount = new ItemCountsHelper(8, false);
        private readonly ItemCountsHelper renderedVoxelCount = new ItemCountsHelper(8, false);
        private readonly ItemCountsHelper polygonCount = new ItemCountsHelper(8, false);
        private readonly HashSet<IEntity> entitiesToRender = new HashSet<IEntity>();
        private readonly Plane[] frustrumPlanes = new Plane[6];
        private readonly ShaderManager shaderManager = new ShaderManager();
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
            entitiesToRender.Clear();

            meshManager = null;
        }

        public override bool Initialize()
        {
            int maxThreads = TiVEController.UserSettings.Get(UserSettings.ChunkCreationThreadsKey);
            meshManager = new VoxelMeshManager(maxThreads);
            
            return shaderManager.Initialize();
        }

        protected override void UpdateInternal(int ticksSinceLastFrame, Scene currentScene)
        {
            if (currentScene == null)
                return;

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
            if (cameraData == null || cameraData.Location == cameraData.LookAtLocation)
                return; // No camera to render with or it probably hasn't been initialized yet

            UpdateFrustrum(cameraData);

            Matrix4f viewProjectionMatrix;
            CalculateViewProjectionMatrix(cameraData, out viewProjectionMatrix);

            entitiesToRender.Clear();
            FillEntitiesToRender(currentScene.RenderNode);
            meshManager.LoadMeshesForEntities(entitiesToRender, cameraData, currentScene);

            RenderStatistics stats = new RenderStatistics();
            //stats += RenderSceneDebug(currentScene.RenderNode, ref viewProjectionMatrix, -1);

            foreach (IEntity renderEntity in entitiesToRender)
            {
                RenderComponent renderData = renderEntity.GetComponent<RenderComponent>();
                Debug.Assert(renderData != null);

                IVertexDataCollection meshData;
                using (new PerformanceLock(renderData.SyncLock))
                    meshData = (IVertexDataCollection)renderData.MeshData;

                if (meshData == null)
                    continue; // No data to render with

                stats += RenderEntityMesh(renderData, meshData, ref viewProjectionMatrix);
            }

            drawCount.PushCount(stats.DrawCount);
            voxelCount.PushCount(stats.VoxelCount);
            polygonCount.PushCount(stats.PolygonCount);
            renderedVoxelCount.PushCount(stats.RenderedVoxelCount);
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

                if (cameraData.Enabled) 
                    return cameraData;
            }
            return null;
        }

        /// <summary>
        /// Calcualates the view projection matrix from the specified camera data
        /// </summary>
        private static void CalculateViewProjectionMatrix(CameraComponent cameraData, out Matrix4f viewProjectionMatrix)
        {
            Matrix4f projectionMatrix;
            Matrix4f.CreatePerspectiveFieldOfView(cameraData.FieldOfView, cameraData.AspectRatio, NearDist, cameraData.FarDistance, out projectionMatrix);
            
            Matrix4f viewMatrix;
            Matrix4f.LookAt(cameraData.Location, cameraData.LookAtLocation, cameraData.UpVector, out viewMatrix);
            
            Matrix4f.Mult(ref viewMatrix, ref projectionMatrix, out viewProjectionMatrix);
        }

        /// <summary>
        /// Updates the cached frustrum data with the data from the specified camera
        /// </summary>
        private void UpdateFrustrum(CameraComponent cameraData)
        {
            float nearHeight = NearDist * (float)Math.Tan(cameraData.FieldOfView * 0.5f);
            float nearWidth = nearHeight * cameraData.AspectRatio;

            Vector3f zAxis = cameraData.Location - cameraData.LookAtLocation;
            zAxis.Normalize();
            Vector3f xAxis = Vector3f.Cross(cameraData.UpVector, zAxis);
            xAxis.Normalize();
            Vector3f yAxis = Vector3f.Cross(zAxis, xAxis);

            Vector3f nearCenter = cameraData.Location - zAxis * NearDist;
            Vector3f farCenter = cameraData.Location - zAxis * cameraData.FarDistance;

            // Calculate frustrum planes
            frustrumPlanes[Near] = new Plane(-zAxis, nearCenter);
            frustrumPlanes[Far] = new Plane(zAxis, farCenter);

            Vector3f normal = (nearCenter + yAxis * nearHeight) - cameraData.Location;
            normal.Normalize();
            frustrumPlanes[Top] = new Plane(Vector3f.Cross(normal, xAxis), nearCenter + yAxis * nearHeight);

            normal = (nearCenter - yAxis * nearHeight) - cameraData.Location;
            normal.Normalize();
            frustrumPlanes[Bottom] = new Plane(Vector3f.Cross(xAxis, normal), nearCenter - yAxis * nearHeight);

            normal = (nearCenter - xAxis * nearWidth) - cameraData.Location;
            normal.Normalize();
            frustrumPlanes[Left] = new Plane(Vector3f.Cross(normal, yAxis), nearCenter - xAxis * nearWidth);

            normal = (nearCenter + xAxis * nearWidth) - cameraData.Location;
            normal.Normalize();
            frustrumPlanes[Right] = new Plane(Vector3f.Cross(yAxis, normal), nearCenter + xAxis * nearWidth);
        }

        /// <summary>
        /// Fills the specified HashSet with entities that should be rendered based on the current location and orientation of the camera
        /// </summary>
        private void FillEntitiesToRender(RenderNode node)
        {
            if (node.Entities != null)
            {
                entitiesToRender.UnionWith(node.Entities);
                return;
            }

            RenderNode[] childrenLocal = node.ChildNodes;
            for (int i = 0; i < childrenLocal.Length; i++)
            {
                RenderNode childNode = childrenLocal[i];
                if (childNode != null)
                {
                    if (BoxInView(childNode.BoundingBox))
                        FillEntitiesToRender(childNode);
                }
            }
        }

        /// <summary>
        /// Determines if the specified bounding box is visible from the current location and orientation of the camera
        /// </summary>
        private bool BoxInView(BoundingBox box)
        {
            for (int i = 0; i < frustrumPlanes.Length; i++)
            {
                if (frustrumPlanes[i].DistanceFromPoint(box.GetPositivePoint(frustrumPlanes[i].PlaneNormal)) < 0)
                    return false;
            }
            return true;
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

        private RenderStatistics RenderSceneDebug(RenderNode node, ref Matrix4f viewProjectionMatrix, int locationInParent)
        {
            node.RenderDebugOutline(shaderManager, ref viewProjectionMatrix, locationInParent);

            RenderStatistics stats = new RenderStatistics(1, 12, 0, 0);
            RenderNode[] childrenLocal = node.ChildNodes;
            for (int i = 0; i < childrenLocal.Length; i++)
            {
                RenderNode childBox = childrenLocal[i];
                if (childBox != null && BoxInView(childBox.BoundingBox))
                    stats += RenderSceneDebug(childBox, ref viewProjectionMatrix, i);
            }

            return stats;
        }
        #endregion
    }
}
