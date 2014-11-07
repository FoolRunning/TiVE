using System;
using System.Linq;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    /// <summary>
    /// Creates a world based on a set of generators
    /// </summary>
    internal sealed class GameWorldManager : IDisposable
    {
        public GameWorld GameWorld { get; private set; }

        public bool CreateWorld(int worldXsize, int worldYsize, int worldZsize, long seed)
        {
            Messages.Print("Creating new world...");
            BlockList blockList = ResourceManager.BlockListManager.BlockList;
            GameWorld createdWorld = new GameWorld(worldXsize, worldYsize, worldZsize, blockList);

            try
            {
                foreach (IWorldGeneratorStage generator in TiVEController.PluginManager.GetPluginsOfType<IWorldGeneratorStage>().OrderBy(wg => wg.Priority))
                {
                    Console.WriteLine(generator.StageDescription);
                    generator.UpdateWorld(createdWorld, seed, blockList);
                }
            }
            catch (Exception e)
            {
                Messages.AddFailText();
                Messages.AddStackTrace(e);
                GameWorld = null;
                return false;
            }
            
            Messages.AddDoneText();
            GameWorld = createdWorld;
            return true;
        }

        public void Dispose()
        {
            if (GameWorld != null)
                GameWorld.Dispose();
        }
    }
}
