namespace ProdigalSoftware.TiVEPluginFramework.Generators
{
    public interface IWorldGenerator
    {
        string BlockListForWorld(string gameWorldName);

        IGameWorld CreateGameWorld(string gameWorldName, IBlockList blockList);
    }
}
