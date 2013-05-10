using System.Threading;
using System.Windows.Forms;
using ProdigalSoftware.TiVE.Plugins;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE
{
    public static class TiVEController
    {
        public static void RunStarter()
        {
            StarterForm form = new StarterForm();
            Application.Run(form);
        }

        internal static void RunEngine()
        {
            Thread gameThread = new Thread(() =>
                {
                    PluginManager.LoadPlugins();

                    using (Game game = new Game())
                        game.Run(60);
                });
            gameThread.Name = "gameThread";
            gameThread.Start();
        }
    }
}
