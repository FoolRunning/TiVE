using OpenTK;

namespace ProdigalSoftware.TiVE.Utils
{
    internal struct Plane
    {
        public readonly Vector3 PlaneNormal;
        public readonly float PlaneD;

        public Plane(Vector3 newPlaneNormal, Vector3 newPlanePoint)
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
