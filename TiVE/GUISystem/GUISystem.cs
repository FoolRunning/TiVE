﻿using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVEPluginFramework;
using Squid;

namespace ProdigalSoftware.TiVE.GUISystem
{
    internal class GUISystem : EngineSystem
    {
        private Desktop desktop;

        public GUISystem() : base("GUI")
        {
        }

        #region Implementation of EngineSystem
        public override void Dispose()
        {
            Gui.Renderer.Dispose();
            Gui.Renderer = null;
            desktop = null;
        }

        public override bool Initialize()
        {
            Gui.Renderer = new SquidRenderer();
            return true;
        }

        public override void ChangeScene(Scene oldScene, Scene newScene)
        {
            desktop = new Desktop();
            desktop.Size = new Point(300, 300);
            Label label = new Label();
            label.TextColor = (int)new Color4b(255, 255, 255, 255).ToArgb();
            label.Text = "Loading...";
            label.Position = new Point(100, 100);
            desktop.Controls.Add(label);
        }

        protected override bool UpdateInternal(int ticksSinceLastUpdate, Scene currentScene)
        {
            //desktop.Size = new Point(100, 100);
            desktop.Update();
            desktop.Draw();

            return true;
        }
        #endregion
    }
}
