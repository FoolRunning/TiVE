using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework.Generators;

namespace ProdigalSoftware.TiVE.Core
{
    internal sealed class Engine
    {
        private readonly int maxTicksPerUpdate = (int)(Stopwatch.Frequency / 2);

        private readonly List<EngineSystemBase> systems = new List<EngineSystemBase>();
        private readonly float timePerUpdate;
        private readonly int ticksPerUpdate;
        private readonly object syncLock = new object();
        private int ticksSinceLastUpdate;

        private Thread sceneLoadThread;
        private bool continueMainLoop = true;
        private Scene currentScene;
        private string currentSceneName;

        /// <summary>
        /// Creates a new Engine that updates at the specified rate
        /// </summary>
        public Engine(int updatesPerSecond)
        {
            if (updatesPerSecond <= 0)
                throw new ArgumentException("updatesPerSecond must be greater then zero");

            ticksPerUpdate = (int)(Stopwatch.Frequency / updatesPerSecond);
            timePerUpdate = 1.0f / updatesPerSecond;
        }

        public Rectangle WindowClientBounds { get; private set; }

        public void AddSystem(EngineSystemBase system)
        {
            systems.Add(system);
        }

        public void DeleteCurrentScene()
        {
            lock (syncLock)
            {
                if (currentScene != null)
                    currentScene.Dispose();
                currentScene = null;
            }
        }

        public void MainLoop(string sceneToLoad)
        {
            INativeDisplay nativeDisplay = null;
            try
            {
                nativeDisplay = InitializeWindow();
                nativeDisplay.DisplayClosing += (s, e) => continueMainLoop = false;

                TiVEController.Backend.Initialize();
                NativeDisplayResized(nativeDisplay.ClientBounds); // Make sure we start out at the correct size

                continueMainLoop = InitializeSystems();
                if (continueMainLoop)
                {
                    LoadScene("Loading", false);
                    LoadScene(sceneToLoad, true);
                }

                long previousTime = Stopwatch.GetTimestamp();
                while (continueMainLoop)
                {
                    long currentTime = Stopwatch.GetTimestamp();
                    int ticksSinceLastFrame = (int)(currentTime - previousTime);
                    if (ticksSinceLastFrame > maxTicksPerUpdate)
                        ticksSinceLastFrame = maxTicksPerUpdate;

                    previousTime = currentTime;

                    nativeDisplay.ProcessNativeEvents();
                    TiVEController.Backend.BeforeRenderFrame();
                    UpdateSystems(ticksSinceLastFrame);
                    nativeDisplay.UpdateDisplayContents();
                }
            }
            catch (Exception e)
            {
                Messages.AddError("Exception while running main loop:");
                Messages.AddStackTrace(e);
            }
            finally
            {
                if (sceneLoadThread != null)
                {
                    // Something bad probably happened while loading the next scene.
                    sceneLoadThread.Abort();
                    sceneLoadThread.Join();
                }

                DisposeSystems();

                if (nativeDisplay != null)
                {
                    nativeDisplay.CloseWindow();
                    nativeDisplay.Dispose();
                }
            }
        }

        public bool InitializeSystems()
        {
            for (int i = 0; i < systems.Count; i++)
            {
                try
                {
                    if (!systems[i].Initialize())
                        return false;
                }
                catch (Exception e)
                {
                    Messages.AddError("Unable to initialize " + systems[i].DebuggingName + ":");
                    Messages.AddStackTrace(e);
                    return false;
                }
            }
            return true;
        }

