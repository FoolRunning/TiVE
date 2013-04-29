using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using ProdigalSoftware.TiVE.Renderer;
using ProdigalSoftware.TiVE.Renderer.World;

namespace ProjectM
{
    public class Game : GameWindow
    {
        public const int WORLD_X_SIZE = 1024;
        public const int WORLD_Y_SIZE = 1024;
        public const int WORLD_Z_SIZE = 2;

        private Random random = new Random();
        private List<Block> blocks;
        private GameWorld world;

        private Camera camera = new Camera();

        /// <summary>Creates a 800x600 window with the specified title.</summary>
        public Game()
            : base(1600, 1200, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 0, 0), "Blocks",
                GameWindowFlags.Default, DisplayDevice.Default, 3, 1, GraphicsContextFlags.Default)
        {
            VSync = VSyncMode.On;
            camera.SetLocation(15, 7, 120);
        }

        /// <summary>Load resources here.</summary>
        /// <param name="e">Not used.</param>
        protected override void OnLoad(EventArgs eArgs)
        {
            base.OnLoad(eArgs);

            Debug.Write("Generating block data...");
            blocks = new List<Block>(100);

            for (int i = 0; i < 100; i++)
            {
                blocks.Add(new Block(i > 0, i >= 50));
            }
            Debug.WriteLine("DONE");

            WorldGenerator generator = new WorldGenerator(WORLD_X_SIZE, WORLD_Y_SIZE, WORLD_Z_SIZE);
            world = generator.CreateWorld(LongRandom());
            world.SetBlockList(blocks);

            GL.ClearColor(0.1f, 0.1f, 0.1f, 0.0f);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Cw);

            GL.Enable(EnableCap.Blend);
            //GL.Enable(EnableCap.AlphaTest);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GlUtils.CheckGLErrors();
        }

        private long LongRandom()
        {
            byte[] buf = new byte[8];
            random.NextBytes(buf);
            return BitConverter.ToInt64(buf, 0);
        }

        /// <summary>
        /// Called when your window is resized. Set your viewport here. It is also
        /// a good place to set up your projection matrix (which probably changes
        /// along when the aspect ratio of your window).
        /// </summary>
        /// <param name="e">Not used.</param>
        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            camera.SetViewport(Width, Height);
        }

        /// <summary>
        /// Called when it is time to setup the next frame. Add you game logic here.
        /// </summary>
        /// <param name="e">Contains timing information for framerate independent logic.</param>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (Keyboard[Key.Escape])
                Exit();

            Vector3 camLoc = camera.Location;

            if (Keyboard[Key.A])
                camLoc.X -= 1;
            if (Keyboard[Key.D])
                camLoc.X += 1;
            if (Keyboard[Key.W])
                camLoc.Y += 1;
            if (Keyboard[Key.S])
                camLoc.Y -= 1;

            if (Keyboard[Key.KeypadPlus])
            {
                camLoc.Z = Math.Max(camLoc.Z - 3.0f, 4.0f * Block.Block_Size);
            }
            else if (Keyboard[Key.KeypadMinus])
            {
                camLoc.Z = Math.Min(camLoc.Z + 3.0f, 20.0f * Block.Block_Size);
            }

            camera.SetLocation(camLoc.X, camLoc.Y, camLoc.Z);
            camera.SetLookAtLocation(camLoc.X, camLoc.Y, 0);
            camera.Update();
        }

        protected override void OnUnload(EventArgs e)
        {
            Debug.Write("Deleting blocks...");
            foreach (Block block in blocks)
                block.Delete();

            Debug.WriteLine("Done");

            base.OnUnload(e);
        }

        /// <summary>
        /// Called when it is time to render the next frame. Add your rendering code here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            int polygonCount = world.Draw(camera);

            SwapBuffers();
            //GlUtils.CheckGLErrors();
            Title = string.Format("Polygon count = {0}", polygonCount);
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // The 'using' idiom guarantees proper resource cleanup.
            // We request 15 UpdateFrame events per second, and unlimited
            // RenderFrame events (as fast as the computer can handle).
            using (Game game = new Game())
            {
                game.Run(60.0);
            }
        }
    }
}
