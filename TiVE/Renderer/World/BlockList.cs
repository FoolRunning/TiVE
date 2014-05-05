using System.Collections.Generic;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    internal sealed class BlockList : IBlockList
    {
        private readonly Dictionary<string, BlockInformation> blockToIndexMap = new Dictionary<string, BlockInformation>();

        public int BlockCount
        {
            get { return blockToIndexMap.Count; }
        }

        public void AddBlock(string blockName, BlockInformation block)
        {
            blockToIndexMap.Add(blockName, block);
        }

        public BlockInformation this[string blockName]
        {
            get { return blockToIndexMap[blockName]; }
        }
    }
}
