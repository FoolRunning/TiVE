using System;
using System.Diagnostics;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    /// <summary>
    /// Creates a world based on a set of generators
    /// </summary>
    public sealed class WorldGenerator
    {
        private readonly int worldXsize;
        private readonly int worldYsize;
        private readonly int worldZsize;

        public WorldGenerator(int worldXsize, int worldYsize, int worldZsize)
        {
            this.worldXsize = worldXsize;
            this.worldYsize = worldYsize;
            this.worldZsize = worldZsize;
        }

        public GameWorld CreateWorld(long seed)
        {
            GameWorld createdWorld = new GameWorld(worldXsize, worldYsize, worldZsize);
            Random rand1 = new Random((int)((seed >> 32) & 0xFFFFFFFF));
            Random rand2 = new Random((int)(seed & 0xFFFFFFFF));

            int lastPercent = -1;
            for (int x = 0; x < worldXsize; x++)
            {
                for (int y = 0; y < worldYsize; y++)
                {
                    for (int z = 0; z < worldZsize; z++)
                    {
                        if (z == 0)
                            createdWorld.SetBlock(x, y, z, (ushort)(50));
                        else
                            createdWorld.SetBlock(x, y, z, (ushort)(1));
                    }
                }
                int newPercent = x * 100 / (worldXsize - 1);
                if (newPercent != lastPercent)
                {
                    Debug.WriteLine(newPercent + "%");
                    lastPercent = newPercent;
                }
            }

            return createdWorld;
        }
    }
}
