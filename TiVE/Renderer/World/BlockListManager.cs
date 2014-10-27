using System.Collections.Generic;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.Voxels;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    internal sealed class BlockListManager
    {
        private readonly Dictionary<BlockInformation, BlockInformation> blockAnimationMap = new Dictionary<BlockInformation, BlockInformation>();
        private readonly Dictionary<BlockInformation, VoxelGroup> blockMeshes = new Dictionary<BlockInformation, VoxelGroup>();

        public BlockList BlockList { get; private set; }

        public bool Initialize()
        {
            Messages.Print("Loading blocks...");

            BlockList blockList = new BlockList();

            foreach (IBlockGenerator generator in ResourceManager.PluginManager.GetPluginsOfType<IBlockGenerator>())
            {
                blockList.AddBlocks(generator.CreateBlocks());
                blockList.AddAnimations(generator.CreateAnimations());
            }

            if (blockList.BlockCount == 0)
            {
                Messages.AddFailText();
                Messages.AddWarning("Created empty block list");
                BlockList = null;
                return false;
            }

            Messages.AddDoneText();
            BlockList = blockList;
            return true;
        }

        public void Dispose()
        {
            BlockList = null;
            foreach (VoxelGroup voxelGroup in blockMeshes.Values)
                voxelGroup.Dispose();
        }

        public void UpdateAnimations(float timeSinceLastUpdate)
        {
            blockAnimationMap.Clear();

            foreach (AnimationInfo animationInfo in BlockList.Animations)
            {
                animationInfo.TimeSinceLastFrame += timeSinceLastUpdate;
                if (animationInfo.TimeSinceLastFrame >= animationInfo.AnimationFrameTime)
                {
                    animationInfo.TimeSinceLastFrame -= animationInfo.AnimationFrameTime;
                    for (int blockIndex = 0; blockIndex < animationInfo.AnimationSequence.Length - 1; blockIndex++)
                        blockAnimationMap.Add(animationInfo.AnimationSequence[blockIndex], animationInfo.AnimationSequence[blockIndex + 1]);
                }
            }
        }

        public RenderStatistics RenderAnimatedBlocks(ref Matrix4 matrixMVP, int camMinX, int camMaxX, int camMinY, int camMaxY)
        {
            GameWorld gameWorld = ResourceManager.GameWorldManager.GameWorld;
            Matrix4 translationMatrix = Matrix4.Identity;

            RenderStatistics stats = new RenderStatistics();
            for (int z = gameWorld.BlockSize.Z - 1; z >= 0; z--)
            {
                translationMatrix.M43 = z * BlockInformation.BlockSize;
                for (int x = camMinX; x < camMaxX; x++)
                {
                    translationMatrix.M41 = x * BlockInformation.BlockSize;
                    for (int y = camMinY; y < camMaxY; y++)
                    {
                        BlockInformation block = gameWorld[x, y, z];
                        if (!BlockList.BelongsToAnimation(block))
                            continue;

                        BlockInformation newBlock = NextFrameFor(block);
                        if (newBlock != null)
                            gameWorld[x, y, z] = block = newBlock;
                        
                        VoxelGroup voxelGroup;
                        if (!blockMeshes.TryGetValue(block, out voxelGroup))
                            blockMeshes[block] = voxelGroup = new IndexedVoxelGroup(block);

                        translationMatrix.M42 = y * BlockInformation.BlockSize;
                        Matrix4 result;
                        Matrix4.Mult(ref translationMatrix, ref matrixMVP, out result);
                        stats += voxelGroup.Render(ref result);
                    }
                }
            }

            return stats;
        }

        private BlockInformation NextFrameFor(BlockInformation block)
        {
            BlockInformation nextBlock;
            blockAnimationMap.TryGetValue(block, out nextBlock);
            return nextBlock;
        }
    }
}
