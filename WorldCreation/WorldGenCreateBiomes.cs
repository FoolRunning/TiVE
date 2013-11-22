using System.Threading.Tasks;
using ProdigalSoftware.TiVEPluginFramework;

namespace WorldCreation
{
    /// <summary>
    /// World generation stage to create the bioms of the game world
    /// </summary>
    public sealed class WorldGenCreateBiomes : IWorldGeneratorStage
    {
        public void UpdateWorld(IGameWorld gameWorld, long seed, IBlockList blockList)
        {
            // Use parallel for for speed since there is no syncing needed
            Parallel.For(0, gameWorld.Xsize, x =>
                {
                    for (int y = 0; y < gameWorld.Ysize; y++)
                    {
                        gameWorld.SetBiome(x, y, 0);
                    }
                });
        }

        public ushort Priority
        {
            get { return 100; }
        }

        public string StageDescription
        {
            get { return "Creating Biomes"; }
        }
    }
}
