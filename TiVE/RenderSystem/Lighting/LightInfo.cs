using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.RenderSystem.Lighting
{
    /// <summary>
    /// Contains information about a light in the game world
    /// </summary>
    internal sealed class LightInfo
    {
        /// <summary>The voxel location of the light</summary>
        public readonly Vector3f Location;
        /// <summary>Light information</summary>
        public readonly Color3f LightColor;

        /// <summary>Cached lighting calculation needed by the current light model</summary>
        public readonly float CachedLightCalc;
        
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
            Location = new Vector3f(
                light.Location.X + (blockX * BlockLOD32.VoxelSize), 
                light.Location.Y + (blockY * BlockLOD32.VoxelSize), 
                light.Location.Z + (blockZ * BlockLOD32.VoxelSize));
            LightColor = light.Color;

            CachedLightCalc = cachedLightCalc;
        }
    }
}
