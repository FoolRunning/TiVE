using System;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    /// <summary>
    /// 
    /// </summary>
    internal static class GameWorldManager
    {
        public static GameWorld LoadGameWorld(string gameWorldName, out BlockList blockList)
        {
            string blockListName = null;
            try
            {
                foreach (IWorldGenerator generator in TiVEController.PluginManager.GetPluginsOfType<IWorldGenerator>())
                {
                    blockListName = generator.BlockListForWorld(gameWorldName);
                    if (blockListName != null)
                        break;
                }
            }
            catch (Exception e)
            {
                Messages.AddStackTrace(e);
            }

            blockList = null;
            if (blockListName != null)
            {
                // Plugin gave name of blocklist to load (which should mean it will generate a world for us as well)
                return CreateWorldFromPlugin(blockListName, gameWorldName, out blockList);
            }

            // TODO: Implement saving/loading of game worlds

            Messages.AddError("Could not find world " + gameWorldName);
            return null;
        }

        private static GameWorld CreateWorldFromPlugin(string blockListName, string gameWorldName, out BlockList blockList)
        {
            blockList = BlockListManager.LoadBlockList(blockListName);
            if (blockList == null)
            {
                Messages.AddError("Could not get block list for world " + gameWorldName);
                return null;
            }

            Messages.Print("Loading world " + gameWorldName + "...");
            try
            {
                foreach (IWorldGenerator generator in TiVEController.PluginManager.GetPluginsOfType<IWorldGenerator>())
                {
                    GameWorld createdWorld = generator.CreateGameWorld(gameWorldName, blockList);
                    if (createdWorld != null)
                    {
                        Messages.AddDoneText();
                        
                        long totalVoxels = 0;
                        int totalBlocks = 0;
                        for (int wz = 0; wz < createdWorld.BlockSize.Z; wz++)
                        {
                            for (int wx = 0; wx < createdWorld.BlockSize.X; wx++)
                            {
                                for (int wy = 0; wy < createdWorld.BlockSize.Y; wy++)
                                {
                                    ushort block = createdWorld[wx, wy, wz];
                                    if (block != 0)
                                    {
                                        totalBlocks++;
                                        totalVoxels += blockList[block].TotalVoxels;
                                    }
                                }
                            }
                        }

                        Messages.AddDebug(string.Format("Blocks in world: {0:N0}", totalBlocks));
                        Messages.AddDebug(string.Format("Voxels in world: {0:N0}", totalVoxels));

                        return createdWorld;
                    }
                }
            }
            catch (Exception e)
            {
                Messages.AddFailText();
                Messages.AddStackTrace(e);
                return null;
            }

            Messages.AddFailText();
            Messages.AddError("Could not find world " + gameWorldName);
            return null;
        }
    }
}
