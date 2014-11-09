using System;
using System.Diagnostics;
using Microsoft.CSharp.RuntimeBinder;
using NLua.Exceptions;
using ProdigalSoftware.TiVE.Renderer;
using ProdigalSoftware.TiVE.Renderer.Lighting;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVE.Scripts;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE
{
    internal sealed class GameLogic : IDisposable
    {
        private const int GameUpdatesPerSecond = 60;
        private const int DisplayUpdatesPerSecond = 60;

        private readonly IGameWorldRenderer renderer = new WorldChunkRenderer();
        private readonly Camera camera = new Camera();

        private readonly TimeStatHelper renderTime = new TimeStatHelper(2, true);
        private readonly TimeStatHelper updateTime = new TimeStatHelper(2, true);
        private readonly TimeStatHelper frameTime = new TimeStatHelper(2, true);

        private readonly CountStatHelper drawCount = new CountStatHelper(4, false);
        private readonly CountStatHelper voxelCount = new CountStatHelper(8, false);
        private readonly CountStatHelper polygonCount = new CountStatHelper(8, false);
        private readonly CountStatHelper renderedVoxelCount = new CountStatHelper(8, false);

        private volatile bool continueMainLoop;
        private IKeyboard keyboard;
        private double lastPrintTime; 
        private dynamic gameScript;

        public void Dispose()
        {
            ResourceManager.Cleanup();
        }

        public bool Initialize()
        {
            if (!ResourceManager.Initialize())
            {
                ResourceManager.Cleanup();
                return false;
            }

            gameScript = TiVEController.LuaScripts.GetScript("Game");
            if (gameScript == null)
            {
                ResourceManager.Cleanup();
                Messages.AddError("Failed to find Game script");
                return false;
            }

            LuaScripts.AddLuaTableForEnum<Keys>(gameScript);

            gameScript.KeyPressed = new Func<Keys, bool>(k => keyboard.IsKeyPressed(k));
            gameScript.Vector = new Func<float, float, float, OpenTK.Vector3>((x, y, z) => new OpenTK.Vector3(x, y, z));
            gameScript.Color = new Func<float, float, float, Color3f>((r, g, b) => new Color3f(r, g, b));
            gameScript.GameWorld = new Func<GameWorld>(() => ResourceManager.GameWorldManager.GameWorld);
            gameScript.ReloadLevel = new Action(() => ResourceManager.ChunkManager.ReloadAllChunks());

            gameScript.CreateWorld = new Func<int, int, int, IGameWorld>((xSize, ySize, zSize) =>
            {
                if (!ResourceManager.GameWorldManager.CreateWorld(xSize, ySize, zSize, LongRandom() /*123456789123456789*/))
                    throw new TiVEException("Failed to load resources");
                return ResourceManager.GameWorldManager.GameWorld;
            });

            try
            {
                gameScript.Initialize(camera);
            }
            catch (RuntimeBinderException)
            {
                ResourceManager.Cleanup();
                Messages.AddError("Can not find Initialize(camera) function in Game script");
                return false;
            }
            catch (LuaScriptException e)
            {
                ResourceManager.Cleanup();
                Messages.AddStackTrace(e);
                return false;
            }

            // Calculate static lighting
            const float minLightValue = 0.002f; // 0.002f (0.2%) produces the best result as that is less then a single light value's worth
            StaticLightingHelper lightingHelper = new StaticLightingHelper(ResourceManager.GameWorldManager.GameWorld, 50, minLightValue);
            lightingHelper.Calculate();

            return true;
        }

        public void RunMainLoop()
        {
            INativeWindow nativeWindow = TiVEController.Backend.CreateNatveWindow(1280, 720, false, false);
            nativeWindow.WindowResized += newSize =>
                {
                    TiVEController.Backend.WindowResized(newSize);
                    camera.AspectRatio = newSize.Width / (float)newSize.Height;
                };
            nativeWindow.WindowClosing += (s, e) => continueMainLoop = false;
            keyboard = nativeWindow.KeyboardImplementation;

            TiVEController.Backend.Initialize();

            long ticksPerGameUpdate = Stopwatch.Frequency / GameUpdatesPerSecond;
            long ticksPerDisplayUpdate = Stopwatch.Frequency / DisplayUpdatesPerSecond;

            continueMainLoop = true;
            long previousGameUpdateTime = 0;
            long previousDisplayUpdateTime = 0;
            while (continueMainLoop)
            {
                nativeWindow.ProcessNativeEvents();

                long currentTime = Stopwatch.GetTimestamp();
                if (previousGameUpdateTime + ticksPerGameUpdate <= currentTime)
                {
                    float timeSinceLastFrame = (currentTime - previousGameUpdateTime) / (float)ticksPerDisplayUpdate;
                    previousGameUpdateTime += ticksPerGameUpdate;
                    if (!UpdateFrame(timeSinceLastFrame))
                        break;

                    lastPrintTime += timeSinceLastFrame;
                    if (lastPrintTime > 1)
                    {
                        lastPrintTime -= 1;
                        updateTime.UpdateDisplayedTime();
                        renderTime.UpdateDisplayedTime();
                        frameTime.UpdateDisplayedTime();

                        drawCount.UpdateDisplayedTime();
                        voxelCount.UpdateDisplayedTime();
                        renderedVoxelCount.UpdateDisplayedTime();
                        polygonCount.UpdateDisplayedTime();

                        nativeWindow.WindowTitle = string.Format("TiVE   Frame={6}   Update={5}   Render={4}   Voxels={0}  Rendered={1}  Polys={2}  Draws={3}",
                            voxelCount.DisplayedValue, renderedVoxelCount.DisplayedValue, polygonCount.DisplayedValue, drawCount.DisplayedValue,
                            renderTime.DisplayedValue, updateTime.DisplayedValue, frameTime.DisplayedValue);
                    }
                }
                if (previousDisplayUpdateTime + ticksPerGameUpdate <= currentTime)
                {
                    previousDisplayUpdateTime += ticksPerGameUpdate;
                    RenderFrame();
                    nativeWindow.UpdateDisplayContents();
                }
            }

            keyboard = null;
            ResourceManager.Cleanup();
            nativeWindow.CloseWindow();
            nativeWindow.Dispose();
        }

        private bool UpdateFrame(float timeSinceLastFrame)
        {
            if (keyboard.IsKeyPressed(Keys.Escape))
                return false;

            updateTime.MarkStartTime();
            try
            {
                gameScript.Update(camera);
            }
            catch (RuntimeBinderException)
            {
                Messages.AddError("Can not find Update(camera) function in Game script");
                return false;
            }
            catch (LuaScriptException e)
            {
                Messages.AddStackTrace(e);
                return false;
            }

            camera.Update();
            renderer.Update(camera, timeSinceLastFrame);

            updateTime.PushTime();
            return true;
        }

        private void RenderFrame()
        {
            frameTime.PushTime();
            frameTime.MarkStartTime();
            //frameTime.AddData((float)e.Time * 1000f);

            renderTime.MarkStartTime();

            TiVEController.Backend.BeforeRenderFrame();

            RenderStatistics stats = renderer.Draw(camera);

            drawCount.PushCount(stats.DrawCount);
            voxelCount.PushCount(stats.VoxelCount);
            polygonCount.PushCount(stats.PolygonCount);
            renderedVoxelCount.PushCount(stats.RenderedVoxelCount);

            renderTime.PushTime();
        }

        private static long LongRandom()
        {
            byte[] buf = new byte[8];
            Random random = new Random();
            random.NextBytes(buf);
            return BitConverter.ToInt64(buf, 0);
        }

        #region TimeStatHelper class
        private sealed class TimeStatHelper
        {
            private readonly string formatString;
            private long startTicks;
            private float minTime = float.MaxValue;
            private float maxTime;
            private float totalTime;
            private int dataCount;

            public TimeStatHelper(int digitsAfterDecimal, bool showMinMax)
            {
                if (showMinMax)
                    formatString = "{0:F" + digitsAfterDecimal + "}({1:F" + digitsAfterDecimal + "}-{2:F" + digitsAfterDecimal + "})";
                else
                    formatString = "{0:F" + digitsAfterDecimal + "}";
            }

            public string DisplayedValue { get; private set; }

            /// <summary>
            /// Updates the display time with the average of the data points
            /// </summary>
            public void UpdateDisplayedTime()
            {
                DisplayedValue = string.Format(formatString, totalTime / Math.Max(dataCount, 1), minTime, maxTime);
                totalTime = 0;
                dataCount = 0;
                minTime = float.MaxValue;
                maxTime = 0;
            }

            public void MarkStartTime()
            {
                startTicks = Stopwatch.GetTimestamp();
            }

            public void PushTime()
            {
                long endTime = Stopwatch.GetTimestamp();
                float newTime = (endTime - startTicks) * 1000.0f / Stopwatch.Frequency;
                totalTime += newTime;
                dataCount++;

                if (newTime < minTime)
                    minTime = newTime;

                if (newTime > maxTime)
                    maxTime = newTime;
            }
        }
        #endregion

        #region CountStatHelper class
        private sealed class CountStatHelper
        {
            private readonly string formatString;
            private int totalCount;
            private int minCount = int.MaxValue;
            private int maxCount;
            private int dataCount;

            public CountStatHelper(int maxDigits, bool showMinMax)
            {
                if (showMinMax)
                    formatString = "{0:D" + maxDigits + "}({1:D" + maxDigits + "}-{2:D" + maxDigits + "})";
                else
                    formatString = "{0:D" + maxDigits + "}";
            }

            public string DisplayedValue { get; private set; }

            /// <summary>
            /// Updates the display time with the average of the data points
            /// </summary>
            public void UpdateDisplayedTime()
            {
                DisplayedValue = string.Format(formatString, totalCount / Math.Max(dataCount, 1), minCount, maxCount);
                totalCount = 0;
                dataCount = 0;
                minCount = int.MaxValue;
                maxCount = 0;
            }

            /// <summary>
            /// Adds the specified value as a new data point
            /// </summary>
            public void PushCount(int newCount)
            {
                totalCount += newCount;
                dataCount++;

                if (newCount < minCount)
                    minCount = newCount;

                if (newCount > maxCount)
                    maxCount = newCount;
            }
        }
        #endregion
    }
}
