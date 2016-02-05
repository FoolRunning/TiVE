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
        internal static readonly ResourceLoader ResourceLoader = new ResourceLoader();
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
            TiVESerializer.Implementation = new TiVESerializerImplementation();
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
            Engine.AddSystem(new ParticleSystem.ParticleSystem());
            Engine.AddSystem(new GUISystem.GUISystem());

            Engine.AddSystem(new ScriptSystem.ScriptSystem(Backend.Keyboard, Backend.Mouse));
            Engine.AddSystem(new CameraSystem.CameraSystem());

            Engine.MainLoop(sceneToLoad);
        }

        private static void starterForm_VisibleChanged(object sender, EventArgs e)
        {
            Thread initialLoadThread = new Thread(() =>
            {
                bool success = PluginManager.LoadPlugins();
                if (success)
                    UserSettings.Load();
                if (success)
                    success = ResourceLoader.Initialize();
                if (success)
                    success = ((TiVESerializerImplementation)TiVESerializer.Implementation).Initialize();

                starterForm.AfterInitialLoad(success);

                //TestIntStructAccess();
                DoBlockLineTest();
                DoFastVoxelRayCastTest();
                DoVoxelRayCastTest();
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

        #region Ray cast speed tests
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

        private static void DoFastVoxelRayCastTest()
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
                    gameWorld.NoVoxelInLineFast(center, center, center, i % center + 200, i % center + 200, i % center + 200);
                sw.Stop();
                totalMs += sw.ElapsedMilliseconds;
            }
            Messages.Println(string.Format("10,000 fast ray casts took average of {0}ms", totalMs / 20.0f), Color.Chocolate);
        }

        private static void DoBlockLineTest()
        {
            GameWorld gameWorld = new GameWorld(200, 200, 200);
            BlockList blockList = new BlockList();
            BlockImpl block = new BlockImpl("dummy");
            block.AddComponent(new LightPassthroughComponent());
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
                    gameWorld.NoBlocksInLine(center, center, center, i % center + 98, i % center + 98, i % center + 98);
                sw.Stop();
                totalMs += sw.ElapsedMilliseconds;
            }
            Messages.Println(string.Format("10,000 block lines took average of {0}ms", totalMs / 20.0f), Color.Chocolate);
        }
        #endregion

        #region Struct with int vs. int speed
        //private static void TestIntStructAccess()
        //{
        //    Console.WriteLine();
        //    StructInt();
        //    Int();
        //}

        //private static void StructInt()
        //{
        //    IntStruct[] values = new IntStruct[1000000];

        //    double total = 0.0;
        //    var timer = new Stopwatch();
        //    for (int passes = 0; passes < 300; passes++)
        //    {
        //        timer.Restart();
        //        for (int i = 0; i < values.Length; i++)
        //            values[i] = new IntStruct(i % 20);
        //        int totalValue = 0;
        //        for (int i = 0; i < values.Length; i++)
        //            totalValue += values[i];
        //        timer.Stop();
        //        total += timer.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
        //        //Console.WriteLine("values: {0}, {1}, {2}, {3}, {4}, {5}, {6}", 
        //        //    values[0].Item1, values[0].Item2, values[0].Item3, values[0].Item4, values[0].Item5, values[0].Item6, values[0].Item7);
        //    }
        //    Messages.Println(string.Format("{0}: {1,7:0.000} ", "Struct", total / 300), Color.Chocolate);
        //}

        //private static void Int()
        //{
        //    int[] values = new int[1000000];

        //    double total = 0.0;
        //    var timer = new Stopwatch();
        //    for (int passes = 0; passes < 300; passes++)
        //    {
        //        timer.Restart();
        //        for (int i = 0; i < values.Length; i++)
        //            values[i] = i % 20;
        //        int totalValue = 0;
        //        for (int i = 0; i < values.Length; i++)
        //            totalValue += values[i];
        //        timer.Stop();
        //        total += timer.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
        //        //Console.WriteLine("values: {0}, {1}, {2}, {3}, {4}, {5}, {6}", 
        //        //    values[0].Item1, values[0].Item2, values[0].Item3, values[0].Item4, values[0].Item5, values[0].Item6, values[0].Item7);
        //    }

        //    Messages.Println(string.Format("{0}: {1,7:0.000} ", "Int", total / 300), Color.Chocolate);
        //}

        //private struct IntStruct
        //{
        //    public readonly int Value;

        //    public IntStruct(int value)
        //    {
        //        Value = value;
        //    }

        //    public static implicit operator int(IntStruct val)
        //    {
        //        return val.Value;
        //    }
        //}
        #endregion
    }
}
