using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public sealed class BlockAnimationDefinition
    {
        private readonly string[] animationSequence;
        private readonly float animationFrameTime;

        public BlockAnimationDefinition(int animationFrameTime, params string[] sequenceBlockNames)
        {
            this.animationFrameTime = animationFrameTime / 1000.0f;

            animationSequence = sequenceBlockNames;
        }

        public IEnumerable<string> AnimationSequence
        {
            get { return animationSequence; }
        }

        public float AnimationFrameTime
        {
            get { return animationFrameTime; }
        }
    }
}
