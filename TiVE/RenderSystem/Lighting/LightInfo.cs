using System.Runtime.CompilerServices;
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
        /// <summary>Cached lighting calculation needed by the current light model for use in shadows</summary>
        public readonly float CachedLightCalcShadow; // 4 bytes
        /// <summary>Color of the light when at full brightness</summary>
        public readonly Color3f LightColor;    // 12 bytes
        /// <summary>True to simulate reflective ambient lighting with this light</summary>
        public readonly bool ReflectiveAmbientLighting; // 1 byte

        /// <summary>
        /// Creates a new LightInfo from the specified information
        /// </summary>
        /// <param name="blockX">The block location of the light on the x-axis</param>
        /// <param name="blockY">The block location of the light on the y-axis</param>
        /// <param name="blockZ">The block location of the light on the z-axis</param>
        /// <param name="light">The light</param>
        /// <param name="cachedLightCalc">Cached lighting calculation needed by the current light model</param>
        /// <param name="cachedLightCalcForShadow">Cached lighting calculation needed by the current light model</param>
        public LightInfo(int blockX, int blockY, int blockZ, LightComponent light, float cachedLightCalc, float cachedLightCalcForShadow)
        {
            VoxelLocX = (ushort)(light.Location.X + blockX * Block.VoxelSize);
            VoxelLocY = (ushort)(light.Location.Y + blockY * Block.VoxelSize);
            VoxelLocZ = (ushort)(light.Location.Z + blockZ * Block.VoxelSize);
            LightColor = light.Color;
            CachedLightCalc = cachedLightCalc;
            CachedLightCalcShadow = cachedLightCalcForShadow;
            ReflectiveAmbientLighting = light.ReflectiveAmbientLighting;
        }

        /// <summary>
        /// Gets the amount of light (0.0 - 1.0) that this light would produce for the voxel at the specified location
        /// </summary>
        public float GetLightPercentage(int voxelX, int voxelY, int voxelZ, LightingModel lightingModel)
        {
            float distSquared = DistanceSquared(voxelX, voxelY, voxelZ);
            return lightingModel.GetLightPercentage(distSquared, CachedLightCalc);
        }

        /// <summary>
        /// Gets the amount of light (0.0 - 1.0) that this light would produce for the voxel at the specified location when in a shadow
        /// </summary>
        public float GetLightPercentageShadow(int voxelX, int voxelY, int voxelZ, LightingModel lightingModel)
        {
            float distSquared = DistanceSquared(voxelX, voxelY, voxelZ);
            return lightingModel.GetLightPercentage(distSquared, CachedLightCalcShadow);
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
