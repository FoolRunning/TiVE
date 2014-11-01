using System;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVE.Utils;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal sealed class Camera
    {
        private const float NearDist = 1.0f;
        private const float FarDist = 800.0f;

        private const int Top = 0;
        private const int Bottom = 1;
        private const int Left = 2;
        private const int Right = 3;
        private const int Near = 4;
        private const int Far = 5;

        private readonly Plane[] frustrumPlanes = new Plane[6];

        private Vector3 location;
        private Vector3 lookAtLocation;
        private Vector3 upVector;
        private Matrix4 viewMatrix;
        private Matrix4 projectionMatrix;
        private bool needUpdateViewMatrix;
        private bool needUpdateProjectionMatrix;
        private float fieldOfView;
        private float aspectRatio;
        private float nearWidth;
        private float nearHeight;

        public Camera()
        {
            aspectRatio = 16 / 9.0f;
            fieldOfView = (float)Math.PI / 3; // 60 degrees
            upVector = Vector3.UnitY;
            needUpdateViewMatrix = true;
            needUpdateProjectionMatrix = true;
        }

        public Matrix4 ViewMatrix
        {
            get { return viewMatrix; }
        }

        public Matrix4 ProjectionMatrix
        {
            get { return projectionMatrix; }
        }

        public Vector3 UpVector
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

        public Vector3 Location
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

        public Vector3 LookAtLocation
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

        public float AspectRatio
        {
            get { return aspectRatio; }
            set 
            {
                if (value != aspectRatio)
                {
                    aspectRatio = value;
                    needUpdateProjectionMatrix = true;
                }
            }
        }

        public float FoV
        {
            get { return fieldOfView; }
            set
            {
                if (value != fieldOfView)
                {
                    fieldOfView = value;
                    needUpdateProjectionMatrix = true;
                }
            }
        }

        public void Update()
        {
            if (needUpdateProjectionMatrix)
            {
                projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, NearDist, FarDist);

                // Cache projection-related values used in the frustrum calculation
                nearHeight = NearDist * (float)Math.Tan(FoV * 0.5f);
                nearWidth = nearHeight * aspectRatio;
            }

            if (needUpdateViewMatrix)
            {
                needUpdateViewMatrix = false;
                viewMatrix = Matrix4.LookAt(location, lookAtLocation, upVector);
            
                // Calculate frustrum planes
                Vector3 zAxis = location - lookAtLocation;
                zAxis.Normalize();
                Vector3 xAxis = Vector3.Cross(upVector, zAxis);
                xAxis.Normalize();
                Vector3 yAxis = Vector3.Cross(zAxis, xAxis);

                Vector3 nearCenter = location - zAxis * NearDist;
                Vector3 farCenter = location - zAxis * FarDist;

                frustrumPlanes[Near].UpdatePlane(-zAxis, nearCenter);
                frustrumPlanes[Far].UpdatePlane(zAxis, farCenter);

                Vector3 normal = (nearCenter + yAxis * nearHeight) - location;
                normal.Normalize();
                frustrumPlanes[Top].UpdatePlane(Vector3.Cross(normal, xAxis), nearCenter + yAxis * nearHeight);

                normal = (nearCenter - yAxis * nearHeight) - location;
                normal.Normalize();
                frustrumPlanes[Bottom].UpdatePlane(Vector3.Cross(xAxis, normal), nearCenter - yAxis * nearHeight);

                normal = (nearCenter - xAxis * nearWidth) - location;
                normal.Normalize();
                frustrumPlanes[Left].UpdatePlane(Vector3.Cross(normal, yAxis), nearCenter - xAxis * nearWidth);

                normal = (nearCenter + xAxis * nearWidth) - location;
                normal.Normalize();
                frustrumPlanes[Right].UpdatePlane(Vector3.Cross(yAxis, normal), nearCenter + xAxis * nearWidth);
            }
        }
        
        public void GetViewPlane(float distance, out Vector3 topLeft, out Vector3 bottomRight)
        {
            Vector3 zAxis = (Location - LookAtLocation);
            zAxis.Normalize();

            Vector3 xAxis = Vector3.Cross(Vector3.UnitY, zAxis);
            xAxis.Normalize();

            Vector3 yAxis = Vector3.Cross(zAxis, xAxis);

            Vector3 farPoint = Location - zAxis * distance;

            float height = (float)Math.Tan(FoV * 0.5f) * distance;
            float width = height * AspectRatio;

            topLeft = farPoint + yAxis * height - xAxis * width;
            bottomRight = farPoint - yAxis * height + xAxis * width;
        }

        public bool BoxInView(WorldBoundingBox box, bool allowIntersecting)
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
