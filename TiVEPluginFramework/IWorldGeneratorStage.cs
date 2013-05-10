namespace ProdigalSoftware.TiVEPluginFramework
{
    public interface IWorldGeneratorStage
    {
        void UpdateWorld(IGameWorld gameWorld, long seed, IBlockList blockList);
        
        ushort Priority { get;  }

        string StageDescription { get; }
    }
}
