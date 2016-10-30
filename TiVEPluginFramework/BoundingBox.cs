using System;
using System.IO;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public class BoundingBox : ITiVESerializable
    {
        public static readonly Guid ID = new Guid("4AC88D87-3EE1-44DA-898F-4D06FF5B01FB");

        private Vector3f minPoint;
        private Vector3f maxPoint;

        public BoundingBox(BinaryReader reader)
        {
            minPoint = new Vector3f(reader);
            maxPoint = new Vector3f(reader);
        }

        public BoundingBox(Vector3f minPoint, Vector3f maxPoint)
        {
            this.minPoint = minPoint;
            this.maxPoint = maxPoint;
        }

        public Vector3f MinPoint => minPoint;
        public Vector3f MaxPoint => maxPoint;

        #region Implementation of ITiVESerializable
        public void SaveTo(BinaryWriter writer)
        {
            minPoint.SaveTo(writer);
            maxPoint.SaveTo(writer);
        }
        #endregion

        public bool IntersectsWith(BoundingBox other)
        {
            return minPoint.X <= other.maxPoint.X && maxPoint.X >= other.minPoint.X &&
                minPoint.Y <= other.maxPoint.Y && maxPoint.Y >= other.minPoint.Y &&
                minPoint.Z <= other.maxPoint.Z && maxPoint.Z >= other.minPoint.Z;
        }

        internal void GetPositivePoint(ref Vector3f planeNormal, out Vector3f result)
        {
            result = minPoint; // Make copy
            if (planeNormal.X >= 0)
                result.X = maxPoint.X;
            if (planeNormal.Y >= 0)
                result.Y = maxPoint.Y;
            if (planeNormal.Z >= 0)
                result.Z = maxPoint.Z;
        }

        internal void GetNegativePoint(Vector3f planeNormal, out Vector3f result)
        {
            result = maxPoint; // Make copy
            if (planeNormal.X >= 0)
                result.X = minPoint.X;
            if (planeNormal.Y >= 0)
                result.Y = minPoint.Y;
            if (planeNormal.Z >= 0)
                result.Z = minPoint.Z;
        }
    }
}
