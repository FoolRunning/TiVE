using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Generators;

namespace ProdigalSoftware.TiVE.RenderSystem.World
{
    internal sealed class BlockManager
    {
        private readonly Dictionary<string, Block> blockNameToBlock = new Dictionary<string, Block>(5000);

        public bool Initialize()
        {
            Messages.Print("Loading blocks...");

            Stopwatch sw = Stopwatch.StartNew();

            blockNameToBlock.Add(Block.Empty.Name, Block.Empty);

            List<string> blockWarnings = new List<string>();
            foreach (IBlockGenerator generator in TiVEController.PluginManager.GetPluginsOfType<IBlockGenerator>())
            {
                try
                {
                    foreach (Block block in generator.CreateBlocks())
                    {
                        if (blockNameToBlock.ContainsKey(block.Name))
                            blockWarnings.Add("Duplicate block definition for block " + block.Name);

                        blockNameToBlock[block.Name] = block;
                    }
                }
                catch (Exception e)
                {
                    blockWarnings.Add(e.ToString());
                }
            }

            sw.Stop();

            if (blockNameToBlock.Count == 1)
            {
                Messages.AddFailText();
                Messages.AddError("Couldn't find any blocks");
                return false;
            }

            Messages.AddDoneText();
            Messages.AddDebug(string.Format("Loading {0} blocks took {1}ms", blockNameToBlock.Count, sw.ElapsedMilliseconds));
            foreach (string message in blockWarnings)
                Messages.AddWarning(message);
            return true;
        }

        public Block GetBlock(string blockName)
        {
            Block block;
            if (blockNameToBlock.TryGetValue(blockName, out block))
                return block;

            Messages.AddWarning("Could not find block " + blockName);
            return Block.Empty;
        }
    }
}
