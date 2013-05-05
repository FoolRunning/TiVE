using System;
using System.Collections.Generic;
using System.Linq;
using ProdigalSoftware.TiVE.Plugins;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    /// <summary>
    /// Creates a world based on a set of generators
    /// </summary>
    internal sealed class WorldGenerator
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

        public GameWorld CreateWorld(long seed, BlockList blockList)
        {
            Messages.Print("Creating new world...");
            GameWorld createdWorld = new GameWorld(worldXsize, worldYsize, worldZsize, blockList);

            try
            {
                foreach (IWorldGenerator generator in PluginManager.GetPluginsOfType<IWorldGenerator>().OrderBy(wg => wg.Priority))
                    generator.UpdateWorld(createdWorld, seed, blockList);
            }
            catch (Exception e)
            {
                Messages.AddFailText();
                Messages.AddStackTrace(e);
                return null;
            }
            
            Messages.AddDoneText();

            return createdWorld;
        }
    }
}
