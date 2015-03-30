﻿using System;
using System.Diagnostics;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVE.RenderSystem.Voxels;
using ProdigalSoftware.TiVE.Utils;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.RenderSystem
{
    internal sealed class RenderSystem : EngineSystem
    {
        #region Constants
        private const float NearDist = 0.1f;

        private const int Top = 0;
        private const int Bottom = 1;
        private const int Left = 2;
        private const int Right = 3;
        private const int Near = 4;
        private const int Far = 5;
        #endregion

        private readonly Plane[] frustrumPlanes = new Plane[6];
        private readonly ShaderManager shaderManager = new ShaderManager();

        public RenderSystem() : base("Renderer")
        {
        }

        #region Implementation of EngineSystem
        public override void Dispose()
        {
        }

        public override bool Initialize()
        {
            return shaderManager.Initialize();
        }

        protected override void UpdateInternal(int ticksSinceLastFrame, Scene currentScene)
        {
            if (currentScene == null)
                return;

            RenderStatistics stats = new RenderStatistics();

            CameraComponent cameraData = FindCamera(currentScene);
            if (cameraData == null)
                return; // No camera to render with

            UpdateFrustrum(cameraData);

            Matrix4f viewProjectionMatrix;
            CalculateViewProjectionMatrix(cameraData, out viewProjectionMatrix);

            foreach (IEntity renderEntity in currentScene.GetEntitiesWithComponent<RenderComponent>())
            {
                RenderComponent render = renderEntity.GetComponent<RenderComponent>();
                Debug.Assert(render != null);

                if (!BoxInView(render.BoundingBox, true))
                    continue; // Outside the view frustrum so don't bother rendering it

                IVertexDataCollection meshData;
                using (new PerformanceLock(render.SyncLock))
                    meshData = (IVertexDataCollection)render.MeshData;

                if (meshData == null)
                    continue;

                Debug.Assert(meshData.IsInitialized);

                IShaderProgram shader = shaderManager.GetShaderProgram(VoxelMeshHelper.Get(false).ShaderName);
                shader.Bind();

                Matrix4f translationMatrix = Matrix4f.CreateTranslation((int)render.Location.X, (int)render.Location.Y, (int)render.Location.Z);

                Matrix4f viewProjectionModelMatrix;
                Matrix4f.Mult(ref translationMatrix, ref viewProjectionMatrix, out viewProjectionModelMatrix);
                shader.SetUniform("matrix_ModelViewProjection", ref viewProjectionModelMatrix);

                TiVEController.Backend.Draw(PrimitiveType.Triangles, meshData);
                stats += new RenderStatistics(1, render.PolygonCount, render.VoxelCount, render.RenderedVoxelCount);
            }
        }
        #endregion

        #region Private helper methods
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

        private static void CalculateViewProjectionMatrix(CameraComponent cameraData, out Matrix4f viewProjectionMatrix)
        {
            Matrix4f projectionMatrix = Matrix4f.CreatePerspectiveFieldOfView(cameraData.FieldOfView, cameraData.AspectRatio, NearDist, cameraData.FarDistance);
            Matrix4f viewMatrix = Matrix4f.LookAt(cameraData.Location, cameraData.LookAtLocation, cameraData.UpVector);
            viewProjectionMatrix = Matrix4f.Mult(viewMatrix, projectionMatrix);
        }

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

        private bool BoxInView(BoundingBox box, bool allowIntersecting)
        {
            for (int i = 0; i < frustrumPlanes.Length; i++)
            {
                if (frustrumPlanes[i].DistanceFromPoint(box.GetPositivePoint(frustrumPlanes[i].PlaneNormal)) < 0)
                    return false;
                if (!allowIntersecting && frustrumPlanes[i].DistanceFromPoint(box.GetNegativePoint(frustrumPlanes[i].PlaneNormal)) < 0)
                    return false;
            }
            return true;
        }
        #endregion
    }
}