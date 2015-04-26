using ProdigalSoftware.TiVE.Core;
using Squid;

namespace ProdigalSoftware.TiVE.GUISystem
{
    internal class GUISystem : EngineSystem
    {
        private Scene lastDrawnScene;
        private Desktop desktop;

        public GUISystem() : base("GUI")
        {
        }

        #region Implementation of EngineSystem
        public override void Dispose()
        {
            Gui.Renderer.Dispose();
            Gui.Renderer = null;
        }

        public override bool Initialize()
        {
            Gui.Renderer = new SquidRenderer();
            return true;
        }

        protected override bool UpdateInternal(int ticksSinceLastUpdate, float timeBlendFactor, Scene currentScene)
        {
            if (currentScene != lastDrawnScene)
            {
                desktop = new Desktop();
                Label label = new Label();
                label.Text = "Loading...";
                label.Position = new Point(100, 100);
                desktop.Controls.Add(label);
            }

            desktop.Update();
            desktop.Draw();

            lastDrawnScene = currentScene;
            return true;
        }
        #endregion
    }
}
