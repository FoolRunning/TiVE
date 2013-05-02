using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public interface IGameWorld
    {
        int Xsize { get; }

        int Ysize { get; }

        int Zsize { get; }

        void SetBlock(int x, int y, int z, ushort blockIndex);

        ushort GetBlock(int x, int y, int z);
    }
}
