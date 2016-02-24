using System;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.ProjectM.Plugins
{
    public sealed class BlockRandomizer
    {
        private readonly Block[] blocks;
        private readonly Random random = new Random();

        public BlockRandomizer(string blockname, int blockCount)
        {
            blocks = new Block[blockCount];
            for (int i = 0; i < blocks.Length; i++)
                blocks[i] = Factory.Get<Block>(blockname + i);
        }

        public Block NextBlock()
        {
            return blocks[random.Next(blocks.Length)];
        }
    }
}
