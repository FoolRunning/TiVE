using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ProdigalSoftware.TiVEPluginFramework;

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
                    foreach (string file in files)
                    {
                        using (BinaryReader reader = new BinaryReader(new FileStream(file, FileMode.Open)))
                        {
                            string blockName = Path.GetFileNameWithoutExtension(file);
                            BlockInformation block = MagicaVoxelImporter.CreateBlock(reader, blockName);
                            if (block != null)
                                WriteBlock(block);
                        }
                    }
                }
            }
        }

        private static void WriteBlock(BlockInformation block)
        {
            string dir = @"c:\temp\TiVE Blocks";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string filePath = dir + "\\" + block.BlockName + ".TiVEb";
            using (BinaryWriter writer = new BinaryWriter(new FileStream(filePath, FileMode.Create), Encoding.ASCII))
            {
                writer.Write("TiVEb");
                writer.Write((byte)BlockInformation.VoxelSize);
                for (int i = 0; i < block.VoxelsArray.Length; i++)
                    writer.Write(block.VoxelsArray[i]);
            }
        }
    }
}
