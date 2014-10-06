using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Lighting;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    internal sealed class LightInfo
    {
        private readonly int voxelLocX;
        private readonly int voxelLocY;
        private readonly int voxelLocZ;
        private readonly ILight light;

        public LightInfo(int blockX, int blockY, int blockZ, ILight light)
        {
            voxelLocX = light.Location.X + blockX * BlockInformation.BlockSize;
            voxelLocY = light.Location.Y + blockY * BlockInformation.BlockSize;
            voxelLocZ = light.Location.Z + blockZ * BlockInformation.BlockSize;
            this.light = light;
        }

        public Color4f GetLightAtVoxel(int voxelX, int voxelY, int voxelZ)
        {
            int lightLocX = voxelLocX - voxelX;
            int lightLocY = voxelLocY - voxelY;
            int lightLocZ = voxelLocZ - voxelZ;
            float distSquared = lightLocX * lightLocX + lightLocY * lightLocY + lightLocZ * lightLocZ;
            return light.GetColorAtDistSquared(distSquared);
        }
    }

}
