using System.Collections.Generic;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    internal sealed class BlockList : IBlockList
    {
        private readonly Dictionary<string, ushort> blockToIndexMap = new Dictionary<string, ushort>();
        private readonly List<BlockInformation> blocks = new List<BlockInformation>();

        public BlockList()
        {
            blocks.Add(null); // zero'th item is always empty space
        }

        public int BlockCount
        {
            get { return blocks.Count; }
        }

        public void AddBlock(string blockName, BlockInformation block)
        {
            blockToIndexMap.Add(blockName, (ushort)blocks.Count);
            blocks.Add(block);
        }

        public ushort GetBlockIndex(string blockName)
        {
            return blockToIndexMap[blockName];
        }

        public BlockInformation this[ushort index]
        {
            get { return blocks[index]; }
        }
    }
}
