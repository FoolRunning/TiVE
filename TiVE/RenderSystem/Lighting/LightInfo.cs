﻿using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.RenderSystem.Lighting
{
    /// <summary>
    /// Contains information about a light in the game world
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct LightInfo
    {
        /// <summary>The voxel location of the light on the x-axis</summary>
        public readonly ushort VoxelLocX;      // 2 bytes
        /// <summary>The voxel location of the light on the y-axis</summary>
        public readonly ushort VoxelLocY;      // 2 bytes
        /// <summary>The voxel location of the light on the z-axis</summary>
        public readonly ushort VoxelLocZ;      // 2 bytes
        /// <summary>Color of the light when at full brightness</summary>
        public readonly Color3f LightColor;    // 12 bytes
        /// <summary>Cached lighting calculation needed by the current light model</summary>
        private readonly float cachedLightCalc; // 4 bytes
        /// <summary>Cached lighting calculation needed by the current light model for use in shadows</summary>
        private readonly float cachedLightCalcShadow; // 4 bytes

        /// <summary>
        /// Creates a new LightInfo from the specified information
        /// </summary>
        /// <param name="blockX">The block location of the light on the x-axis</param>
        /// <param name="blockY">The block location of the light on the y-axis</param>
        /// <param name="blockZ">The block location of the light on the z-axis</param>
        /// <param name="light">The light</param>
        /// <param name="cachedLightCalc">Cached lighting calculation needed by the current light model</param>
        /// <param name="cachedLightCalcShadow">Cached lighting calculation needed by the current light model</param>
        public LightInfo(int blockX, int blockY, int blockZ, LightComponent light, float cachedLightCalc, float cachedLightCalcShadow)
        {
            VoxelLocX = (ushort)(light.Location.X + (blockX << Block.VoxelSizeBitShift));
            VoxelLocY = (ushort)(light.Location.Y + (blockY << Block.VoxelSizeBitShift));
            VoxelLocZ = (ushort)(light.Location.Z + (blockZ << Block.VoxelSizeBitShift));
            LightColor = light.Color;
            this.cachedLightCalc = cachedLightCalc;
            this.cachedLightCalcShadow = cachedLightCalcShadow;
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
        public float GetLightPercentageShadow(int voxelX, int voxelY, int voxelZ, LightingModel lightingModel)
        {
            float distSquared = DistanceSquared(voxelX, voxelY, voxelZ);
            return lightingModel.GetLightPercentage(distSquared, cachedLightCalcShadow);
        }

        public int BlockX
        {
            get { return VoxelLocX >> Block.VoxelSizeBitShift; }
        }

        public int BlockY
        {
            get { return VoxelLocY >> Block.VoxelSizeBitShift; }
        }

        public int BlockZ
        {
            get { return VoxelLocZ >> Block.VoxelSizeBitShift; }
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
