using System;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public interface IBlockList : IDisposable
    {
        BlockInformation this[string blockName] { get; }
    }
}
