using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using ProdigalSoftware.TiVE;
using ProdigalSoftware.TiVE.Renderer;
using ProdigalSoftware.TiVE.Renderer.Lighting;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEEditor.Common
{
    internal class TiVEGameControl : GLControl
    {
        private readonly Timer timer = new Timer();
        private readonly Camera camera = new Camera();
        private readonly WorldChunkRenderer renderer = new WorldChunkRenderer(1);
        private readonly bool reallyDesignMode;

        public TiVEGameControl() : base(new GraphicsMode(32, 16, 0, 4), 3, 1, GraphicsContextFlags.ForwardCompatible)
        {
            reallyDesignMode = DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

            if (!reallyDesignMode)
            {
                timer.Interval = 100;
                timer.Tick += timer_Tick;
                timer.Start();
            }
        }

        public GameWorld GameWorld
        {
            get { return renderer.GameWorld; }
        }

        public BlockList BlockList
        {
            get { return renderer.BlockList; }
        }

        public Camera Camera
        {
            get { return camera; }
        }

        public void SetGameWorld(BlockList blockList, GameWorld gameWorld)
        {
            renderer.SetGameWorld(blockList, gameWorld);
        }

        public void RefreshLevel(bool refreshStaticLighting)
        {
            if (refreshStaticLighting)
            {
                GameWorld gameWorld = GameWorld;
                for (int z = 0; z < gameWorld.BlockSize.Z; z++)
                {
                    for (int x = 0; x < gameWorld.BlockSize.X; x++)
                    {
                        for (int y = 0; y < gameWorld.BlockSize.Y; y++)
                        {
                            gameWorld.GetLights(x, y, z).Clear();
                            gameWorld.SetBlockLight(x, y, z, new Color3f());
                        }
                    }
                }

                StaticLightingHelper lightingHelper = new StaticLightingHelper(renderer.GameWorld, 50, 0.002f);
                lightingHelper.Calculate();
            }
            renderer.RefreshLevel();
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
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (!reallyDesignMode)
            {
                MakeCurrent();
                renderer.Dispose();
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

                camera.Update();

                renderer.Update(camera, 0.0f);
                renderer.Draw(camera);
                SwapBuffers();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!reallyDesignMode)
            {
                camera.AspectRatio = ClientRectangle.Width / (float)ClientRectangle.Height;

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
