using System.Runtime.CompilerServices;
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
        /// <summary></summary>
        public readonly int BlockDist;
        /// <summary>Light information</summary>
        public readonly Color3f LightColor;

        /// <summary>Cached lighting calculation needed by the current light model</summary>
        public readonly float CachedLightCalc;
        /// <summary>Cached lighting calculation needed by the current light model for use in shadows</summary>
        public readonly float CachedLightCalcAmbient;
        
        /// <summary>
        /// Creates a new LightInfo from the specified information
        /// </summary>
        /// <param name="blockX">The block location of the light on the x-axis</param>
        /// <param name="blockY">The block location of the light on the y-axis</param>
        /// <param name="blockZ">The block location of the light on the z-axis</param>
        /// <param name="light">The light</param>
        /// <param name="cachedLightCalc">Cached lighting calculation needed by the current light model</param>
        /// <param name="cachedLightCalcAmbient">Cached lighting calculation needed by the current light model for ambient lighting</param>
        public LightInfo(int blockX, int blockY, int blockZ, LightComponent light, float cachedLightCalc, float cachedLightCalcAmbient)
        {
            Location = new Vector3f(light.Location.X + (blockX * BlockLOD32.VoxelSize), 
                light.Location.Y + (blockY * BlockLOD32.VoxelSize), 
                light.Location.Z + (blockZ * BlockLOD32.VoxelSize));
            BlockDist = light.LightBlockDist;
            LightColor = light.Color;

            CachedLightCalc = cachedLightCalc;
            CachedLightCalcAmbient = cachedLightCalcAmbient;
        }

        //public int BlockX => VoxelLocX / BlockLOD32.VoxelSize;

        //public int BlockY => VoxelLocY / BlockLOD32.VoxelSize;

        //public int BlockZ => VoxelLocZ / BlockLOD32.VoxelSize;

        public void VoxelLocation(LODLevel detailLevel, out int x, out int y, out int z)
        {
            x = (int)Location.X;
            y = (int)Location.Y;
            z = (int)Location.Z;
            LODUtils.AdjustLocationForDetailLevelFrom32(ref x, ref y, ref z, detailLevel);
        }

        /// <summary>
        /// Gets the amount of light (0.0 - 1.0) that this light would produce for the voxel at the specified location
        /// </summary>
        public float GetLightPercentageDiffuseAndAmbient(int voxelX32, int voxelY32, int voxelZ32, LightingModel lightingModel)
        {
            float distSquared = DistanceSquared(voxelX32, voxelY32, voxelZ32);
            return lightingModel.GetLightPercentage(distSquared, CachedLightCalc) + 
                lightingModel.GetLightPercentage(distSquared, CachedLightCalcAmbient);
        }

        /// <summary>
        /// Gets the amount of light (0.0 - 1.0) that this light would produce for the voxel at the specified location
        /// </summary>
        public float GetLightPercentageDiffuse(int voxelX32, int voxelY32, int voxelZ32, LightingModel lightingModel)
        {
            float distSquared = DistanceSquared(voxelX32, voxelY32, voxelZ32);
            return lightingModel.GetLightPercentage(distSquared, CachedLightCalc);
        }

        /// <summary>
        /// Gets the amount of light (0.0 - 1.0) that this light would produce for the voxel at the specified location when in a shadow
        /// </summary>
        public float GetLightPercentageAmbient(int voxelX32, int voxelY32, int voxelZ32, LightingModel lightingModel)
        {
            float distSquared = DistanceSquared(voxelX32, voxelY32, voxelZ32);
            return lightingModel.GetLightPercentage(distSquared, CachedLightCalcAmbient);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float DistanceSquared(int voxelX32, int voxelY32, int voxelZ32)
        {
            int lightDistX = (int)Location.X - voxelX32;
            int lightDistY = (int)Location.Y - voxelY32;
            int lightDistZ = (int)Location.Z - voxelZ32;
            return lightDistX * lightDistX + lightDistY * lightDistY + lightDistZ * lightDistZ;
        }
    }
}
