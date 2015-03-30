using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Utils
{
    internal struct Plane
    {
        public readonly Vector3f PlaneNormal;
        public readonly float PlaneD;

        public Plane(Vector3f newPlaneNormal, Vector3f newPlanePoint)
        {
            PlaneNormal = newPlaneNormal;
            PlaneD = -Vector3f.Dot(newPlaneNormal, newPlanePoint);
        }

        public float DistanceFromPoint(Vector3f point)
        {
            return Vector3f.Dot(PlaneNormal, point) + PlaneD;
        }
    }
}
