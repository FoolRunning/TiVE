using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.RenderSystem.Lighting
{
    /// <summary>
    /// Contains information about a light in the game world
    /// </summary>
    internal struct LightInfo
    {
        /// <summary>The voxel location of the light on the x-axis</summary>
        public readonly ushort VoxelLocX;      // 2 bytes
        /// <summary>The voxel location of the light on the y-axis</summary>
        public readonly ushort VoxelLocY;      // 2 bytes
        /// <summary>The voxel location of the light on the z-axis</summary>
        public readonly ushort VoxelLocZ;      // 2 bytes
        /// <summary>Cached lighting calculation needed by the current light model</summary>
        public readonly float CachedLightCalc; // 4 bytes
        /// <summary>Color of the light when at full brightness</summary>
        public readonly Color3f LightColor;    // 12 bytes

        /// <summary>
        /// Creates a new LightInfo from the specified information
        /// </summary>
        /// <param name="blockX">The block location of the light on the x-axis</param>
        /// <param name="blockY">The block location of the light on the y-axis</param>
        /// <param name="blockZ">The block location of the light on the z-axis</param>
        /// <param name="light">The light</param>
        /// <param name="cachedLightCalc">Cached lighting calculation needed by the current light model</param>
        public LightInfo(int blockX, int blockY, int blockZ, LightComponent light, float cachedLightCalc)
        {
            VoxelLocX = (ushort)(light.Location.X + blockX * Block.VoxelSize);
            VoxelLocY = (ushort)(light.Location.Y + blockY * Block.VoxelSize);
            VoxelLocZ = (ushort)(light.Location.Z + blockZ * Block.VoxelSize);
            LightColor = light.Color;
            CachedLightCalc = cachedLightCalc;
        }
    }
}
