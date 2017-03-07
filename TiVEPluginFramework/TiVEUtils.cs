using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using ProdigalSoftware.TiVEPluginFramework.Components;

namespace ProdigalSoftware.TiVEPluginFramework
{
    internal static class TiVEUtils
    {
        /// <summary>
        /// Throws an ArgumentOutOfRangeException if the specified location is outside the bounds of the specified size.
        /// This method is not compiled into release builds.
        /// </summary>
        [Conditional("DEBUG")]
        [AssertionMethod]
        public static void DebugCheckConstraints(int x, int y, int z, Vector3i size)
        {
            if (x < 0 || x >= size.X)
                throw new ArgumentOutOfRangeException(nameof(x));
            if (y < 0 || y >= size.Y)
                throw new ArgumentOutOfRangeException(nameof(y));
            if (z < 0 || z >= size.Z)
                throw new ArgumentOutOfRangeException(nameof(z));
        }

        /// <summary>
        /// Throws an ArgumentOutOfRangeException if the specified location is outside the bounds of the specified size.
        /// This method is not compiled into release builds.
        /// </summary>
        [Conditional("DEBUG")]
        [AssertionMethod]
        public static void DebugCheckConstraints(int x, int y, int z, int size)
        {
            if (x < 0 || x >= size)
                throw new ArgumentOutOfRangeException(nameof(x));
            if (y < 0 || y >= size)
                throw new ArgumentOutOfRangeException(nameof(y));
            if (z < 0 || z >= size)
                throw new ArgumentOutOfRangeException(nameof(z));
        }

        /// <summary>
        /// Determines if the specified bounding box is visible from the current location and orientation of the camera
        /// </summary>
        public static bool BoxInView(CameraComponent cameraData, BoundingBox box)
        {
            for (int i = 0; i < cameraData.FrustrumPlanes.Length; i++)
            {
                Vector3f positivePoint;
                box.GetPositivePoint(ref cameraData.FrustrumPlanes[i].PlaneNormal, out positivePoint);
                if (cameraData.FrustrumPlanes[i].DistanceFromPoint(ref positivePoint) < 0)
                    return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FastAbs(int value)
        {
            return value >= 0 ? value : -value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FastSign(int value)
        {
            return value == 0 ? 0 : (value < 0 ? -1 : 1);
        }
    }
}
