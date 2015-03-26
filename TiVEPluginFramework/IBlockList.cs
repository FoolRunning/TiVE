using System;
using JetBrains.Annotations;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public interface IBlockList : IDisposable
    {
        [UsedImplicitly]
        ushort this[string blockName] { get; }

        [UsedImplicitly]
        Block this[int blockIndex] { get; }
    }
}
