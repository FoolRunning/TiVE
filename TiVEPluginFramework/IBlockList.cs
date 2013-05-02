using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public interface IBlockList
    {
        ushort GetBlockIndex(string blockName);
    }
}
