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
        internal static readonly BlockManager BlockManager = new BlockManager();
        internal static readonly IControllerBackend Backend = new OpenTKBackend();
        internal static readonly UserSettings UserSettings = new UserSettings();
        internal static Engine Engine;

        private static StarterForm starterForm;

        static TiVEController()
        {
            for (int i = 0; i < 30; i++)
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
            Engine.AddSystem(new ScriptSystem.ScriptSystem(Backend.Keyboard, Backend.Mouse));
            Engine.AddSystem(new AISystem.AISystem());
            Engine.AddSystem(new CameraSystem.CameraSystem());
            Engine.AddSystem(new SoundSystem.SoundSystem());
            Engine.AddSystem(new VoxelMeshSystem.VoxelMeshSystem());

            Engine.AddSystem(new RenderSystem.RenderSystem());
            Engine.AddSystem(new ParticleSystem.ParticleSystem());
            Engine.AddSystem(new GUISystem.GUISystem());

            Engine.MainLoop(sceneToLoad);

            Engine = null;
            GC.Collect();
        }

        private static void starterForm_VisibleChanged(object sender, EventArgs e)
        {
            Thread initialLoadThread = new Thread(() =>
            {
                bool success = ResourceLoader.Initialize();
                if (success)
                    success = PluginManager.LoadPlugins();
                if (success)
                    UserSettings.Load();
                if (success)
                    success = ((TiVESerializerImplementation)TiVESerializer.Implementation).Initialize();

                starterForm.AfterInitialLoad(success);

                //TestIntStructAccess();
                //TestRandomSpeed();
                //DoBlockLineTest();
                //DoSqrtSpeedTests();
                DoRayCastTests();
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
        private static void DoRayCastTests()
        {
            DoVoxelRayCastTest();
            //DoVoxelRayCastFastTest();
        }

        private static void DoVoxelRayCastTest()
        {
            GameWorld gameWorld = new GameWorld(100, 100, 100);
            Block block = new Block("dummy");
            for (int x = 0; x < 100; x++)
            {
                for (int z = 0; z < 100; z++)
                {
                    for (int y = 0; y < 100; y++)
                        gameWorld[x, y, z] = (x + y + z % 3 == 0) ? block : Block.Empty;
                }
            }

            gameWorld.Initialize();

            //for (int level = (int)LODLevel.V32; level <= (int)LODLevel.V4; level++)
            {
                LODLevel detailLevel = LODLevel.V32; // (LODLevel)level;
                long totalMs = 0;
                Stopwatch sw = new Stopwatch();
                int center = gameWorld.VoxelSize32.X / LODUtils.AdjustForDetailLevelTo32(2, detailLevel);
                int offSet = LODUtils.AdjustForDetailLevelFrom32(200, detailLevel);
                int minMs = int.MaxValue;
                int maxMs = 0;
                for (int t = 0; t < 10; t++)
                {
                    sw.Restart();
                    for (int i = 0; i < 10000; i++)
                        gameWorld.NoVoxelInLine(center, center, center, i % center + offSet, i % center + offSet, i % center + offSet, detailLevel);
                    sw.Stop();
                    totalMs += sw.ElapsedMilliseconds;
                    if (sw.ElapsedMilliseconds < minMs)
                        minMs = (int)sw.ElapsedMilliseconds;
                    if (sw.ElapsedMilliseconds > maxMs)
                        maxMs = (int)sw.ElapsedMilliseconds;
                }
                Messages.Println($"10,000 ray casts at detail {detailLevel} took average of {totalMs / 10.0f}ms ({minMs}-{maxMs})", Color.Chocolate);
            }
        }

        //private static void DoVoxelRayCastFastTest()
        //{
        //    GameWorld gameWorld = new GameWorld(100, 100, 100);
        //    Block block = new Block("dummy");
        //    for (int x = 0; x < 100; x++)
        //    {
        //        for (int z = 0; z < 100; z++)
        //        {
        //            for (int y = 0; y < 100; y++)
        //                gameWorld[x, y, z] = (x + y + z % 3 == 0) ? block : Block.Empty;
        //        }
        //    }

        //    gameWorld.Initialize();

        //    //for (int level = (int)LODLevel.V32; level <= (int)LODLevel.V4; level++)
        //    {
        //        LODLevel detailLevel = LODLevel.V32; // (LODLevel)level;
        //        long totalMs = 0;
        //        Stopwatch sw = new Stopwatch();
        //        int center = gameWorld.VoxelSize32.X / LODUtils.AdjustForDetailLevelTo32(2, detailLevel);
        //        int offSet = LODUtils.AdjustForDetailLevelFrom32(200, detailLevel);
        //        int minMs = int.MaxValue;
        //        int maxMs = 0;
        //        for (int t = 0; t < 10; t++)
        //        {
        //            sw.Restart();
        //            for (int i = 0; i < 10000; i++)
        //                gameWorld.NoVoxelInLineFast(center, center, center, i % center + offSet, i % center + offSet, i % center + offSet, detailLevel);
        //            sw.Stop();
        //            totalMs += sw.ElapsedMilliseconds;
        //            if (sw.ElapsedMilliseconds < minMs)
        //                minMs = (int)sw.ElapsedMilliseconds;
        //            if (sw.ElapsedMilliseconds > maxMs)
        //                maxMs = (int)sw.ElapsedMilliseconds;
        //        }
        //        Messages.Println($"10,000 fast ray casts at detail {detailLevel} took average of {totalMs / 10.0f}ms ({minMs}-{maxMs})", Color.Chocolate);
        //    }
        //}

        //private static void DoBlockLineTest()
        //{
        //    GameWorld gameWorld = new GameWorld(200, 200, 200);
        //    Block block = new Block("dummy");
        //    block.AddComponent(new LightPassthroughComponent());
        //    for (int x = 0; x < 100; x++)
        //    {
        //        for (int z = 0; z < 100; z++)
        //        {
        //            for (int y = 0; y < 100; y++)
        //                gameWorld[x, y, z] = (x + y + z % 2 == 0) ? block : Block.Empty;
        //        }
        //    }

        //    gameWorld.Initialize();
        //    int center = gameWorld.BlockSize.X / 2;

        //    long totalMs = 0;
        //    Stopwatch sw = new Stopwatch();
        //    for (int t = 0; t < 20; t++)
        //    {
        //        sw.Restart();
        //        for (int i = 0; i < 10000; i++)
        //            gameWorld.NoBlocksInLine(center, center, center, i % center + 98, i % center + 98, i % center + 98);
        //        sw.Stop();
        //        totalMs += sw.ElapsedMilliseconds;
        //    }
        //    Messages.Println($"10,000 block lines took average of {totalMs / 20.0f}ms", Color.Chocolate);
        //}
        #endregion
        
        #region Random number generator speed tests
        //private static void TestRandomSpeed()
        //{
        //    DotNetRandomSpeedTest();
        //    TiVERandomGeneratorSpeedTest();
        //}

        //private static void DotNetRandomSpeedTest()
        //{
        //    Stopwatch sw = Stopwatch.StartNew();
        //    Random random = new Random();
        //    int value = 184756152;
        //    for (int i = 0; i < 100000000; i++)
        //        value ^= random.Next();
        //    sw.Stop();

        //    Messages.Println(string.Format("100M .Net random numbers took {0}ms", sw.ElapsedTicks * 1000.0f / Stopwatch.Frequency), Color.Chocolate);
        //}

        //private static void TiVERandomGeneratorSpeedTest()
        //{
        //    Stopwatch sw = Stopwatch.StartNew();
        //    RandomGenerator random = new RandomGenerator();
        //    int value = 184756152;
        //    for (int i = 0; i < 100000000; i++)
        //        value ^= random.Next();
        //    sw.Stop();

        //    Messages.Println(string.Format("100M TiVE random numbers took {0}ms", sw.ElapsedTicks * 1000.0f / Stopwatch.Frequency), Color.Chocolate);
        //}
        #endregion
        
        #region Struct with int vs. int speed tests
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
        
        #region FastSqrt vs. Math.Sqrt speed tests
        //private static void DoSqrtSpeedTests()
        //{
        //    DoFastSqrtTest();
        //    DoMathSqrtTest();
        //}

        //private static void DoFastSqrtTest()
        //{
        //    float[] values = new float[2000000];
        //    RandomGenerator random = new RandomGenerator();
        //    for (int i = 0; i < values.Length; i++)
        //        values[i] = random.NextFloat();

        //    long totalMs = 0;
        //    Stopwatch sw = new Stopwatch();
        //    int minMs = int.MaxValue;
        //    int maxMs = 0;
        //    for (int t = 0; t < 20; t++)
        //    {
        //        float value = 0.0f;
        //        sw.Restart();
        //        for (int i = 0; i < values.Length; i++)
        //            value = MathUtils.FastSqrt(values[i]);
        //        sw.Stop();
        //        Console.WriteLine(value + " - " + Math.Sqrt(values[values.Length - 1]));
        //        totalMs += sw.ElapsedMilliseconds;
        //        if (sw.ElapsedMilliseconds < minMs)
        //            minMs = (int)sw.ElapsedMilliseconds;
        //        if (sw.ElapsedMilliseconds > maxMs)
        //            maxMs = (int)sw.ElapsedMilliseconds;
        //    }
        //    Messages.Println($"{values.Length} FastSqrt took average of {totalMs / 20.0f}ms ({minMs}-{maxMs})", Color.Chocolate);
        //}

        //private static void DoMathSqrtTest()
        //{
        //    float[] values = new float[2000000];
        //    RandomGenerator random = new RandomGenerator();
        //    for (int i = 0; i < values.Length; i++)
        //        values[i] = random.NextFloat();

        //    long totalMs = 0;
        //    Stopwatch sw = new Stopwatch();
        //    int minMs = int.MaxValue;
        //    int maxMs = 0;
        //    for (int t = 0; t < 20; t++)
        //    {
        //        float value = 0.0f;
        //        sw.Restart();
        //        for (int i = 0; i < values.Length; i++)
        //            value = (float)Math.Sqrt(values[i]);
        //        sw.Stop();
        //        Console.WriteLine(value);
        //        totalMs += sw.ElapsedMilliseconds;
        //        if (sw.ElapsedMilliseconds < minMs)
        //            minMs = (int)sw.ElapsedMilliseconds;
        //        if (sw.ElapsedMilliseconds > maxMs)
        //            maxMs = (int)sw.ElapsedMilliseconds;
        //    }
        //    Messages.Println($"{values.Length} MathSqrt took average of {totalMs / 20.0f}ms ({minMs}-{maxMs})", Color.Chocolate);
        //}
        #endregion
    }
}
