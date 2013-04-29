using System;
using OpenTK;

namespace ProdigalSoftware.TiVE.Renderer
{
    public sealed class Camera
    {
        private Vector3 location;
        private Vector3 lookAtLocation;
        private Matrix4 viewMatrix;
        private Matrix4 projectionMatrix;
        private bool needUpdate;

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

        public float FoV { get; set; }

        public void SetViewport(int width, int height)
        {
            AspectRatio = width / (float)height;
        }

        public void SetLocation(float x, float y, float z)
        {
            location.X = x;
            location.Y = y;
            location.Z = z;
            needUpdate = true;
        }

        public void SetLookAtLocation(float x, float y, float z)
        {
            lookAtLocation.X = x;
            lookAtLocation.Y = y;
            lookAtLocation.Z = z;
            needUpdate = true;
        }

        public void Update()
        {
            if (needUpdate)
            {
                viewMatrix = Matrix4.LookAt(location.X, location.Y, location.Z,
                    lookAtLocation.X, lookAtLocation.Y, lookAtLocation.Z,
                    0.0f, 1.0f, 0.0f);
                
                projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(FoV, AspectRatio, 1.0f, 1064.0f);

                needUpdate = false;
            }
        }
    }
}
