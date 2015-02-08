﻿using System;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Lighting;

namespace ProdigalSoftware.TiVE.Renderer.Lighting
{
    internal abstract class LightingModel
    {
        private const float MinRealisticLightPercent = 0.01f;

        private static readonly LightingModel realistic = new RealisticLightingModel();
        private static readonly LightingModel brightRealistic = new BrightRealisticLightingModel();
        private static readonly LightingModel fantasy1 = new Fantasy1LightingModel();
        private static readonly LightingModel fantasy2 = new Fantasy2LightingModel();

        public static LightingModel Get(LightingModelType lightingType)
        {
            switch (lightingType)
            {
                case LightingModelType.BrightRealistic: return brightRealistic;
                case LightingModelType.Fantasy1: return fantasy1;
                case LightingModelType.Fantasy2: return fantasy2;
                default: return realistic;
            }
        }

        /// <summary>
        /// Gets a value to cache for the specified light that will make the lighting calculations faster
        /// </summary>
        public abstract float GetCacheLightCalculation(ILight light);

        /// <summary>
        /// Gets the amount of light (0.0 - 1.0) that would the specified light would produce for the voxel at the specified location
        /// </summary>
        public abstract float GetLightPercentage(LightInfo lightInfo, int voxelX, int voxelY, int voxelZ);

        #region RealisticLightingModel class
        private sealed class RealisticLightingModel : LightingModel
        {
            public override float GetCacheLightCalculation(ILight light)
            {
                float dist = light.LightBlockDist * BlockInformation.VoxelSize;
                return 1.0f / (dist * dist * MinRealisticLightPercent); // Light attentuation
            }

            public override float GetLightPercentage(LightInfo lightInfo, int voxelX, int voxelY, int voxelZ)
            {
                int lightDistX = lightInfo.VoxelLocX - voxelX;
                int lightDistY = lightInfo.VoxelLocY - voxelY;
                int lightDistZ = lightInfo.VoxelLocZ - voxelZ;
                float distSquared = lightDistX * lightDistX + lightDistY * lightDistY + lightDistZ * lightDistZ;
                return 1.0f / (1.0f + distSquared * lightInfo.CachedLightCalc);
            }
        }
        #endregion

        #region BrightRealisticLightingModel class
        private sealed class BrightRealisticLightingModel : LightingModel
        {
            public override float GetCacheLightCalculation(ILight light)
            {
                float dist = light.LightBlockDist * BlockInformation.VoxelSize;
                return 1.0f / (dist * dist * dist * dist * MinRealisticLightPercent); // Light attentuation
            }

            public override float GetLightPercentage(LightInfo lightInfo, int voxelX, int voxelY, int voxelZ)
            {
                int lightDistX = lightInfo.VoxelLocX - voxelX;
                int lightDistY = lightInfo.VoxelLocY - voxelY;
                int lightDistZ = lightInfo.VoxelLocZ - voxelZ;
                float distSquared = lightDistX * lightDistX + lightDistY * lightDistY + lightDistZ * lightDistZ;
                return 1.0f / (1.0f + distSquared * distSquared * lightInfo.CachedLightCalc);
            }
        }
        #endregion

        #region Fantasy1LightingModel class
        private sealed class Fantasy1LightingModel : LightingModel
        {
            public override float GetCacheLightCalculation(ILight light)
            {
                float dist = light.LightBlockDist * BlockInformation.VoxelSize;
                return dist * dist; // Max light distance squared
            }

            public override float GetLightPercentage(LightInfo lightInfo, int voxelX, int voxelY, int voxelZ)
            {
                int lightDistX = lightInfo.VoxelLocX - voxelX;
                int lightDistY = lightInfo.VoxelLocY - voxelY;
                int lightDistZ = lightInfo.VoxelLocZ - voxelZ;
                float distSquared = lightDistX * lightDistX + lightDistY * lightDistY + lightDistZ * lightDistZ;
                float att = Math.Max(0.0f, 1.0f - distSquared / lightInfo.CachedLightCalc);
                return att * att;
            }
        }
        #endregion

        #region Fantasy2LightingModel class
        private sealed class Fantasy2LightingModel : LightingModel
        {
            public override float GetCacheLightCalculation(ILight light)
            {
                float dist = light.LightBlockDist * BlockInformation.VoxelSize;
                return dist * dist; // Max light distance squared
            }

            public override float GetLightPercentage(LightInfo lightInfo, int voxelX, int voxelY, int voxelZ)
            {
                int lightDistX = lightInfo.VoxelLocX - voxelX;
                int lightDistY = lightInfo.VoxelLocY - voxelY;
                int lightDistZ = lightInfo.VoxelLocZ - voxelZ;
                float distSquared = lightDistX * lightDistX + lightDistY * lightDistY + lightDistZ * lightDistZ;
                float att = Math.Max(0.0f, 1.0f - distSquared / lightInfo.CachedLightCalc);
                return att * att * att * att * att;
            }
        }
        #endregion
    }
}