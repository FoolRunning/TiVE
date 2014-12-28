namespace ProdigalSoftware.TiVEPluginFramework
{
    public interface IWorldGenerator
    {
        string BlockListForWorld(string gameWorldName);

        GameWorld CreateGameWorld(string gameWorldName, IBlockList blockList);
    }
}
