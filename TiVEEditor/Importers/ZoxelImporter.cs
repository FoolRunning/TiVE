using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVEEditor.Importers
{
    public static class ZoxelImporter
    {
        public static void Import()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                JObject obj = JObject.Load(new JsonTextReader(new StreamReader(dlg.FileName)));
                int width = (int)obj["width"];
                int height = (int)obj["height"];
                int depth = (int)obj["depth"];
                int frameCount = (int)obj["frames"];
                List<VoxelSprite> sprites = new List<VoxelSprite>();
                for (int frameNum = 1; frameNum <= frameCount; frameNum++)
                {
                    VoxelSprite sprite = new VoxelSprite(width, height, depth);
                    foreach (JArray token in obj["frame" + frameNum].OfType<JArray>())
                    {
                        if (token.Count == 4)
                            sprite[(int)token[0], (int)token[1], (int)token[2]] = (Voxel)(uint)token[3];
                    }
                    sprites.Add(sprite);
                }
            }
        }

    }
}
