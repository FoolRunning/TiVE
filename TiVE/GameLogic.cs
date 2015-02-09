﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using Microsoft.CSharp.RuntimeBinder;
using NLua.Exceptions;
using ProdigalSoftware.TiVE.Renderer;
using ProdigalSoftware.TiVE.Renderer.Lighting;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVE.Scripts;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE
{
    internal sealed class GameLogic
    {
        private const int UpdatesPerSecond = 60;

        private readonly TimeStatHelper renderTime = new TimeStatHelper(2, true);
        private readonly TimeStatHelper updateTime = new TimeStatHelper(2, true);
        private readonly TimeStatHelper frameTime = new TimeStatHelper(2, true);

        private readonly CountStatHelper drawCount = new CountStatHelper(4, false);
        private readonly CountStatHelper voxelCount = new CountStatHelper(8, false);
        private readonly CountStatHelper polygonCount = new CountStatHelper(8, false);
        private readonly CountStatHelper renderedVoxelCount = new CountStatHelper(8, false);

        private volatile bool continueMainLoop;
        private IKeyboard keyboard;
        private IGameWorldRenderer renderer;
        private dynamic gameScript;

        public bool Initialize()
        {
            // Creation of the renderer must be done before the call to Initialize on the script
            int numChunkCreationThreads = TiVEController.UserSettings.Get(UserSettings.ChunkCreationThreadsKey);
            renderer = new WorldChunkRenderer(numChunkCreationThreads);

            gameScript = TiVEController.LuaScripts.GetScript("Game");
            if (gameScript == null)
            {
                Messages.AddError("Failed to find Game script");
                return false;
            }

            LuaScripts.AddLuaTableForEnum<Keys>(gameScript);
            
            gameScript.KeyPressed = new Func<Keys, bool>(k => keyboard.IsKeyPressed(k));
            gameScript.Vector = new Func<float, float, float, OpenTK.Vector3>((x, y, z) => new OpenTK.Vector3(x, y, z));
            gameScript.Color = new Func<float, float, float, Color3f>((r, g, b) => new Color3f(r, g, b));
            gameScript.Renderer = new Func<IGameWorldRenderer>(() => renderer);
            gameScript.Camera = new Func<Camera>(() => renderer.Camera);
            gameScript.GameWorld = new Func<GameWorld>(() => renderer.GameWorld);
            gameScript.ReloadLevel = new Action(() => renderer.RefreshLevel());
            gameScript.EmptyBlock = BlockInformation.Empty;
            gameScript.BlockAt = new Func<int, int, int, BlockInformation>((blockX, blockY, blockZ) =>
            {
                GameWorld gameWorld = renderer.GameWorld;
                if (blockX < 0 || blockX >= gameWorld.BlockSize.X || blockY < 0 || blockY >= gameWorld.BlockSize.Y || blockZ < 0 || blockZ >= gameWorld.BlockSize.Z)
                    return BlockInformation.Empty;

                return gameWorld[blockX, blockY, blockZ];
            });

            gameScript.VoxelAt = new Func<int, int, int, uint>((voxelX, voxelY, voxelZ) =>
            {
                GameWorld gameWorld = renderer.GameWorld;
                if (voxelX < 0 || voxelX >= gameWorld.VoxelSize.X || voxelY < 0 || voxelY >= gameWorld.VoxelSize.Y || voxelZ < 0 || voxelZ >= gameWorld.VoxelSize.Z)
                    return 0;

                return gameWorld.GetVoxel(voxelX, voxelY, voxelZ);
            });

            gameScript.LoadWorld = new Func<string, GameWorld>(worldName =>
            {
                BlockList blockList;
                GameWorld newWorld = GameWorldManager.LoadGameWorld(worldName, out blockList);
                if (newWorld == null)
                    throw new TiVEException("Failed to create game world");
                renderer.SetGameWorld(blockList, newWorld);
                return newWorld;
            });

            try
            {
                gameScript.Initialize(renderer.Camera);
            }
            catch (RuntimeBinderException)
            {
                Messages.AddError("Can not find Initialize(camera) function in Game script");
                return false;
            }
            catch (LuaScriptException e)
            {
                Messages.AddStackTrace(e);
                return false;
            }

            // Calculate static lighting
            renderer.LightProvider.Calculate(CalcOptions.CalculateLights);

            // This seems to be needed for the GC to realize that the light information and the game world are long-lived
            // to keep it from causing pauses shortly after starting the render loop.
            for (int i = 0; i < 3; i++)
                GC.Collect();

            return true;
        }

        public void RunMainLoop()
        {
            FullScreenMode fullScreenMode = (FullScreenMode)(int)TiVEController.UserSettings.Get(UserSettings.FullScreenModeKey);
            bool useVsync = TiVEController.UserSettings.Get(UserSettings.EnableVSyncKey);
            int antiAliasAmount = TiVEController.UserSettings.Get(UserSettings.AntiAliasAmountKey);

            // TODO: Add a way for these to be chosen by the user
            const int width = 1280;
            const int height = 720;

            INativeDisplay nativeDisplay = TiVEController.Backend.CreateNatveDisplay(width, height, fullScreenMode, antiAliasAmount, useVsync);
            nativeDisplay.Icon = Properties.Resources.P_button;

            nativeDisplay.DisplayResized += NativeDisplayResized;
            nativeDisplay.DisplayClosing += (s, e) => continueMainLoop = false;
            keyboard = TiVEController.Backend.Keyboard;

            TiVEController.Backend.Initialize();
            NativeDisplayResized(nativeDisplay.ClientBounds); // Make sure we start out at the correct size

            long ticksPerUpdate = Stopwatch.Frequency / UpdatesPerSecond;

            continueMainLoop = true;
            long previousDisplayUpdateTime = Stopwatch.GetTimestamp();
            long lastPrintTime = Stopwatch.GetTimestamp();

            while (continueMainLoop)
            {
                nativeDisplay.ProcessNativeEvents();

                long currentTime = Stopwatch.GetTimestamp();
                if (lastPrintTime + Stopwatch.Frequency <= currentTime)
                {
                    lastPrintTime = currentTime;
                    updateTime.UpdateDisplayedTime();
                    renderTime.UpdateDisplayedTime();
                    frameTime.UpdateDisplayedTime();

                    drawCount.UpdateDisplayedTime();
                    voxelCount.UpdateDisplayedTime();
                    renderedVoxelCount.UpdateDisplayedTime();
                    polygonCount.UpdateDisplayedTime();

                    nativeDisplay.WindowTitle = string.Format("TiVE   Frame={6}   Update={5}   Render={4}   Voxels={0}  Rendered={1}  Polys={2}  Draws={3}",
                        voxelCount.DisplayedValue, renderedVoxelCount.DisplayedValue, polygonCount.DisplayedValue, drawCount.DisplayedValue,
                        renderTime.DisplayedValue, updateTime.DisplayedValue, frameTime.DisplayedValue);
                }

                if (useVsync || previousDisplayUpdateTime + ticksPerUpdate <= currentTime)
                {
                    float timeSinceLastFrame = (currentTime - previousDisplayUpdateTime) / (float)Stopwatch.Frequency;
                    previousDisplayUpdateTime = currentTime;
                    nativeDisplay.UpdateDisplayContents();

                    UpdateGame(timeSinceLastFrame);
                    RenderFrame();
                }
                else if (previousDisplayUpdateTime + ticksPerUpdate - TiVEController.MaxTicksForSleep > currentTime)
                    Thread.Sleep(1);
            }

            Messages.Print("Cleaning up...");
            
            keyboard = null;
            renderer.Dispose();
            nativeDisplay.CloseWindow();
            nativeDisplay.Dispose();

            Messages.AddDoneText();
        }

        private void NativeDisplayResized(Rectangle newClientBounds)
        {
            TiVEController.Backend.WindowResized(newClientBounds);
            renderer.Camera.AspectRatio = newClientBounds.Width / (float)newClientBounds.Height;
        }

        private void RenderFrame()
        {
            frameTime.PushTime();
            frameTime.MarkStartTime();

            renderTime.MarkStartTime();

            TiVEController.Backend.BeforeRenderFrame();

            RenderStatistics stats = renderer.Draw();

            drawCount.PushCount(stats.DrawCount);
            voxelCount.PushCount(stats.VoxelCount);
            polygonCount.PushCount(stats.PolygonCount);
            renderedVoxelCount.PushCount(stats.RenderedVoxelCount);

            renderTime.PushTime();
        }
        
        private void UpdateGame(float timeSinceLastFrame)
        {
            keyboard.Update();

            if (keyboard.IsKeyPressed(Keys.Escape))
                continueMainLoop = false;

            updateTime.MarkStartTime();
            try
            {
                gameScript.Update(renderer.Camera);
            }
            catch (RuntimeBinderException)
            {
                Messages.AddError("Can not find Update(camera) function in Game script");
                continueMainLoop = false;
            }
            catch (LuaScriptException e)
            {
                Messages.AddStackTrace(e);
                continueMainLoop = false;
            }

            renderer.Update(timeSinceLastFrame);//*/ 1.0f / GameUpdatesPerSecond);

            updateTime.PushTime();
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

            private readonly object syncObj = new object();

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
                using (new PerformanceLock(syncObj))
                {
                    DisplayedValue = string.Format(formatString, totalTime / Math.Max(dataCount, 1), minTime, maxTime);
                    totalTime = 0;
                    dataCount = 0;
                    minTime = float.MaxValue;
                    maxTime = 0;
                }
            }

            public void MarkStartTime()
            {
                using (new PerformanceLock(syncObj))
                    startTicks = Stopwatch.GetTimestamp();
            }

            public void PushTime()
            {
                long endTime = Stopwatch.GetTimestamp();
                float newTime = (endTime - startTicks) * 1000.0f / Stopwatch.Frequency;
                using (new PerformanceLock(syncObj))
                {
                    totalTime += newTime;
                    dataCount++;

                    if (newTime < minTime)
                        minTime = newTime;

                    if (newTime > maxTime)
                        maxTime = newTime;
                }
            }
        }
        #endregion

        #region CountStatHelper class
        private sealed class CountStatHelper
        {
            private readonly string formatString;
            private long totalCount;
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
