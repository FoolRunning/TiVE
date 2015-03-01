using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Lighting;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.Lighting
{
    internal struct LightInfo
    {
        public readonly ushort VoxelLocX;      // 2 bytes
        public readonly ushort VoxelLocY;      // 2 bytes
        public readonly ushort VoxelLocZ;      // 2 bytes
        public readonly float CachedLightCalc; // 4 bytes
        public readonly Color3f LightColor;    // 12 bytes

        public LightInfo(int blockX, int blockY, int blockZ, ILight light, float cachedLightCalc)
        {
            VoxelLocX = (ushort)(light.Location.X + blockX * BlockInformation.VoxelSize);
            VoxelLocY = (ushort)(light.Location.Y + blockY * BlockInformation.VoxelSize);
            VoxelLocZ = (ushort)(light.Location.Z + blockZ * BlockInformation.VoxelSize);
            LightColor = light.Color;
            CachedLightCalc = cachedLightCalc;
        }
    }
}
