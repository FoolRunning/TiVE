using System;
using System.Diagnostics;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Generators;

namespace ProdigalSoftware.TiVE.RenderSystem.World
{
    /// <summary>
    /// 
    /// </summary>
    internal static class GameWorldManager
    {
        public static GameWorld LoadGameWorld(string gameWorldName)
        {
            // Plugin gave name of blocklist to load (which should mean it will generate a world for us as well)
            GameWorld result = CreateWorldFromPlugin(gameWorldName);
            if (result != null)
                return result;

            // TODO: Implement saving/loading of game worlds

            Messages.AddFailText();
            Messages.AddError("Could not find world " + gameWorldName);
            return null;
        }

        private static GameWorld CreateWorldFromPlugin(string gameWorldName)
        {
            Messages.Print("Loading world " + gameWorldName + "...");
            Stopwatch sw = Stopwatch.StartNew();
            GameWorld createdWorld = null;
            try
            {
                foreach (IWorldGenerator generator in TiVEController.PluginManager.GetPluginsOfType<IWorldGenerator>())
                {
                    createdWorld = (GameWorld)generator.CreateGameWorld(gameWorldName);
                    if (createdWorld != null)
                        break;
                }
            }
            catch (Exception e)
            {
                Messages.AddFailText();
                Messages.AddStackTrace(e);
                return null;
            }
            finally
            {
                sw.Stop();
            }

            if (createdWorld == null)
                return null;

            createdWorld.Initialize();

            long totalVoxels = 0;
            long totalBlocks = 0;
            for (int wz = 0; wz < createdWorld.BlockSize.Z; wz++)
            {
                for (int wx = 0; wx < createdWorld.BlockSize.X; wx++)
                {
                    for (int wy = 0; wy < createdWorld.BlockSize.Y; wy++)
                    {
                        Block block = createdWorld[wx, wy, wz];
                        if (block != Block.Empty)
                        {
                            totalBlocks++;
                            totalVoxels += block.TotalVoxels;
                        }
                    }
                }
            }

            Messages.AddDoneText();
            Messages.AddDebug(string.Format("Loading world took {0}ms", sw.ElapsedMilliseconds));
            Messages.AddDebug(string.Format("Blocks in world: {0:N0}", totalBlocks));
            Messages.AddDebug(string.Format("Voxels in world: {0:N0}", totalVoxels));

            return createdWorld;
        }
    }
}
