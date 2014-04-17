using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Resources
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
                foreach (BlockInformation blockInfo in generator.CreateBlocks())
                    blockList.AddBlock(blockInfo.BlockName, blockInfo);
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
    }
}
