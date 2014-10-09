using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Lighting;

namespace ProdigalSoftware.TiVE.Renderer.Lighting
{
    internal sealed class LightInfo
    {
        public readonly int VoxelLocX;
        public readonly int VoxelLocY;
        public readonly int VoxelLocZ;
        public readonly ILight Light;

        public LightInfo(int blockX, int blockY, int blockZ, ILight light)
        {
            VoxelLocX = light.Location.X + blockX * BlockInformation.BlockSize;
            VoxelLocY = light.Location.Y + blockY * BlockInformation.BlockSize;
            VoxelLocZ = light.Location.Z + blockZ * BlockInformation.BlockSize;
            Light = light;
        }
    }
}
