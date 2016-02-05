using System;
using System.IO;
using JetBrains.Annotations;

namespace ProdigalSoftware.TiVEPluginFramework
{
    [PublicAPI]
    public struct Plane : ITiVESerializable
    {
        public static readonly Guid ID = new Guid("A84E61F9-0717-4F9F-9A53-CBF2D6EC1965");

        public Vector3f PlaneNormal;
        public float PlaneD;

        public Plane(BinaryReader reader)
        {
            PlaneD = reader.ReadSingle();
            PlaneNormal = new Vector3f(reader);
        }

        public Plane(Vector3f newPlaneNormal, Vector3f newPlanePoint)
        {
            PlaneNormal = newPlaneNormal;
            PlaneD = -Vector3f.Dot(newPlaneNormal, newPlanePoint);
        }

        public float DistanceFromPoint(Vector3f point)
        {
            return Vector3f.Dot(PlaneNormal, point) + PlaneD;
        }

        #region Implementation of ITiVESerializable
        public void SaveTo(BinaryWriter writer)
        {
            writer.Write(PlaneD);
            PlaneNormal.SaveTo(writer);
        }
        #endregion
    }
}
