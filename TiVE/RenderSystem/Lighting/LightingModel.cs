using System;
using System.Runtime.CompilerServices;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.RenderSystem.Lighting
{
    internal abstract class LightingModel
    {
        public const float MinRealisticLightPercent = 0.01f;
        private const float ShadowLightDistMinFactor = 2.0f;

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
        /// Gets the amount of light (0.0 - 1.0) that would the specified light would produce for the voxel at the specified location
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetLightPercentage(LightInfo lightInfo, int voxelX, int voxelY, int voxelZ)
        {
            int lightDistX = lightInfo.VoxelLocX - voxelX;
            int lightDistY = lightInfo.VoxelLocY - voxelY;
            int lightDistZ = lightInfo.VoxelLocZ - voxelZ;
            float distSquared = lightDistX * lightDistX + lightDistY * lightDistY + lightDistZ * lightDistZ;
            return GetLightPercentageInternal(distSquared, lightInfo.CachedLightCalc);
        }

        /// <summary>
        /// Gets the amount of light (0.0 - 1.0) that would the specified light would produce for the voxel at the specified location
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetLightPercentageShadow(LightInfo lightInfo, int voxelX, int voxelY, int voxelZ)
        {
            int lightDistX = lightInfo.VoxelLocX - voxelX;
            int lightDistY = lightInfo.VoxelLocY - voxelY;
            int lightDistZ = lightInfo.VoxelLocZ - voxelZ;
            float distSquared = lightDistX * lightDistX + lightDistY * lightDistY + lightDistZ * lightDistZ;
            return GetLightPercentageInternal(distSquared, lightInfo.CachedLightCalcShadow);
        }

        /// <summary>
        /// Gets a value to cache for the specified light that will make the lighting calculations faster
        /// </summary>
        public abstract float GetCacheLightCalculation(LightComponent light);

        /// <summary>
        /// Gets a value to cache for the specified light that will make the lighting calculations in shadow faster
        /// </summary>
        public abstract float GetCacheLightCalculationForShadow(LightComponent light);

        protected abstract float GetLightPercentageInternal(float distSquared, float cachedLightCalc);

        #region RealisticLightingModel class
        private sealed class RealisticLightingModel : LightingModel
        {
            public override float GetCacheLightCalculation(LightComponent light)
            {
                float dist = light.LightBlockDist * Block.VoxelSize;
                return 1.0f / (dist * dist * MinRealisticLightPercent); // Light attentuation
            }

            public override float GetCacheLightCalculationForShadow(LightComponent light)
            {
                float dist = (light.LightBlockDist * Block.VoxelSize) / ShadowLightDistMinFactor;
                return 1.0f / (dist * dist * MinRealisticLightPercent); // Light attentuation
            }

            protected override float GetLightPercentageInternal(float distSquared, float cachedLightCalc)
            {
                return 1.0f / (1.0f + distSquared * cachedLightCalc);
            }
        }
        #endregion

        #region BrightRealisticLightingModel class
        private sealed class BrightRealisticLightingModel : LightingModel
        {
            public override float GetCacheLightCalculation(LightComponent light)
            {
                float dist = light.LightBlockDist * Block.VoxelSize;
                return 1.0f / (dist * dist * dist * dist * MinRealisticLightPercent); // Light attentuation
            }

            public override float GetCacheLightCalculationForShadow(LightComponent light)
            {
                float dist = (light.LightBlockDist * Block.VoxelSize) / ShadowLightDistMinFactor;
                return 1.0f / (dist * dist * dist * dist * MinRealisticLightPercent); // Light attentuation
            }

            protected override float GetLightPercentageInternal(float distSquared, float cachedLightCalc)
            {
                return 1.0f / (1.0f + distSquared * distSquared * cachedLightCalc);
            }
        }
        #endregion

        #region Fantasy1LightingModel class
        private sealed class Fantasy1LightingModel : LightingModel
        {
            public override float GetCacheLightCalculation(LightComponent light)
            {
                float dist = light.LightBlockDist * Block.VoxelSize;
                return dist * dist; // Max light distance squared
            }

            public override float GetCacheLightCalculationForShadow(LightComponent light)
            {
                float dist = (light.LightBlockDist * Block.VoxelSize) / ShadowLightDistMinFactor;
                return dist * dist; // Max light distance squared
            }

            protected override float GetLightPercentageInternal(float distSquared, float cachedLightCalc)
            {
                float att = Math.Max(0.0f, 1.0f - distSquared / cachedLightCalc);
                return att * att;
            }
        }
        #endregion

        #region Fantasy2LightingModel class
        private sealed class Fantasy2LightingModel : LightingModel
        {
            public override float GetCacheLightCalculation(LightComponent light)
            {
                float dist = light.LightBlockDist * Block.VoxelSize;
                return dist * dist; // Max light distance squared
            }

            public override float GetCacheLightCalculationForShadow(LightComponent light)
            {
                float dist = (light.LightBlockDist * Block.VoxelSize) / ShadowLightDistMinFactor;
                return dist * dist; // Max light distance squared
            }

            protected override float GetLightPercentageInternal(float distSquared, float cachedLightCalc)
            {
                float att = Math.Max(0.0f, 1.0f - distSquared / cachedLightCalc);
                return att * att * att * att * att;
            }
        }
        #endregion
    }
}
