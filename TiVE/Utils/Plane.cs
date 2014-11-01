using OpenTK;

namespace ProdigalSoftware.TiVE.Utils
{
    internal struct Plane
    {
        public Vector3 PlaneNormal;
        public float PlaneD;

        public void UpdatePlane(Vector3 newPlaneNormal, Vector3 newPlanePoint)
        {
            PlaneNormal = newPlaneNormal;
            PlaneD = -Vector3.Dot(newPlaneNormal, newPlanePoint);
        }

        public float DistanceFromPoint(Vector3 point)
        {
            return Vector3.Dot(PlaneNormal, point) + PlaneD;
        }
    }
}
