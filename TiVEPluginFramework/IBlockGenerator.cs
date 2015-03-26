using System.Collections.Generic;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public interface IBlockGenerator
    {
        IEnumerable<Block> CreateBlocks(string blockListName);
    }
}
