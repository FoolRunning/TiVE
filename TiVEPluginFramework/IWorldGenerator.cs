namespace ProdigalSoftware.TiVEPluginFramework
{
    public interface IWorldGenerator
    {
        void UpdateWorld(IGameWorld gameWorld, long seed, IBlockList blockList);

        uint Priority { get;  }
    }
}
