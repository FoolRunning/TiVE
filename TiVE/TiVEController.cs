using System;
using System.Diagnostics;
using System.Runtime;
using System.Threading;
using System.Windows.Forms;
using ProdigalSoftware.TiVE.Plugins;
using ProdigalSoftware.TiVE.Renderer.OpenTKImpl;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVE.Resources;
using ProdigalSoftware.TiVE.Scripts;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;

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

        internal static void RunEngine(string startScript)
        {
            Thread loadingThread = new Thread(() =>
            {
                GameLogic gameLogic = new GameLogic();
                if (!gameLogic.Initialize(startScript))
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

            //DoTest();
        }

        private static void starterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            LuaScripts.Dispose();
            TableDefinitions.Dispose();
            PluginManager.Dispose();
            UserSettings.Save();
        }

        private static void DoTest()
        {
            GameWorld gameWorld = new GameWorld(100, 100, 100);
            BlockList blockList = new BlockList();
            blockList.AddBlock(new Block("dummy"));
            ushort block = blockList["dummy"];
            for (int x = 0; x < 100; x++)
            {
                for (int z = 0; z < 100; z++)
                {
                    for (int y = 0; y < 100; y++)
                        gameWorld[x, y, z] = block;
                }
            }

            gameWorld.Initialize(blockList);
            int center = gameWorld.VoxelSize.X / 2;

            long totalMs = 0;
            Stopwatch sw = new Stopwatch();
            for (int t = 0; t < 10; t++)
            {
                sw.Restart();
                for (int i = 0; i < 10000; i++)
                    gameWorld.NoVoxelInLine(center, center, center, i % center + 200, i % center + 200, i % center + 200);
                sw.Stop();
                totalMs += sw.ElapsedMilliseconds;
            }

            Console.WriteLine("Took average of {0}ms", totalMs / 10.0f);
        }
    }
}