        public void UpdateSystems(int ticksSinceLastFrame)
        {
            ticksSinceLastUpdate += ticksSinceLastFrame;
            int slicedUpdateCount = ticksSinceLastUpdate / ticksPerUpdate;
            ticksSinceLastUpdate -= slicedUpdateCount * ticksPerUpdate;

            float timeBlendFactor = ticksSinceLastUpdate / (float)ticksPerUpdate;
            lock (syncLock) // Don't let the scene change while updating
            {
                for (int sys = 0; sys < systems.Count; sys++)
                {
                    systems[sys].UpdateTiming(ticksSinceLastFrame);

                    try
                    {
                        TimeSlicedEngineSystem slicedSystem = systems[sys] as TimeSlicedEngineSystem;
                        bool keepRunning = true;
                        if (slicedSystem == null)
                            keepRunning = ((EngineSystem)systems[sys]).Update(ticksSinceLastFrame, timeBlendFactor, currentScene);
                        else
                        {
                            for (int i = 0; i < slicedUpdateCount; i++)
                                keepRunning = slicedSystem.Update(timePerUpdate, currentScene);
                        }
                        if (!keepRunning)
                            continueMainLoop = false;
                    }
                    catch (Exception e)
                    {
                        Messages.AddError("Exception when updating " + systems[sys].DebuggingName + ":");
                        Messages.AddStackTrace(e);
                        continueMainLoop = false;
                        break;
                    }
                }
            }
        }

        public void DisposeSystems()
        {
            for (int i = 0; i < systems.Count; i++)
            {
                try
                {
                    systems[i].Dispose();
                }
                catch (Exception e)
                {
                    Messages.AddError("Exception when disposing " + systems[i].DebuggingName + ":");
                    Messages.AddStackTrace(e);
                }
            }

            DeleteCurrentScene(); // Must be done after disposing systems

            GC.Collect();
            Messages.Println("Done cleaning up");
        }

        private void LoadScene(string sceneName, bool useSeparateThread)
        {
            currentSceneName = sceneName;
            ThreadStart loadSceneAction = () =>
            {
                foreach (ISceneGenerator generator in TiVEController.PluginManager.GetPluginsOfType<ISceneGenerator>())
                {
                    Scene scene = (Scene)generator.CreateScene(sceneName);
                    if (scene == null)
                        Messages.AddWarning("Failed to find scene: " + sceneName);
                    else
                    {
                        SetScene(scene);
                        break;
                    }
                }
                sceneLoadThread = null;
            };

            if (!useSeparateThread)
                loadSceneAction();
            else
            {
                sceneLoadThread = new Thread(loadSceneAction);
                sceneLoadThread.Name = "Scene Load";
                sceneLoadThread.IsBackground = true;
                sceneLoadThread.Start();
            }
        }

        private void SetScene(Scene newScene)
        {
            foreach (EngineSystemBase system in systems)
                system.PrepareForScene(currentSceneName);

            lock (syncLock)
            {
                Scene previousScene = currentScene;

                foreach (EngineSystemBase system in systems)
                    system.ChangeScene(previousScene, newScene);

                if (previousScene != null)
                    previousScene.Dispose();
                currentScene = newScene;
            }
            Messages.AddDebug("Running scene: " + currentSceneName);
        }

        private INativeDisplay InitializeWindow()
        {
            FullScreenMode fullScreenMode = (FullScreenMode)(int)TiVEController.UserSettings.Get(UserSettings.FullScreenModeKey);
            bool useVsync = TiVEController.UserSettings.Get(UserSettings.EnableVSyncKey);
            int antiAliasAmount = TiVEController.UserSettings.Get(UserSettings.AntiAliasAmountKey);
            ResolutionSetting displaySetting = (ResolutionSetting)TiVEController.UserSettings.Get(UserSettings.DisplayResolutionKey);

            INativeDisplay nativeDisplay = TiVEController.Backend.CreateNatveDisplay(displaySetting, fullScreenMode, antiAliasAmount, useVsync);
            nativeDisplay.Icon = Properties.Resources.P_button;

            nativeDisplay.DisplayResized += NativeDisplayResized;
            nativeDisplay.DisplayClosing += (s, e) => continueMainLoop = false;

            return nativeDisplay;
        }

        private void NativeDisplayResized(Rectangle newClientBounds)
        {
            WindowClientBounds = newClientBounds;
            TiVEController.Backend.WindowResized(newClientBounds);
        }
    }
}
