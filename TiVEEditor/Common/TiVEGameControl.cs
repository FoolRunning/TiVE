using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using ProdigalSoftware.TiVE;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVE.RenderSystem.Lighting;
using ProdigalSoftware.TiVE.RenderSystem.World;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;

namespace ProdigalSoftware.TiVEEditor.Common
{
    internal class TiVEGameControl : GLControl
    {
        private readonly Timer timer = new Timer();
        private readonly Engine engine = new Engine(30);
        private readonly Scene scene = new Scene();
        private readonly bool reallyDesignMode;

        public TiVEGameControl() : base(new GraphicsMode(32, 16, 0, 4), 3, 1, GraphicsContextFlags.ForwardCompatible)
        {
            reallyDesignMode = DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

            if (!reallyDesignMode)
            {
                TiVEController.UserSettings.Load();
                timer.Interval = 100;
                timer.Tick += timer_Tick;
                timer.Start();
            }
        }

        public Scene Scene => 
            scene;

        public GameWorld GameWorld => 
            scene.GameWorldInternal;

        public LightProvider LightProvider => 
            scene.WorldLightProvider;

        public CameraComponent Camera
        {
            get 
            {
                IEntity entity = scene.GetEntitiesWithComponent<CameraComponent>().FirstOrDefault();
                return entity?.GetComponent<CameraComponent>();
            }
        }

        public void SetGameWorld(GameWorld gameWorld)
        {
            scene.SetGameWorld(gameWorld);
        }

        public void RefreshLevel(bool refreshStaticLighting)
        {
            Console.WriteLine("Need to implement level refresh!");
            Debug.Fail("implement me!");
            //renderer.GameWorld.Initialize(renderer.BlockList);
            //if (refreshStaticLighting)
            //    renderer.WorldLightProvider.Calculate(renderer.BlockList, true);
            //renderer.RefreshLevel();
        }

        #region Overrides of GLControl
        protected override void Dispose(bool disposing)
        {
            timer.Dispose();
            base.Dispose(disposing);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!reallyDesignMode)
            {
                MakeCurrent();
                TiVEController.Backend.Initialize();
                engine.InitializeSystems();
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (!reallyDesignMode)
            {
                MakeCurrent();
                engine.DeleteCurrentScene();
                engine.DisposeSystems();
            }
            base.OnHandleDestroyed(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (!reallyDesignMode)
            {
                MakeCurrent();
                TiVEController.Backend.BeforeRenderFrame();
                
                engine.UpdateSystems(1);
                SwapBuffers();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!reallyDesignMode)
            {
                Camera.AspectRatio = ClientRectangle.Width / (float)ClientRectangle.Height;

                MakeCurrent();
                TiVEController.Backend.WindowResized(ClientRectangle);
            }
        }
        #endregion

        void timer_Tick(object sender, EventArgs e)
        {
            Invalidate();
        }
    }
}
