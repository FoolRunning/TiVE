using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.ProjectM.Controllers
{
    /// <summary>
    /// World generation stage that clears the top of the world to create an overworld (outside the caves)
    /// </summary>
    public class WorldGen1 : IWorldGeneratorStage
    {
        public void UpdateWorld(IGameWorld gameWorld, long seed, IBlockList blockList)
        {
        }

        public ushort Priority
        {
            get { return 500; }
        }
        
        public string StageDescription
        {
            get { return "Not Used"; }
        }
    }
}
