using System;
using System.Collections.Generic;
using ProdigalSoftware.TiVE.Plugins;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    internal sealed class BlockList : IBlockList
    {
        private readonly Dictionary<string, ushort> blockToIndexMap = new Dictionary<string, ushort>();
        private readonly List<Block> blocks = new List<Block>();

        private BlockList()
        {
            blocks.Add(null); // zero'th item is always empty space
        }

        public void DeleteBlocks()
        {
            foreach (Block block in blocks)
                block.Delete();
        }

        public static BlockList CreateBlockList()
        {
            Messages.Print("Creating blocks...");

            BlockList blockList = new BlockList();

            foreach (IBlockGenerator generator in PluginManager.GetPluginsOfType<IBlockGenerator>())
            {
                foreach (BlockInformation blockInfo in generator.CreateBlocks())
                    blockList.AddBlock(blockInfo.BlockName, new Block(blockInfo));
            }

            return blockList;
        }

        public void AddBlock(string blockName, Block block)
        {
            blockToIndexMap.Add(blockName, (ushort)blocks.Count);
            blocks.Add(block);
        }

        public ushort GetBlockIndex(string blockName)
        {
            ushort blockIndex;
            return blockToIndexMap.TryGetValue(blockName, out blockIndex) ? blockIndex : (ushort)0;
        }

        public Block this[ushort index]
        {
            get { return blocks[index]; }
        }
    }
}
