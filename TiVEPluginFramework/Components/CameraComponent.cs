using System;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework.Components
{
    [PublicAPI]
    [MoonSharpUserData]
    public sealed class CameraComponent : IComponent
    {
        public bool Enabled = true;
        public float FarDistance = 500.0f;
        public float AspectRatio = 16 / 9.0f;
        public float FieldOfView = (float)Math.PI / 3; // 60 degrees
        public Vector3f UpVector = Vector3f.UnitY;
        public Vector3f Location;
        public Vector3f LookAtLocation;
    }
}
