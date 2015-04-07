using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework.Components
{
    /// <summary>
    /// Component for entities that represent the current camera view
    /// </summary>
    [PublicAPI]
    public sealed class CameraComponent : IComponent
    {
        public const float NearDist = 0.1f;

        #region Internal data
        internal const int TopFrustrum = 0;
        internal const int BottomFrustrum = 1;
        internal const int LeftFrustrum = 2;
        internal const int RightFrustrum = 3;
        internal const int NearFrustrum = 4;
        internal const int FarFrustrum = 5;

        internal readonly Plane[] FrustrumPlanes = new Plane[6];
        internal readonly HashSet<IEntity> VisibleEntitites = new HashSet<IEntity>();
        internal Matrix4f ViewProjectionMatrix;
        internal Vector3f PrevLocation;
        internal Vector3f PrevLookAtLocation;
        #endregion

        [UsedImplicitly] public bool Enabled = true;
        [UsedImplicitly] public float FarDistance = 500.0f;
        [UsedImplicitly] public float AspectRatio = 16 / 9.0f;
        [UsedImplicitly] public float FieldOfView = (float)Math.PI / 3; // 60 degrees
        [UsedImplicitly] public Vector3f UpVector = Vector3f.UnitY;
        [UsedImplicitly] public Vector3f Location;
        [UsedImplicitly] public Vector3f LookAtLocation;
    }
}
