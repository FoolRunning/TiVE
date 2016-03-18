using System;
using System.Diagnostics;
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
        [JetBrains.Annotations.AssertionMethod]
        public static void CheckConstraints(int x, int y, int z, Vector3i size)
        {
            if (x < 0 || x >= size.X)
                throw new ArgumentOutOfRangeException("x");
            if (y < 0 || y >= size.Y)
                throw new ArgumentOutOfRangeException("y");
            if (z < 0 || z >= size.Z)
                throw new ArgumentOutOfRangeException("z");
        }

        /// <summary>
        /// Determines if the specified bounding box is visible from the current location and orientation of the camera
        /// </summary>
        public static bool BoxInView(CameraComponent cameraData, BoundingBox box)
        {
            for (int i = 0; i < cameraData.FrustrumPlanes.Length; i++)
            {
                if (cameraData.FrustrumPlanes[i].DistanceFromPoint(box.GetPositivePoint(cameraData.FrustrumPlanes[i].PlaneNormal)) < 0)
                    return false;
            }
            return true;
        }
    }
}
