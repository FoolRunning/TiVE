using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProdigalSoftware.TiVEPluginFramework
{
    internal interface IBlockListInternal
    {
        int BlockCount { get; }

        IList<Block> AllBlocks { get; }
    }
}
