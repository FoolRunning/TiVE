using System.Collections.Generic;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public interface IBlockGenerator
    {
        IEnumerable<BlockInformation> CreateBlocks();
    }
}
