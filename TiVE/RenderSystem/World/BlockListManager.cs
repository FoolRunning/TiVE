using System.Diagnostics;
using System.IO;
using System.Reflection;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework.Generators;

namespace ProdigalSoftware.TiVE.RenderSystem.World
{
    internal static class BlockListManager
    {
        private const string BlockDirName = "Blocks";

        public static BlockList LoadBlockList(string blockListName)
        {
            Messages.Print(string.Format("Loading block list {0}...", blockListName));

            Stopwatch sw = Stopwatch.StartNew();
            string blockFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), 
                "Data", BlockDirName, blockListName + BlockList.FileExtension);

            BlockList blockList = BlockList.FromFile(blockFilePath) ?? new BlockList();

            foreach (IBlockGenerator generator in TiVEController.PluginManager.GetPluginsOfType<IBlockGenerator>())
                blockList.AddBlocks(generator.CreateBlocks(blockListName));

            if (blockList.BlockCount == 0)
            {
                Messages.AddFailText();
                Messages.AddWarning("Created empty block list");
                return null;
            }

            sw.Stop();
            Messages.AddDoneText();
            Messages.AddDebug(string.Format("Loading {0} blocks took {1}ms", blockList.BlockCount, sw.ElapsedMilliseconds));
            return blockList;
        }
    }
}
