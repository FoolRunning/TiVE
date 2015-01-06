using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Lighting;

namespace ProdigalSoftware.TiVE.Renderer.Lighting
{
    internal sealed class LightInfo
    {
        public readonly short VoxelLocX;
        public readonly short VoxelLocY;
        public readonly short VoxelLocZ;
        public readonly float CachedLightCalc;
        public readonly ILight Light;

        public LightInfo(int blockX, int blockY, int blockZ, ILight light, float cachedLightCalc)
        {
            VoxelLocX = (short)(light.Location.X + blockX * BlockInformation.VoxelSize);
            VoxelLocY = (short)(light.Location.Y + blockY * BlockInformation.VoxelSize);
            VoxelLocZ = (short)(light.Location.Z + blockZ * BlockInformation.VoxelSize);
            Light = light;
            CachedLightCalc = cachedLightCalc;
        }
    }
}
