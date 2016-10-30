using System.Runtime.CompilerServices;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.RenderSystem.Lighting
{
    /// <summary>
    /// Contains information about a light in the game world
    /// </summary>
    internal sealed class LightInfo
    {
        /// <summary>The voxel location of the light on the x-axis</summary>
        public readonly ushort VoxelLocX;       // 2 bytes
        /// <summary>The voxel location of the light on the y-axis</summary>
        public readonly ushort VoxelLocY;       // 2 bytes
        /// <summary>The voxel location of the light on the z-axis</summary>
        public readonly ushort VoxelLocZ;       // 2 bytes
        /// <summary>Light information</summary>
        public readonly Color3f LightColor;     // 12 bytes

        /// <summary>Cached lighting calculation needed by the current light model</summary>
        private readonly float cachedLightCalc; // 4 bytes
        /// <summary>Cached lighting calculation needed by the current light model for use in shadows</summary>
        private readonly float cachedLightCalcAmbient; // 4 bytes
        
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
            VoxelLocX = (ushort)(light.Location.X + (blockX * BlockLOD32.VoxelSize));
            VoxelLocY = (ushort)(light.Location.Y + (blockY * BlockLOD32.VoxelSize));
            VoxelLocZ = (ushort)(light.Location.Z + (blockZ * BlockLOD32.VoxelSize));
            LightColor = light.Color;

            this.cachedLightCalc = cachedLightCalc;
            this.cachedLightCalcAmbient = cachedLightCalcAmbient;
        }

        public int BlockX
        {
            get { return VoxelLocX / BlockLOD32.VoxelSize; }
        }

        public int BlockY
        {
            get { return VoxelLocY / BlockLOD32.VoxelSize; }
        }

        public int BlockZ
        {
            get { return VoxelLocZ / BlockLOD32.VoxelSize; }
        }

        public void VoxelLocation(LODLevel detailLevel, out int x, out int y, out int z)
        {
            x = VoxelLocX;
            y = VoxelLocY;
            z = VoxelLocZ;
            LODUtils.AdjustLocationForDetailLevelFrom32(ref x, ref y, ref z, detailLevel);
        }

        /// <summary>
        /// Gets the amount of light (0.0 - 1.0) that this light would produce for the voxel at the specified location
        /// </summary>
        public float GetLightPercentageDiffuseAndAmbient(int voxelX32, int voxelY32, int voxelZ32, LightingModel lightingModel)
        {
            float distSquared = DistanceSquared(voxelX32, voxelY32, voxelZ32);
            return lightingModel.GetLightPercentage(distSquared, cachedLightCalc) +
                lightingModel.GetLightPercentage(distSquared, cachedLightCalcAmbient);
        }

        /// <summary>
        /// Gets the amount of light (0.0 - 1.0) that this light would produce for the voxel at the specified location
        /// </summary>
        public float GetLightPercentageDiffuse(int voxelX32, int voxelY32, int voxelZ32, LightingModel lightingModel)
        {
            float distSquared = DistanceSquared(voxelX32, voxelY32, voxelZ32);
            return lightingModel.GetLightPercentage(distSquared, cachedLightCalc);
        }

        /// <summary>
        /// Gets the amount of light (0.0 - 1.0) that this light would produce for the voxel at the specified location when in a shadow
        /// </summary>
        public float GetLightPercentageAmbient(int voxelX32, int voxelY32, int voxelZ32, LightingModel lightingModel)
        {
            float distSquared = DistanceSquared(voxelX32, voxelY32, voxelZ32);
            return lightingModel.GetLightPercentage(distSquared, cachedLightCalcAmbient);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float DistanceSquared(int voxelX32, int voxelY32, int voxelZ32)
        {
            int lightDistX = VoxelLocX - voxelX32;
            int lightDistY = VoxelLocY - voxelY32;
            int lightDistZ = VoxelLocZ - voxelZ32;
            return lightDistX * lightDistX + lightDistY * lightDistY + lightDistZ * lightDistZ;
        }
    }
}
