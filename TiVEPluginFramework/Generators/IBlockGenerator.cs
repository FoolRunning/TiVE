using System.Collections.Generic;

namespace ProdigalSoftware.TiVEPluginFramework.Generators
{
    public interface IBlockGenerator
    {
        IEnumerable<Block> CreateBlocks(string blockListName);
        //Block GetBlock(string blockName);
    }
}
