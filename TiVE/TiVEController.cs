using System;
using System.Threading;
using System.Windows.Forms;
using ProdigalSoftware.TiVE.Plugins;
using ProdigalSoftware.TiVE.Renderer.OpenGL;
using ProdigalSoftware.TiVE.Resources;
using ProdigalSoftware.TiVE.Scripts;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE
{
    public enum FullScreenMode
    {
        Windowed,
        FullScreen,
        WindowFullScreen
    }

    public static class TiVEController
    {
        internal static readonly PluginManager PluginManager = new PluginManager();
        internal static readonly ResourceTableDefinitionManager TableDefinitions = new ResourceTableDefinitionManager();
        internal static readonly LuaScripts LuaScripts = new LuaScripts();
        internal static readonly IRendererBackend Backend = new OpenGLRendererBackend();
        internal static readonly UserSettings UserSettings = new UserSettings();

        private static StarterForm starterForm;

        public static void RunStarter()
        {
            Thread.CurrentThread.Name = "Main UI";
            starterForm = new StarterForm();
            starterForm.FormClosing += starterForm_FormClosing;
            starterForm.VisibleChanged += starterForm_VisibleChanged;

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
                    gameLogic.RunMainLoop();
                }));
            });
            loadingThread.IsBackground = false;
            loadingThread.Name = "Loading";
            loadingThread.Start();
        }

        static void starterForm_VisibleChanged(object sender, EventArgs e)
        {
            Thread initialLoadThread = new Thread(() =>
            {
                bool success = PluginManager.LoadPlugins();
                if (success)
                    UserSettings.Load();
                //if (success)
                //   success = TableDefinitions.Initialize();
                if (success)
                    success = LuaScripts.Initialize();

                if (success)
                    starterForm.AfterInitialLoad();
            });
            initialLoadThread.IsBackground = true;
            initialLoadThread.Name = "InitialLoad";
            initialLoadThread.Start();
        }

        private static void starterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            LuaScripts.Dispose();
            TableDefinitions.Dispose();
            PluginManager.Dispose();
            UserSettings.Save();
        }
    }
}
