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
        public readonly ushort VoxelLocX;      // 2 bytes
        /// <summary>The voxel location of the light on the y-axis</summary>
        public readonly ushort VoxelLocY;      // 2 bytes
        /// <summary>The voxel location of the light on the z-axis</summary>
        public readonly ushort VoxelLocZ;      // 2 bytes
        /// <summary>Color of the light when at full brightness</summary>
        public Color3f LightColor;    // 12 bytes

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
            VoxelLocX = (ushort)(light.Location.X + (blockX * Block.VoxelSize));
            VoxelLocY = (ushort)(light.Location.Y + (blockY * Block.VoxelSize));
            VoxelLocZ = (ushort)(light.Location.Z + (blockZ * Block.VoxelSize));
            LightColor = light.Color;
            this.cachedLightCalc = cachedLightCalc;
            this.cachedLightCalcAmbient = cachedLightCalcAmbient;
        }

        /// <summary>
        /// Gets the amount of light (0.0 - 1.0) that this light would produce for the voxel at the specified location
        /// </summary>
        public float GetLightPercentageDiffuseAndAmbient(int voxelX, int voxelY, int voxelZ, LightingModel lightingModel)
        {
            float distSquared = DistanceSquared(voxelX, voxelY, voxelZ);
            return lightingModel.GetLightPercentage(distSquared, cachedLightCalc) +
                lightingModel.GetLightPercentage(distSquared, cachedLightCalcAmbient);
        }

        /// <summary>
        /// Gets the amount of light (0.0 - 1.0) that this light would produce for the voxel at the specified location
        /// </summary>
        public float GetLightPercentage(int voxelX, int voxelY, int voxelZ, LightingModel lightingModel)
        {
            float distSquared = DistanceSquared(voxelX, voxelY, voxelZ);
            return lightingModel.GetLightPercentage(distSquared, cachedLightCalc);
        }

        /// <summary>
        /// Gets the amount of light (0.0 - 1.0) that this light would produce for the voxel at the specified location when in a shadow
        /// </summary>
        public float GetLightPercentageAmbient(int voxelX, int voxelY, int voxelZ, LightingModel lightingModel)
        {
            float distSquared = DistanceSquared(voxelX, voxelY, voxelZ);
            return lightingModel.GetLightPercentage(distSquared, cachedLightCalcAmbient);
        }

        public int BlockX
        {
            get { return VoxelLocX / Block.VoxelSize; }
        }

        public int BlockY
        {
            get { return VoxelLocY / Block.VoxelSize; }
        }

        public int BlockZ
        {
            get { return VoxelLocZ / Block.VoxelSize; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float DistanceSquared(int voxelX, int voxelY, int voxelZ)
        {
            int lightDistX = VoxelLocX - voxelX;
            int lightDistY = VoxelLocY - voxelY;
            int lightDistZ = VoxelLocZ - voxelZ;
            return lightDistX * lightDistX + lightDistY * lightDistY + lightDistZ * lightDistZ;
        }
    }
}
