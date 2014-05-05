using System;
using System.Threading;
using System.Windows.Forms;
using ProdigalSoftware.TiVE.Renderer;
using ProdigalSoftware.TiVE.Renderer.OpenGL;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE
{
    public static class TiVEController
    {
        internal static readonly IRendererBackend Backend = new OpenGLRendererBackend();

        private static StarterForm starterForm;

        public static void RunStarter()
        {
            Thread.CurrentThread.Name = "Main UI";
            starterForm = new StarterForm();
            Application.Run(starterForm);
        }

        internal static void RunEngine()
        {
            Thread loadingThread = new Thread(() =>
            {
                GameLogic gameLogic = new GameLogic();
                if (!gameLogic.Initialize())
                    return;

                starterForm.BeginInvoke(new Action(() =>
                {
                    using (IDisplay display = Backend.CreateDisplay())
                        display.RunMainLoop(gameLogic);
                }));
            });
            loadingThread.IsBackground = false;
            loadingThread.Name = "Loading";
            loadingThread.Start();
        }
    }
}
