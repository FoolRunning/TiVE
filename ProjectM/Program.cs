using System;
using System.Windows.Forms;
using ProdigalSoftware.TiVE;

namespace ProdigalSoftware.ProjectM
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

            TiVEController.RunStarter();
        }
    }
}
