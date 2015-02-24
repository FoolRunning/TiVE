using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE.Core
{
    internal sealed class Engine
    {
        private const int UpdateFPS = 60;
        private readonly List<EngineSystem> systems = new List<EngineSystem>();
        private bool continueMainLoop = true;

        public void AddSystem(EngineSystem system)
        {
            systems.Add(system);
        }

        public void MainLoop()
        {
            INativeDisplay nativeDisplay = InitializeWindow();
            nativeDisplay.DisplayClosing += (s, e) => continueMainLoop = false;

            for (int i = 0; i < systems.Count; i++)
            {
                try
                {
                    systems[i].Initialize();
                }
                catch (Exception e)
                {
                    Messages.AddError("Unable to initialize " + systems[i].DebuggingName + ":");
                    Messages.AddStackTrace(e);
                    continueMainLoop = false;
                }
            }

            long ticksPerUpdate = Stopwatch.Frequency / UpdateFPS;
            float timeDelta = ticksPerUpdate / (float)Stopwatch.Frequency;
            long previousTime = Stopwatch.GetTimestamp();
            long ticksSinceLastUpdate = 0;

            while (continueMainLoop)
            {
                long currentTime = Stopwatch.GetTimestamp();
                ticksSinceLastUpdate += (previousTime - currentTime);
                previousTime = currentTime;

                nativeDisplay.ProcessNativeEvents(); 
                
                while (ticksSinceLastUpdate >= ticksPerUpdate)
                {
                    UpdateSystems(timeDelta);
                    ticksSinceLastUpdate -= ticksPerUpdate;
                }

                nativeDisplay.UpdateDisplayContents();
            }

            nativeDisplay.CloseWindow();
            nativeDisplay.Dispose();
        }

        public void UpdateSystems(float timeDelta)
        {
            for (int i = 0; i < systems.Count; i++)
            {
                try
                {
                    systems[i].Update(timeDelta);
                }
                catch (Exception e)
                {
                    Messages.AddError("Exception when updating " + systems[i].DebuggingName + ":");
                    Messages.AddStackTrace(e);
                    continueMainLoop = false;
                }
            }
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
