using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public sealed class BoundingBox
    {
        private readonly Vector3f minPoint;
        private readonly Vector3f maxPoint;

        public BoundingBox(Vector3f minPoint, Vector3f maxPoint)
        {
            this.minPoint = minPoint;
            this.maxPoint = maxPoint;
        }

        public Vector3f MinPoint
        {
            get { return minPoint; }
        }

        public Vector3f MaxPoint
        {
            get { return maxPoint; }
        }

        public bool IntersectsWith(BoundingBox other)
        {
            return minPoint.X <= other.maxPoint.X && maxPoint.X >= other.minPoint.X &&
                minPoint.Y <= other.maxPoint.Y && maxPoint.Y >= other.minPoint.Y &&
                minPoint.Z <= other.maxPoint.Z && maxPoint.Z >= other.minPoint.Z;
        }

        internal Vector3f GetPositivePoint(Vector3f planeNormal)
        {
            Vector3f posPoint = minPoint; // Make copy
            if (planeNormal.X >= 0)
                posPoint.X = maxPoint.X;
            if (planeNormal.Y >= 0)
                posPoint.Y = maxPoint.Y;
            if (planeNormal.Z >= 0)
                posPoint.Z = maxPoint.Z;

            return posPoint;
        }

        internal Vector3f GetNegativePoint(Vector3f planeNormal)
        {
            Vector3f posPoint = maxPoint; // Make copy
            if (planeNormal.X >= 0)
                posPoint.X = minPoint.X;
            if (planeNormal.Y >= 0)
                posPoint.Y = minPoint.Y;
            if (planeNormal.Z >= 0)
                posPoint.Z = minPoint.Z;

            return posPoint;
        }
    }
}
