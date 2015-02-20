using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Lighting;

namespace ProdigalSoftware.TiVE.Renderer.Lighting
{
    internal sealed class LightInfo
    {
        public readonly ushort VoxelLocX;
        public readonly ushort VoxelLocY;
        public readonly ushort VoxelLocZ;
        public readonly float CachedLightCalc;
        public readonly ILight Light;

        public LightInfo(int blockX, int blockY, int blockZ, ILight light, float cachedLightCalc)
        {
            VoxelLocX = (ushort)(light.Location.X + blockX * BlockInformation.VoxelSize);
            VoxelLocY = (ushort)(light.Location.Y + blockY * BlockInformation.VoxelSize);
            VoxelLocZ = (ushort)(light.Location.Z + blockZ * BlockInformation.VoxelSize);
            Light = light;
            CachedLightCalc = cachedLightCalc;
        }
    }
}
