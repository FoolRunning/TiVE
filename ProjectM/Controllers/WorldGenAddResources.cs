using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.ProjectM.Controllers
{
    public class WorldGenAddResources : IWorldGeneratorStage
    {
        public void UpdateWorld(IGameWorld gameWorld, long seed, IBlockList blockList)
        {

        }

        public ushort Priority
        {
            get { return 400; }
        }

        public string StageDescription
        {
            get { return "Adding Resources"; }
        }
    }
}
