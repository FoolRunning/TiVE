using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework
{
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
