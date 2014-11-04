using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Lighting;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.ProjectS.Controllers
{
    public class DefaultBlockLoader : IBlockGenerator
    {
        public IEnumerable<BlockInformation> CreateBlocks()
        {
            string dir = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Data", "Blocks");
            foreach (string filePath in Directory.GetFiles(dir, "*.TiVEb"))
            {
                BlockInformation block = BlockInformation.FromFile(filePath);
                if (block != null)
                {
                    if (block.BlockName == "LightRed")
                        yield return new BlockInformation(block, "Light0", null, new PointLight(new Vector3b(5, 5, 7), new Color3f(1.0f, 0.0f, 0.0f), 0.005f));
                    else if (block.BlockName == "LightGreen")
                        yield return new BlockInformation(block, "Light1", null, new PointLight(new Vector3b(5, 5, 7), new Color3f(0.0f, 1.0f, 0.0f), 0.005f));
                    else if (block.BlockName == "LightBlue")
                        yield return new BlockInformation(block, "Light2", null, new PointLight(new Vector3b(5, 5, 7), new Color3f(0.0f, 0.0f, 1.0f), 0.004f));
                    else
                        yield return block;
                }
            }
        }

        public IEnumerable<BlockAnimationDefinition> CreateAnimations()
        {
            return null;
        }
    }
}
