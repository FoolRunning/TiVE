using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Lighting;

namespace ProdigalSoftware.TiVE.Renderer.Lighting
{
    internal sealed class LightInfo
    {
        public readonly int VoxelLocX;
        public readonly int VoxelLocY;
        public readonly int VoxelLocZ;
        public readonly float VoxelLightDist;
        public readonly ILight Light;

        public LightInfo(int blockX, int blockY, int blockZ, ILight light)
        {
            VoxelLocX = light.Location.X + blockX * BlockInformation.VoxelSize;
            VoxelLocY = light.Location.Y + blockY * BlockInformation.VoxelSize;
            VoxelLocZ = light.Location.Z + blockZ * BlockInformation.VoxelSize;
            VoxelLightDist = light.LightBlockDist * BlockInformation.VoxelSize;
            Light = light;
        }
    }
}
