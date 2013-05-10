using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    public struct WorldBlock
    {
        public ushort BlockIndex;
        public byte Biome;

        public WorldBlock(ushort blockIndex, byte biome)
        {
            BlockIndex = blockIndex;
            Biome = biome;
        }
    }
}
