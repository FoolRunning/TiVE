﻿using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using ProdigalSoftware.TiVE.Plugins;
using ProdigalSoftware.TiVE.Renderer;
using ProdigalSoftware.TiVE.Renderer.OpenGL;
using ProdigalSoftware.TiVE.Resources;
using ProdigalSoftware.TiVE.Scripts;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE
{
    public static class TiVEController
    {
        internal static readonly PluginManager PluginManager = new PluginManager();
        internal static readonly ResourceTableDefinitionManager TableDefinitions = new ResourceTableDefinitionManager();
        internal static readonly LuaScripts LuaScripts = new LuaScripts();
        internal static readonly IRendererBackend Backend = new OpenGLRendererBackend();

        private static StarterForm starterForm;

        public static void RunStarter()
        {
            Thread.CurrentThread.Name = "Main UI";
            starterForm = new StarterForm();
            starterForm.FormClosing += starterForm_FormClosing;
            starterForm.VisibleChanged += starterForm_VisibleChanged;

            Application.Run(starterForm);
        }

        static void starterForm_VisibleChanged(object sender, EventArgs e)
        {
            Thread initialLoadThread = new Thread(() =>
            {
                bool success = PluginManager.LoadPlugins();
                if (success)
                    success = TableDefinitions.Initialize();
                if (success)
                    success = LuaScripts.Initialize();

                if (success)
                    starterForm.EnableControls();
            });
            initialLoadThread.IsBackground = true;
            initialLoadThread.Name = "InitialLoad";
            initialLoadThread.Start();
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
                    Size size = new Size(1280, 720); // Screen.PrimaryScreen.Bounds.Size;
                    using (IDisplay display = Backend.CreateDisplay(size.Width, size.Height, false, false))
                        display.RunMainLoop(gameLogic);
                }));
            });
            loadingThread.IsBackground = false;
            loadingThread.Name = "Loading";
            loadingThread.Start();
        }

        private static void starterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            LuaScripts.Dispose();
            TableDefinitions.Dispose();
            PluginManager.Dispose();
        }
    }
}
