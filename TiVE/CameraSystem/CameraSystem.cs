using System;
using System.Collections.Generic;
using System.Linq;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVE.RenderSystem;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;

namespace ProdigalSoftware.TiVE.CameraSystem
{
    /// <summary>
    /// Engine system that handles entities that contain a camera component
    /// </summary>
    internal sealed class CameraSystem : EngineSystem
    {
        private readonly HashSet<IEntity> visibleEntities = new HashSet<IEntity>();

        #region Constructor
        public CameraSystem() : base("Camera")
        {
        }
        #endregion

        #region Implementation of EngineSystem
        public override void Dispose()
        {
        }

        public override bool Initialize()
        {
            return true;
        }

        public override void ChangeScene(Scene oldScene, Scene newScene)
        {
            visibleEntities.Clear();
        }

        protected override bool UpdateInternal(int ticksSinceLastUpdate, Scene currentScene)
        {
            CameraComponent cameraData;
            try
            {
                cameraData = currentScene.GetEntitiesWithComponent<CameraComponent>()
                    .Select(entity => entity.GetComponent<CameraComponent>())
                    .SingleOrDefault(c => c.Enabled && c.Location != c.LookAtLocation); // Make sure camera is enabled and has been initialized
            }
            catch (InvalidOperationException)
            {
                Messages.AddError("More than one enabled camera found in scene");
                return false;
            }
            
            if (cameraData == null)
                return true;

            // Calculate the projection matrix
            float aspectRatio = cameraData.AspectRatio;
            if (aspectRatio == 0.0f)
                aspectRatio = TiVEController.Engine.WindowClientBounds.Width / (float)TiVEController.Engine.WindowClientBounds.Height;
            
            UpdateFrustrum(cameraData, aspectRatio);

            Matrix4f projectionMatrix;
            Matrix4f.CreatePerspectiveFieldOfView(cameraData.FieldOfView, aspectRatio, CameraComponent.NearDist, cameraData.FarDistance, out projectionMatrix);

            // Calculate the view matrix
            Matrix4f viewMatrix;
            Matrix4f.LookAt(ref cameraData.Location, ref cameraData.LookAtLocation, ref cameraData.UpVector, out viewMatrix);
            
            // Create and cache the view projection matrix
            Matrix4f.Mult(ref viewMatrix, ref projectionMatrix, out cameraData.ViewProjectionMatrix);

            // Determine what entities are visible from this camera
            cameraData.VisibleEntitites.Clear();
            FindVisibleEntities(cameraData.VisibleEntitites, cameraData, currentScene.RenderNode);

            cameraData.NewlyHiddenEntitites.Clear();
            foreach (IEntity entity in visibleEntities)
            {
                if (!cameraData.VisibleEntitites.Contains(entity))
                    cameraData.NewlyHiddenEntitites.Add(entity);
            }

            cameraData.NewlyVisibleEntitites.Clear();
            foreach (IEntity entity in cameraData.VisibleEntitites)
            {
                if (!visibleEntities.Contains(entity))
                    cameraData.NewlyVisibleEntitites.Add(entity);
            }

            visibleEntities.Clear();
            visibleEntities.UnionWith(cameraData.VisibleEntitites);
            return true;
        }
        #endregion

        #region Private helper methods
        /// <summary>
        /// Updates the cached frustrum data with the data from the specified camera
        /// </summary>
        private static void UpdateFrustrum(CameraComponent cameraData, float aspectRatio)
        {
            float nearHeight = CameraComponent.NearDist * (float)Math.Tan(cameraData.FieldOfView * 0.5f);
            float nearWidth = nearHeight * aspectRatio;
            
            Vector3f zAxis = cameraData.Location - cameraData.LookAtLocation;
            zAxis.Normalize();
            Vector3f xAxis;
            Vector3f.Cross(ref cameraData.UpVector, ref zAxis, out xAxis);
            xAxis.Normalize();
            Vector3f yAxis;
            Vector3f.Cross(ref zAxis, ref xAxis, out yAxis);

            Vector3f nearCenter = cameraData.Location - zAxis * CameraComponent.NearDist;
            Vector3f farCenter = cameraData.Location - zAxis * cameraData.FarDistance;

            // Calculate frustrum planes
            cameraData.FrustrumPlanes[CameraComponent.NearFrustrum] = new Plane(-zAxis, nearCenter);
            cameraData.FrustrumPlanes[CameraComponent.FarFrustrum] = new Plane(zAxis, farCenter);

            Vector3f normal = (nearCenter + yAxis * nearHeight) - cameraData.Location;
            normal.Normalize();
            Vector3f temp;
            Vector3f.Cross(ref normal, ref xAxis, out temp);
            cameraData.FrustrumPlanes[CameraComponent.TopFrustrum] = new Plane(temp, nearCenter + yAxis * nearHeight);

            normal = (nearCenter - yAxis * nearHeight) - cameraData.Location;
            normal.Normalize();
            Vector3f.Cross(ref xAxis, ref normal, out temp);
            cameraData.FrustrumPlanes[CameraComponent.BottomFrustrum] = new Plane(temp, nearCenter - yAxis * nearHeight);

            normal = (nearCenter - xAxis * nearWidth) - cameraData.Location;
            normal.Normalize();
            Vector3f.Cross(ref normal, ref yAxis, out temp);
            cameraData.FrustrumPlanes[CameraComponent.LeftFrustrum] = new Plane(temp, nearCenter - xAxis * nearWidth);

            normal = (nearCenter + xAxis * nearWidth) - cameraData.Location;
            normal.Normalize();
            Vector3f.Cross(ref yAxis, ref normal, out temp);
            cameraData.FrustrumPlanes[CameraComponent.RightFrustrum] = new Plane(temp, nearCenter + xAxis * nearWidth);
        }

        /// <summary>
        /// Fills the specified HashSet with entities that visible based on the current location and orientation of the camera
        /// </summary>
        private static void FindVisibleEntities(HashSet<IEntity> visibleEntities, CameraComponent cameraData, RenderNodeBase node)
        {
            LeafRenderNode leafNode = node as LeafRenderNode;
            if (leafNode != null)
            {
                for (int i = 0; i < leafNode.Entities.Count; i++)
                    visibleEntities.Add(leafNode.Entities[i]);
            }
            else
            {
                RenderNodeBase[] childrenLocal = ((RenderNode)node).ChildNodes;
                for (int i = 0; i < childrenLocal.Length; i++)
                {
                    RenderNodeBase childNode = childrenLocal[i];
                    if (childNode != null)
                    {
                        if (TiVEUtils.BoxInView(cameraData, childNode))
                            FindVisibleEntities(visibleEntities, cameraData, childNode);
                    }
                }
            }
        }
        #endregion
    }
}
