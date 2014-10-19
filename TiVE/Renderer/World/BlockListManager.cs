using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    internal sealed class BlockListManager
    {
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
        }

        public void UpdateAnimations(float timeSinceLastFrame)
        {
            BlockList.UpdateAnimationMap(timeSinceLastFrame);
        }
    }
}
