using System;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.RenderSystem.Lighting
{
    internal abstract class LightingModel
    {
        public const float MinRealisticLightPercent = 0.004f;
        private const float ShadowLightDistMinFactor = 1.9f;

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
        public abstract float GetCacheLightCalculation(LightComponent light);

        /// <summary>
        /// Gets a value to cache for the specified light that will make the lighting calculations in shadow faster
        /// </summary>
        public abstract float GetCacheLightCalculationForShadow(LightComponent light);

        public abstract float GetLightPercentage(float distSquared, float cachedLightCalc);

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

            public override float GetLightPercentage(float distSquared, float cachedLightCalc)
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

            public override float GetLightPercentage(float distSquared, float cachedLightCalc)
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
                return 1.0f / (dist * dist); // One over max light distance squared
            }

            public override float GetCacheLightCalculationForShadow(LightComponent light)
            {
                float dist = (light.LightBlockDist * Block.VoxelSize) / ShadowLightDistMinFactor;
                return 1.0f / (dist * dist); // One over max light distance squared
            }

            public override float GetLightPercentage(float distSquared, float cachedLightCalc)
            {
                float att = Math.Max(0.0f, 1.0f - distSquared * cachedLightCalc);
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
                return 1.0f / (dist * dist); // One over max light distance squared
            }

            public override float GetCacheLightCalculationForShadow(LightComponent light)
            {
                float dist = (light.LightBlockDist * Block.VoxelSize) / ShadowLightDistMinFactor;
                return 1.0f / (dist * dist); // One over max light distance squared
            }

            public override float GetLightPercentage(float distSquared, float cachedLightCalc)
            {
                float att = Math.Max(0.0f, 1.0f - distSquared * cachedLightCalc);
                return att * att * att * att * att;
            }
        }
        #endregion
    }
}
