using System.IO;
using System.Reflection;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    internal static class BlockListManager
    {
        private const string BlockDirName = "Blocks";

        public static BlockList LoadBlockList(string blockListName)
        {
            Messages.Print(string.Format("Loading block list {0}...", blockListName));

            string blockFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), 
                "Data", BlockDirName, blockListName + "." + BlockList.FileExtension);

            BlockList blockList = BlockList.FromFile(blockFilePath) ?? new BlockList();

            foreach (IBlockGenerator generator in TiVEController.PluginManager.GetPluginsOfType<IBlockGenerator>())
            {
                blockList.AddBlocks(generator.CreateBlocks(blockListName));
                blockList.AddAnimations(generator.CreateAnimations(blockListName));
            }

            if (blockList.BlockCount == 0)
            {
                Messages.AddFailText();
                Messages.AddWarning("Created empty block list");
                return null;
            }

            Messages.AddDoneText();
            return blockList;
        }
    }
}
