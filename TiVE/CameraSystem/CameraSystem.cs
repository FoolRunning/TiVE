using System;
using System.Collections.Generic;
using System.Diagnostics;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVE.RenderSystem;
using ProdigalSoftware.TiVE.Utils;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.CameraSystem
{
    /// <summary>
    /// Engine system that handles entities that contain a camera component
    /// </summary>
    internal sealed class CameraSystem : EngineSystem
    {
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

        protected override bool UpdateInternal(int ticksSinceLastUpdate, Scene currentScene)
        {
            foreach (IEntity entity in currentScene.GetEntitiesWithComponent<CameraComponent>())
            {
                CameraComponent cameraData = entity.GetComponent<CameraComponent>();
                Debug.Assert(cameraData != null);

                if (!cameraData.Enabled || cameraData.Location == cameraData.LookAtLocation)
                    continue; // Camera not enabled or probably hasn't been initialized yet

                UpdateFrustrum(cameraData);

                // Calculate the projection matrix
                Matrix4f projectionMatrix;
                Matrix4f.CreatePerspectiveFieldOfView(cameraData.FieldOfView, cameraData.AspectRatio,
                    CameraComponent.NearDist, cameraData.FarDistance, out projectionMatrix);

                // Calculate the view matrix
                Matrix4f viewMatrix;
                Matrix4f.LookAt(cameraData.Location, cameraData.LookAtLocation, cameraData.UpVector, out viewMatrix);

                // Create and cache the view projection matrix
                Matrix4f.Mult(ref viewMatrix, ref projectionMatrix, out cameraData.ViewProjectionMatrix);

                // Determine what entities are visible from this camera
                cameraData.VisibleEntitites.Clear();
                FindVisibleEntities(cameraData.VisibleEntitites, cameraData, currentScene.RenderNode);
            }

            return true;
        }
        #endregion

        #region Private helper methods
        /// <summary>
        /// Updates the cached frustrum data with the data from the specified camera
        /// </summary>
        private static void UpdateFrustrum(CameraComponent cameraData)
        {
            float nearHeight = CameraComponent.NearDist * (float)Math.Tan(cameraData.FieldOfView * 0.5f);
            float nearWidth = nearHeight * cameraData.AspectRatio;

            Vector3f zAxis = cameraData.Location - cameraData.LookAtLocation;
            zAxis.Normalize();
            Vector3f xAxis = Vector3f.Cross(cameraData.UpVector, zAxis);
            xAxis.Normalize();
            Vector3f yAxis = Vector3f.Cross(zAxis, xAxis);

            Vector3f nearCenter = cameraData.Location - zAxis * CameraComponent.NearDist;
            Vector3f farCenter = cameraData.Location - zAxis * cameraData.FarDistance;

            // Calculate frustrum planes
            cameraData.FrustrumPlanes[CameraComponent.NearFrustrum] = new Plane(-zAxis, nearCenter);
            cameraData.FrustrumPlanes[CameraComponent.FarFrustrum] = new Plane(zAxis, farCenter);

            Vector3f normal = (nearCenter + yAxis * nearHeight) - cameraData.Location;
            normal.Normalize();
            cameraData.FrustrumPlanes[CameraComponent.TopFrustrum] = new Plane(Vector3f.Cross(normal, xAxis), nearCenter + yAxis * nearHeight);

            normal = (nearCenter - yAxis * nearHeight) - cameraData.Location;
            normal.Normalize();
            cameraData.FrustrumPlanes[CameraComponent.BottomFrustrum] = new Plane(Vector3f.Cross(xAxis, normal), nearCenter - yAxis * nearHeight);

            normal = (nearCenter - xAxis * nearWidth) - cameraData.Location;
            normal.Normalize();
            cameraData.FrustrumPlanes[CameraComponent.LeftFrustrum] = new Plane(Vector3f.Cross(normal, yAxis), nearCenter - xAxis * nearWidth);

            normal = (nearCenter + xAxis * nearWidth) - cameraData.Location;
            normal.Normalize();
            cameraData.FrustrumPlanes[CameraComponent.RightFrustrum] = new Plane(Vector3f.Cross(yAxis, normal), nearCenter + xAxis * nearWidth);
        }

        /// <summary>
        /// Fills the specified HashSet with entities that visible based on the current location and orientation of the camera
        /// </summary>
        private static void FindVisibleEntities(HashSet<IEntity> visibleEntities, CameraComponent cameraData, RenderNode node)
        {
            List<IEntity> entities = node.Entities;
            if (entities != null)
            {
                for (int i = 0; i < entities.Count; i++)
                    visibleEntities.Add(entities[i]);
                return;
            }

            RenderNode[] childrenLocal = node.ChildNodes;
            for (int i = 0; i < childrenLocal.Length; i++)
            {
                RenderNode childNode = childrenLocal[i];
                if (childNode != null)
                {
                    if (TiVEUtils.BoxInView(cameraData, childNode.BoundingBox))
                        FindVisibleEntities(visibleEntities, cameraData, childNode);
                }
            }
        }
        #endregion
    }
}
