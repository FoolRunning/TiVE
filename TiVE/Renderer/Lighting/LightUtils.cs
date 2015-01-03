using System;
using System.Runtime.CompilerServices;

namespace ProdigalSoftware.TiVE.Renderer.Lighting
{
    internal static class LightUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetLightPercentage(LightInfo lightInfo, int voxelX, int voxelY, int voxelZ)
        {
            int lightLocX = lightInfo.VoxelLocX - voxelX;
            int lightLocY = lightInfo.VoxelLocY - voxelY;
            int lightLocZ = lightInfo.VoxelLocZ - voxelZ;
            float distSquared = lightLocX * lightLocX + lightLocY * lightLocY + lightLocZ * lightLocZ;
            float att = Math.Max(0.0f, 1.0f - distSquared / (lightInfo.VoxelLightDist * lightInfo.VoxelLightDist));
            //float att = Math.Max(0.0f, 1.0f - (float)Math.Sqrt(distSquared) / lightInfo.VoxelLightDist);
            return att * att * att * att * att;
        }
    }
}
