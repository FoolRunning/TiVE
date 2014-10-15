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
        private readonly Dictionary<BlockInformation, BlockInformation> blockAnimationMap = new Dictionary<BlockInformation, BlockInformation>();
        private readonly HashSet<BlockInformation> blocksBelongingToAnimations = new HashSet<BlockInformation>();

        public int BlockCount
        {
            get { return blockToIndexMap.Count; }
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

        public void UpdateAnimationMap(float timeSinceLastUpdate)
        {
            blockAnimationMap.Clear();

            for (int i = 0; i < animationsList.Count; i++)
            {
                AnimationInfo animationInfo = animationsList[i];
                animationInfo.TimeSinceLastFrame += timeSinceLastUpdate;
                if (animationInfo.TimeSinceLastFrame >= animationInfo.AnimationFrameTime)
                {
                    animationInfo.TimeSinceLastFrame -= animationInfo.AnimationFrameTime;
                    for (int blockIndex = 0; blockIndex < animationInfo.AnimationSequence.Length - 1; blockIndex++)
                        blockAnimationMap.Add(animationInfo.AnimationSequence[blockIndex], animationInfo.AnimationSequence[blockIndex + 1]);
                }
            }
        }

        public BlockInformation NextFrameFor(BlockInformation block)
        {
            BlockInformation nextBlock;
            blockAnimationMap.TryGetValue(block, out nextBlock);
            return nextBlock;
        }

        public BlockInformation this[string blockName]
        {
            get { return blockToIndexMap[blockName]; }
        }

        private sealed class AnimationInfo
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
}
