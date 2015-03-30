using System;
using JetBrains.Annotations;
using ProdigalSoftware.TiVE.Utils;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.RenderSystem
{
    [Obsolete("Replaced with CameraComponent")]
    internal sealed class Camera
    {
        #region Constants
        private const float NearDist = 0.5f;

        private const int Top = 0;
        private const int Bottom = 1;
        private const int Left = 2;
        private const int Right = 3;
        private const int Near = 4;
        private const int Far = 5;
        #endregion

        #region Member variables
        private readonly Plane[] frustrumPlanes = new Plane[6];

        private Vector3f location;
        private Vector3f lookAtLocation;
        private Vector3f upVector = Vector3f.UnitY;
        private bool needUpdateViewMatrix;
        private bool needUpdateProjectionMatrix;
        private float fieldOfView = (float)Math.PI / 3; // 60 degrees
        private float aspectRatio = 16 / 9.0f;
        private float nearWidth;
        private float nearHeight;
        private float farDist = 500.0f;
        #endregion

        #region Constructor
        public Camera()
        {
            needUpdateViewMatrix = true;
            needUpdateProjectionMatrix = true;
        }
        #endregion

        #region Properties
        public Matrix4f ViewMatrix { get; private set; }

        public Matrix4f ProjectionMatrix { get; private set; }

        [PublicAPI]
        public float FarDistance
        {
            get { return farDist; }
            set
            {
                if (!Equals(value, farDist))
                {
                    farDist = value;
                    needUpdateProjectionMatrix = true;
                }
            }
        }

        [PublicAPI]
        public Vector3f UpVector
        {
            get { return upVector; }
            set 
            {
                if (value != upVector)
                {
                    upVector = value;
                    needUpdateViewMatrix = true;
                }
            }
        }

        [PublicAPI]
        public Vector3f Location
        {
            get { return location; }
            set 
            {
                if (value != location)
                {
                    location = value;
                    needUpdateViewMatrix = true;
                }
            }
        }

        [PublicAPI]
        public Vector3f LookAtLocation
        {
            get { return lookAtLocation; }
            set 
            {
                if (value != lookAtLocation)
                {
                    lookAtLocation = value;
                    needUpdateViewMatrix = true;
                }
            }
        }

        [PublicAPI]
        public float AspectRatio
        {
            get { return aspectRatio; }
            set 
            {
                if (!Equals(value, aspectRatio))
                {
                    aspectRatio = value;
                    needUpdateProjectionMatrix = true;
                }
            }
        }

        [PublicAPI]
        public float FoV
        {
            get { return fieldOfView; }
            set
            {
                if (!Equals(value, fieldOfView))
                {
                    fieldOfView = value;
                    needUpdateProjectionMatrix = true;
                }
            }
        }
        #endregion

        public void Update()
        {
            if (needUpdateProjectionMatrix)
            {
                needUpdateProjectionMatrix = false;
                ProjectionMatrix = Matrix4f.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, NearDist, farDist);

                // Cache projection-related values used in the frustrum calculation
                nearHeight = NearDist * (float)Math.Tan(FoV * 0.5f);
                nearWidth = nearHeight * aspectRatio;
                needUpdateViewMatrix = true; // Need to update the view matrix with the new width/height values
            }

            if (needUpdateViewMatrix)
            {
                needUpdateViewMatrix = false;
                ViewMatrix = Matrix4f.LookAt(location, lookAtLocation, upVector);
            
                // Calculate frustrum planes
                Vector3f zAxis = location - lookAtLocation;
                zAxis.Normalize();
                Vector3f xAxis = Vector3f.Cross(upVector, zAxis);
                xAxis.Normalize();
                Vector3f yAxis = Vector3f.Cross(zAxis, xAxis);

                Vector3f nearCenter = location - zAxis * NearDist;
                Vector3f farCenter = location - zAxis * farDist;

                frustrumPlanes[Near] = new Plane(-zAxis, nearCenter);
                frustrumPlanes[Far] = new Plane(zAxis, farCenter);

                Vector3f normal = (nearCenter + yAxis * nearHeight) - location;
                normal.Normalize();
                frustrumPlanes[Top] = new Plane(Vector3f.Cross(normal, xAxis), nearCenter + yAxis * nearHeight);

                normal = (nearCenter - yAxis * nearHeight) - location;
                normal.Normalize();
                frustrumPlanes[Bottom] = new Plane(Vector3f.Cross(xAxis, normal), nearCenter - yAxis * nearHeight);

                normal = (nearCenter - xAxis * nearWidth) - location;
                normal.Normalize();
                frustrumPlanes[Left] = new Plane(Vector3f.Cross(normal, yAxis), nearCenter - xAxis * nearWidth);

                normal = (nearCenter + xAxis * nearWidth) - location;
                normal.Normalize();
                frustrumPlanes[Right] = new Plane(Vector3f.Cross(yAxis, normal), nearCenter + xAxis * nearWidth);
            }
        }

        public bool BoxInView(BoundingBox box, bool allowIntersecting)
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
    }
}
