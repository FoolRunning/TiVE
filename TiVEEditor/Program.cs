using System;
using System.Threading;
using System.Windows.Forms;
using ProdigalSoftware.TiVEEditor.Importers;

namespace ProdigalSoftware.TiVEEditor
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
            Thread.CurrentThread.Name = "Main UI";
            ZoxelImporter.Import();

            Application.Run(new TiVEEditorForm());
        }
    }
}
