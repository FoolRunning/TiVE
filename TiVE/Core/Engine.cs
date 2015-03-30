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
        private readonly List<EngineSystem> systems = new List<EngineSystem>();
        private bool continueMainLoop = true;
        private Scene currentScene;
        private readonly object syncLock = new object();

        public void AddSystem(EngineSystem system)
        {
            systems.Add(system);
        }

        public void LoadScene(string sceneName, bool useSeparateThread)
        {
            ThreadStart loadSceneAction = () =>
            {
                foreach (ISceneGenerator generator in TiVEController.PluginManager.GetPluginsOfType<ISceneGenerator>())
                {
                    Scene scene = (Scene)generator.CreateScene(sceneName);
                    if (scene != null)
                    {
                        SetScene(scene);
                        break;
                    }
                }
            };

            if (!useSeparateThread)
                loadSceneAction();
            else
            {
                Thread sceneLoadThread = new Thread(loadSceneAction);
                sceneLoadThread.Name = "Scene Load";
                sceneLoadThread.IsBackground = true;
                sceneLoadThread.Start();
            }
        }

        public void SetScene(Scene newScene)
        {
            lock (syncLock)
            {
                DeleteCurrentScene();
                currentScene = newScene;
            }
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
            INativeDisplay nativeDisplay = InitializeWindow();
            nativeDisplay.DisplayClosing += (s, e) => continueMainLoop = false;

            continueMainLoop = InitializeSystems();

            LoadScene("Loading", false);
            LoadScene(sceneToLoad, true);

            long previousTime = Stopwatch.GetTimestamp();
            while (continueMainLoop)
            {
                long currentTime = Stopwatch.GetTimestamp();
                int ticksSinceLastFrame = (int)(previousTime - currentTime);
                previousTime = currentTime;

                nativeDisplay.ProcessNativeEvents();
                UpdateSystems(ticksSinceLastFrame);
                nativeDisplay.UpdateDisplayContents();
            }

            DisposeSystems();

            nativeDisplay.CloseWindow();
            nativeDisplay.Dispose();
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
            lock (syncLock) // Don't let the scene change while updating
            {
                for (int i = 0; i < systems.Count; i++)
                {
                    try
                    {
                        systems[i].Update(ticksSinceLastFrame, currentScene);
                    }
                    catch (Exception e)
                    {
                        Messages.AddError("Exception when updating " + systems[i].DebuggingName + ":");
                        Messages.AddStackTrace(e);
                        continueMainLoop = false;
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
                    Messages.AddError("Exception when deleting " + systems[i].DebuggingName + ":");
                    Messages.AddStackTrace(e);
                }
            }

            GC.Collect();
            Messages.Println("Done cleaning up");
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
            TiVEController.Backend.WindowResized(newClientBounds);
            //renderer.Camera.AspectRatio = newClientBounds.Width / (float)newClientBounds.Height;
        }
    }
}
