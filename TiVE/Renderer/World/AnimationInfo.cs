using System.Collections.Generic;
using System.Linq;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    internal sealed class AnimationInfo
    {
        public readonly ushort[] AnimationSequence;
        public readonly float AnimationFrameTime;

        public float TimeSinceLastFrame;

        public AnimationInfo(float animationFrameTime, IEnumerable<ushort> animationSequence)
        {
            AnimationFrameTime = animationFrameTime;
            AnimationSequence = animationSequence.ToArray();
        }
    }

}
