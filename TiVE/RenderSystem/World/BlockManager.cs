using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Generators;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.RenderSystem.World
{
    internal sealed class BlockManager
    {
        private static readonly string emptyBlockName = Block.Empty.Name;
        private readonly MostRecentlyUsedCache<string, Block> blockCache = new MostRecentlyUsedCache<string, Block>(1000);

        public Block GetBlock(string blockName)
        {
            return blockCache.GetFromCache(blockName, LoadBlock);
        }

        private static Block LoadBlock(string blockName)
        {
            if (blockName == emptyBlockName)
                return Block.Empty;

            foreach (IBlockGenerator generator in TiVEController.PluginManager.GetPluginsOfType<IBlockGenerator>())
            {
                Block block = generator.CreateBlock(blockName);
                if (block != null)
                    return block;
            }

            Messages.AddWarning("Missing block: " + blockName);
            return Block.Missing;
        }
    }
}
