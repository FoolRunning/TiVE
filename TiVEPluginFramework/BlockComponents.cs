using System;
using JetBrains.Annotations;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public sealed class LightComponent : IBlockComponent
    {
        [PublicAPI] public readonly Vector3b Location;
        [PublicAPI] public readonly int LightBlockDist;
        [PublicAPI] public readonly Color3f Color;

        public LightComponent(Vector3b location, Color3f color, int lightBlockDist)
        {
            Location = location;
            Color = color;
            LightBlockDist = lightBlockDist;
        }
    }

    public sealed class AnimationComponent : IBlockComponent
    {
        public readonly float AnimationFrameTime;
        public readonly string NextBlockName;

        public AnimationComponent(int animationFrameTimeMs, string nextBlockName)
        {
            AnimationFrameTime = animationFrameTimeMs / 1000.0f;
            NextBlockName = nextBlockName;
        }
    }

    public sealed class VoxelAdjusterComponent : IBlockComponent
    {
        internal readonly Func<Voxel, Voxel> Adjuster;

        public VoxelAdjusterComponent(Func<Voxel, Voxel> adjuster)
        {
            Adjuster = adjuster;
        }

        public VoxelAdjusterComponent(float variationPercentage)
        {
            Random random = new Random();
            Adjuster = voxel => LightBrightnessAdjustment(voxel, variationPercentage, random);
        }

        public static Voxel LightBrightnessAdjustment(Voxel voxel, float variationPercentage, Random random)
        {
            float rnd;
            lock (random) // The Random class is not thread-safe, so lock it just in case
                rnd = (float)random.NextDouble();
            float scale = rnd * variationPercentage + (1.0f - variationPercentage / 2.0f);
            return new Voxel(Math.Min(voxel.R / 255f * scale, 1.0f), Math.Min(voxel.G / 255f * scale, 1.0f),
                Math.Min(voxel.B / 255f * scale, 1.0f), voxel.A / 255f);
        }
    }

    public sealed class UnlitComponent : IBlockComponent
    {
        public static readonly IBlockComponent Instance = new UnlitComponent();

        private UnlitComponent()
        {
        }
    }

    public sealed class TransparentComponent : IBlockComponent
    {
        public static readonly IBlockComponent Instance = new TransparentComponent();

        private TransparentComponent()
        {
        }
    }
}
