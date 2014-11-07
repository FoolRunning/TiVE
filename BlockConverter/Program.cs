using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVE.Renderer.World;

namespace BlockConverter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = @"C:\Programming\Utilities\MagicaVoxel\vox";
                dialog.Filter = "Voxel filess (*.vox)|*.vox|All files (*.*)|*.*";
                dialog.CheckPathExists = true;
                dialog.Multiselect = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string[] files = dialog.FileNames;
                    List<BlockInformation> blocks = new List<BlockInformation>();
                    foreach (string file in files)
                    {
                        using (BinaryReader reader = new BinaryReader(new FileStream(file, FileMode.Open)))
                        {
                            string blockName = Path.GetFileNameWithoutExtension(file);
                            BlockInformation block = MagicaVoxelImporter.CreateBlock(reader, blockName);
                            blocks.Add(block);
                        }
                    }

                    string dir = @"c:\temp\TiVE Blocks";
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    BlockList blockList = new BlockList();
                    blockList.AddBlocks(blocks);
                    blockList.SaveToBlockListFile(dir + @"\SnakeBlocks.TiVEb");
                }
            }
        }
    }
}
