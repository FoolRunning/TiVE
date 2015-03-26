using ProdigalSoftware.TiVEPluginFramework.Particles;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public enum TransparencyType
    {
        None,
        Realistic,
        Additive
    }

    public sealed class ParticleSystemComponent : IBlockComponent
    {
        public readonly uint[, ,] ParticleVoxels;
        public readonly ParticleController Controller;
        public readonly int ParticlesPerSecond;
        public readonly int MaxParticles;
        public readonly Vector3b Location;
        public readonly TransparencyType TransparencyType;
        public readonly bool IsLit;

        public ParticleSystemComponent(uint[, ,] particleVoxels, ParticleController controller, Vector3b location,
            int particlesPerSecond, int maxParticles, TransparencyType transparencyType, bool isLit)
        {
            ParticleVoxels = particleVoxels;
            Controller = controller;
            Location = location;
            ParticlesPerSecond = particlesPerSecond;
            MaxParticles = maxParticles;
            TransparencyType = transparencyType;
            IsLit = isLit;
        }
    }

    public sealed class LightComponent : IBlockComponent
    {
        public readonly Vector3b Location;
        public readonly int LightBlockDist;
        public readonly Color3f Color;

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

    public sealed class UnlitComponent : IBlockComponent
    {
    }

    public sealed class TransparentComponent : IBlockComponent
    {
    }
}
