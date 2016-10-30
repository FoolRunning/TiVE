using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace ProdigalSoftware.TiVEPluginFramework.Components
{
    /// <summary>
    /// Component for entities that represent the current camera view
    /// </summary>
    [PublicAPI]
    public sealed class CameraComponent : IComponent
    {
        public static readonly Guid ID = new Guid("794DD0D1-3DB6-4F6D-A6DA-B87689CF6E9A");
        public const float NearDist = 1.0f;
        private const byte SerializedFileVersion = 1;

        #region Internal data
        internal const int TopFrustrum = 0;
        internal const int BottomFrustrum = 1;
        internal const int LeftFrustrum = 2;
        internal const int RightFrustrum = 3;
        internal const int NearFrustrum = 4;
        internal const int FarFrustrum = 5;

        internal readonly Plane[] FrustrumPlanes = new Plane[6];
        internal readonly HashSet<IEntity> VisibleEntitites = new HashSet<IEntity>();
        internal readonly List<IEntity> NewlyVisibleEntitites = new List<IEntity>();
        internal readonly List<IEntity> NewlyHiddenEntitites = new List<IEntity>();
        internal Matrix4f ViewProjectionMatrix;
        #endregion

        [UsedImplicitly] public bool Enabled = true;
        [UsedImplicitly] public float FarDistance = 500.0f;
        [UsedImplicitly] public float AspectRatio; // 0.0f means use the aspect ratio of the client window
        [UsedImplicitly] public float FieldOfView = (float)Math.PI / 4; // 60 degrees
        [UsedImplicitly] public Vector3f UpVector = Vector3f.UnitZ;
        [UsedImplicitly] public Vector3f Location;
        [UsedImplicitly] public Vector3f LookAtLocation;

        public CameraComponent(BinaryReader reader)
        {
            byte fileVersion = reader.ReadByte();
            if (fileVersion > SerializedFileVersion)
                throw new FileTooNewException("CameraComponent");
            
            Enabled = reader.ReadBoolean();
            FarDistance = reader.ReadSingle();
            AspectRatio = reader.ReadSingle();
            FieldOfView = reader.ReadSingle();
            UpVector = new Vector3f(reader);
            Location = new Vector3f(reader);
            LookAtLocation = new Vector3f(reader);
        }

        public CameraComponent()
        {
        }

        public void SaveTo(BinaryWriter writer)
        {
            writer.Write(SerializedFileVersion);
            writer.Write(Enabled);
            writer.Write(FarDistance);
            writer.Write(AspectRatio);
            writer.Write(FieldOfView);
            UpVector.SaveTo(writer);
            Location.SaveTo(writer);
            LookAtLocation.SaveTo(writer);
        }
    }
}
