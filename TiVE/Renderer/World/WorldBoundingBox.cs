using OpenTK;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    internal class WorldBoundingBox
    {
        protected readonly Vector3 minPoint;
        protected readonly Vector3 maxPoint;

        public WorldBoundingBox(Vector3 minPoint, Vector3 maxPoint)
        {
            this.minPoint = minPoint;
            this.maxPoint = maxPoint;
        }

        public Vector3 GetPositivePoint(Vector3 planeNormal)
        {
            Vector3 posPoint = minPoint;
            if (planeNormal.X >= 0)
                posPoint.X = maxPoint.X;
            if (planeNormal.Y >= 0)
                posPoint.Y = maxPoint.Y;
            if (planeNormal.Z >= 0)
                posPoint.Z = maxPoint.Z;

            return posPoint;
        }

        public Vector3 GetNegativePoint(Vector3 planeNormal)
        {
            Vector3 posPoint = maxPoint;
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
