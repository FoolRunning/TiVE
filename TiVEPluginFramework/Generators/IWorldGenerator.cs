namespace ProdigalSoftware.TiVEPluginFramework.Generators
{
    public interface IWorldGenerator
    {
        IGameWorld CreateGameWorld(string gameWorldName);
    }
}
