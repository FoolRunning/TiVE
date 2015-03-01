using System;
using System.Diagnostics;
using System.Runtime;
using System.Threading;
using System.Windows.Forms;
using ProdigalSoftware.TiVE.Plugins;
using ProdigalSoftware.TiVE.Renderer.OpenTKImpl;
using ProdigalSoftware.TiVE.Resources;
using ProdigalSoftware.TiVE.Scripts;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE
{
    public static class TiVEController
    {
        internal static readonly long MaxTicksForSleep;
        internal static readonly PluginManager PluginManager = new PluginManager();
        internal static readonly ResourceTableDefinitionManager TableDefinitions = new ResourceTableDefinitionManager();
        internal static readonly LuaScripts LuaScripts = new LuaScripts();
        internal static readonly IControllerBackend Backend = new OpenTKBackend();
        internal static readonly UserSettings UserSettings = new UserSettings();

        private static StarterForm starterForm;

        static TiVEController()
        {
            for (int i = 0; i < 100; i++)
            {
                long startTime = Stopwatch.GetTimestamp();
                Thread.Sleep(1);
                long totalTime = Stopwatch.GetTimestamp() - startTime;
                if (totalTime > MaxTicksForSleep)
                    MaxTicksForSleep = totalTime;
            }
            Console.WriteLine("Sleeping for 1ms can be " + MaxTicksForSleep * 1000.0f / Stopwatch.Frequency + "ms long");
        }

        public static void RunStarter()
        {
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

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
