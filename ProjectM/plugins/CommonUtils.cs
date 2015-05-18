using System;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.ProjectM.Plugins
{
    #region BlockRandomizer class
    public sealed class BlockRandomizer
    {
        private readonly ushort[] blocks;
        private readonly Random random = new Random();

        public BlockRandomizer(IBlockList blockList, string blockname, int blockCount)
        {
            blocks = new ushort[blockCount];
            for (int i = 0; i < blocks.Length; i++)
                blocks[i] = blockList[blockname + i];
        }

        public ushort NextBlock()
        {
            return blocks[random.Next(blocks.Length)];
        }
    }
    #endregion
}
