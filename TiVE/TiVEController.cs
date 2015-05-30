using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime;
using System.Threading;
using System.Windows.Forms;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVE.Core.Backend.OpenTKImpl;
using ProdigalSoftware.TiVE.Plugins;
using ProdigalSoftware.TiVE.RenderSystem.World;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE
{
    public static class TiVEController
    {
        internal static readonly long MaxTicksForSleep;
        internal static readonly PluginManager PluginManager = new PluginManager();
        internal static readonly IControllerBackend Backend = new OpenTKBackend();
        internal static readonly UserSettings UserSettings = new UserSettings();
        internal static Engine Engine;

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

            Factory.Implementation = new FactoryImpl();
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

        internal static void RunEngine(string sceneToLoad)
        {
            Engine = new Engine(60);
            Engine.AddSystem(new RenderSystem.RenderSystem());
            Engine.AddSystem(new GUISystem.GUISystem());
            Engine.AddSystem(new ScriptSystem.ScriptSystem(Backend.Keyboard, Backend.Mouse));
            Engine.AddSystem(new CameraSystem.CameraSystem());
            Engine.AddSystem(new ParticleSystem.ParticleSystem());

            Engine.MainLoop(sceneToLoad);
        }

        static void starterForm_VisibleChanged(object sender, EventArgs e)
        {
            Thread initialLoadThread = new Thread(() =>
            {
                bool success = PluginManager.LoadPlugins();
                if (success)
                    UserSettings.Load();
                if (success)
                    starterForm.AfterInitialLoad();

                DoVoxelRayCastTest();
                DoBlockLineTest();
            });
            initialLoadThread.IsBackground = true;
            initialLoadThread.Name = "InitialLoad";
            initialLoadThread.Start();
        }

        private static void starterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            PluginManager.Dispose();
            UserSettings.Save();
        }

        private static void DoVoxelRayCastTest()
        {
            GameWorld gameWorld = new GameWorld(100, 100, 100);
            BlockList blockList = new BlockList();
            blockList.AddBlock(new BlockImpl("dummy"));
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
            for (int t = 0; t < 20; t++)
            {
                sw.Restart();
                for (int i = 0; i < 10000; i++)
                    gameWorld.NoVoxelInLine(center, center, center, i % center + 200, i % center + 200, i % center + 200);
                sw.Stop();
                totalMs += sw.ElapsedMilliseconds;
            }
            Messages.Println(string.Format("10,000 ray casts took average of {0}ms", totalMs / 20.0f), Color.Chocolate);
        }

        private static void DoBlockLineTest()
        {
            GameWorld gameWorld = new GameWorld(200, 200, 200);
            BlockList blockList = new BlockList();
            BlockImpl block = new BlockImpl("dummy");
            block.AddComponent(TransparentComponent.Instance);
            blockList.AddBlock(block);
            ushort blockId = blockList["dummy"];
            for (int x = 0; x < 100; x++)
            {
                for (int z = 0; z < 100; z++)
                {
                    for (int y = 0; y < 100; y++)
                        gameWorld[x, y, z] = blockId;
                }
            }

            gameWorld.Initialize(blockList);
            int center = gameWorld.BlockSize.X / 2;

            long totalMs = 0;
            Stopwatch sw = new Stopwatch();
            for (int t = 0; t < 20; t++)
            {
                sw.Restart();
                for (int i = 0; i < 10000; i++)
                    gameWorld.LessThanBlockCountInLine(center, center, center, i % center + 98, i % center + 98, i % center + 98, 2);
                sw.Stop();
                totalMs += sw.ElapsedMilliseconds;
            }
            Messages.Println(string.Format("10,000 block lines took average of {0}ms", totalMs / 20.0f), Color.Chocolate);
        }
    }
}
