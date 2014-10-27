using System.Collections.Generic;
using System.Linq;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    internal sealed class AnimationInfo
    {
        public readonly BlockInformation[] AnimationSequence;
        public readonly float AnimationFrameTime;

        public float TimeSinceLastFrame;

        public AnimationInfo(float animationFrameTime, IEnumerable<BlockInformation> animationSequence)
        {
            AnimationFrameTime = animationFrameTime;
            AnimationSequence = animationSequence.ToArray();
        }
    }

}
