using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public sealed class LightComponent : IBlockComponent
    {
        public readonly Vector3b Location;
        public readonly int LightBlockDist;
        public readonly Color3f Color;
        public readonly bool ReflectiveAmbientLighting;

        public LightComponent(Vector3b location, Color3f color, int lightBlockDist, bool reflectiveAmbientLighting = false)
        {
            Location = location;
            Color = color;
            LightBlockDist = lightBlockDist;
            ReflectiveAmbientLighting = reflectiveAmbientLighting;
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

    public sealed class ReflectiveLightComponent : IBlockComponent
    {
        public static readonly IBlockComponent Instance = new ReflectiveLightComponent();

        private ReflectiveLightComponent()
        {
        }
    }
}
