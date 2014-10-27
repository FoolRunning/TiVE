using System;
using System.Collections.Generic;
using System.Linq;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    internal sealed class BlockList : IBlockList
    {
        private readonly Dictionary<string, BlockInformation> blockToIndexMap = new Dictionary<string, BlockInformation>();
        private readonly List<AnimationInfo> animationsList = new List<AnimationInfo>();
        private readonly HashSet<BlockInformation> blocksBelongingToAnimations = new HashSet<BlockInformation>();

        public int BlockCount
        {
            get { return blockToIndexMap.Count; }
        }

        public IEnumerable<AnimationInfo> Animations
        {
            get { return animationsList; }
        }

        public void AddBlocks(IEnumerable<BlockInformation> blocks)
        {
            foreach (BlockInformation block in blocks)
                blockToIndexMap.Add(block.BlockName, block);
        }

        public void AddAnimations(IEnumerable<BlockAnimationDefinition> animations)
        {
            if (animations == null)
                return;

            foreach (BlockAnimationDefinition animation in animations)
            {
                AnimationInfo animationInfo = new AnimationInfo(animation.AnimationFrameTime,
                    animation.AnimationSequence.Select(blockName => !string.IsNullOrEmpty(blockName) ? blockToIndexMap[blockName] : BlockInformation.Empty));

                if (animationsList.Exists(ani => animationInfo.AnimationSequence.Any(bl => ani.AnimationSequence.Contains(bl))))
                    throw new InvalidOperationException("Block is already used as an animation and can not belong to two animations");

                blocksBelongingToAnimations.UnionWith(animationInfo.AnimationSequence);
                animationsList.Add(animationInfo);
            }
        }

        public bool BelongsToAnimation(BlockInformation block)
        {
            return blocksBelongingToAnimations.Contains(block);
        }

        public BlockInformation this[string blockName]
        {
            get { return blockToIndexMap[blockName]; }
        }
    }
}
