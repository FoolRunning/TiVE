using System;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    /// <summary>
    /// Creates a world based on a set of generators
    /// </summary>
    internal static class GameWorldManager
    {
        public static GameWorld LoadGameWorld(string gameWorldName)
        {
            // TODO: Implement this method when saving/loading of game worlds is implemented
            Messages.Print("Creating new world...");
            GameWorld createdWorld = new GameWorld(10, 10, 10, null);

            try
            {
                foreach (IWorldGenerator generator in TiVEController.PluginManager.GetPluginsOfType<IWorldGenerator>())
                    generator.UpdateGameWorld(createdWorld, gameWorldName);
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

        public static GameWorld GenerateGameWorld(BlockList blockList, string worldName, int worldXsize, int worldYsize, int worldZsize)
        {
            Messages.Print("Creating new world...");
            GameWorld createdWorld = new GameWorld(worldXsize, worldYsize, worldZsize, blockList);

            try
            {
                foreach (IWorldGenerator generator in TiVEController.PluginManager.GetPluginsOfType<IWorldGenerator>())
                    generator.UpdateGameWorld(createdWorld, worldName);
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
