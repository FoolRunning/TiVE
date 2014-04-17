using System;
using OpenTK;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal sealed class Camera : ICamera
    {
        private Vector3 location;
        private Vector3 lookAtLocation;
        private Matrix4 viewMatrix;
        private Matrix4 projectionMatrix;
        private bool needUpdate;
        private float fieldOfView;

        public Camera()
        {
            FoV = (float)Math.PI / 3; // 60 degrees
        }

        public Matrix4 ViewMatrix
        {
            get { return viewMatrix; }
        }

        public Matrix4 ProjectionMatrix
        {
            get { return projectionMatrix; }
        }

        public Vector3 Location
        {
            get { return location; }
        }

        public Vector3 LookAtLocation
        {
            get { return lookAtLocation; }
        }

        public float AspectRatio { get; private set; }

        public float FoV
        {
            get { return fieldOfView; }
            set
            {
                fieldOfView = value;
                needUpdate = true;
            }
        }

        public void SetViewport(int width, int height)
        {
            AspectRatio = width / (float)height;
            needUpdate = true;
        }

        public void SetLocation(float x, float y, float z)
        {
            if (location.X == x && location.Y == y && location.Z == z)
                return;

            location.X = x;
            location.Y = y;
            location.Z = z;
            needUpdate = true;
        }

        public void SetLookAtLocation(float x, float y, float z)
        {
            if (lookAtLocation.X == x && lookAtLocation.Y == y && lookAtLocation.Z == z)
                return;

            lookAtLocation.X = x;
            lookAtLocation.Y = y;
            lookAtLocation.Z = z;
            needUpdate = true;
        }

        public void Update()
        {
            if (!needUpdate) 
                return;

            needUpdate = false;
            viewMatrix = Matrix4.LookAt(location.X, location.Y, location.Z,
                lookAtLocation.X, lookAtLocation.Y, lookAtLocation.Z,
                0.0f, 1.0f, 0.0f);

            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(FoV, AspectRatio, 1.0f, 1064.0f);
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
    }
}
