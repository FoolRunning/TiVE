using System.Windows.Forms;
using ProdigalSoftware.TiVE.Plugins;
using ProdigalSoftware.TiVE.Renderer;
using ProdigalSoftware.TiVE.Renderer.OpenGL;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE
{
    public static class TiVEController
    {
        internal static IRendererBackend Backend = new OpenGLRendererBackend();

        public static void RunStarter()
        {
            StarterForm form = new StarterForm();
            Application.Run(form);
        }

        internal static void RunEngine()
        {
            PluginManager.LoadPlugins();
                    
            GameLogic gameLogic = new GameLogic();
                    
            using (IDisplay display = Backend.CreateDisplay())
                display.RunMainLoop(gameLogic);
        }
    }
}
