﻿using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using ProdigalSoftware.TiVE.Renderer;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE
{
    internal class Game : GameWindow
    {
        public const int WorldXSize = 10000;
        public const int WorldYSize = 2000;
        public const int WorldZSize = 2;

        private BlockList blockList;
        private GameWorld world;

        private readonly Camera camera = new Camera();

        /// <summary>
        /// Creates a 1600x1200 window with the specified title.
        /// </summary>
        public Game()
            : base(1600, 1200, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 16, 0, 0), "Project M",
                GameWindowFlags.Default, DisplayDevice.Default, 3, 1, GraphicsContextFlags.Default)
        {
            VSync = VSyncMode.On;
            camera.SetLocation(20000, (WorldYSize - 75) * BlockInformation.BlockSize, 300);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnLoad(EventArgs eArgs)
        {
            base.OnLoad(eArgs);

            blockList = BlockList.CreateBlockList();

            WorldGenerator generator = new WorldGenerator(WorldXSize, WorldYSize, WorldZSize);
            world = generator.CreateWorld(LongRandom(), blockList); //LongRandom());

            if (world == null)
                Exit();

            GL.ClearColor(0.1f, 0.1f, 0.1f, 0.0f);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Cw);

            GL.Enable(EnableCap.Blend);
            //GL.Enable(EnableCap.AlphaTest);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GlUtils.CheckGLErrors();
        }

        private static long LongRandom()
        {
            byte[] buf = new byte[8];
            Random random = new Random();
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

            bool shift = Keyboard[Key.ShiftLeft];
            if (Keyboard[Key.A])
                camLoc.X -= shift ? 5 : 1;
            if (Keyboard[Key.D])
                camLoc.X += shift ? 5 : 1;
            if (Keyboard[Key.W])
                camLoc.Y += shift ? 5 : 1;
            if (Keyboard[Key.S])
                camLoc.Y -= shift ? 5 : 1;

            if (Keyboard[Key.KeypadPlus])
            {
                camLoc.Z = Math.Max(camLoc.Z - 2.0f, 4.0f * BlockInformation.BlockSize);
            }
            else if (Keyboard[Key.KeypadMinus])
            {
                camLoc.Z = Math.Min(camLoc.Z + 2.0f, 20.0f * BlockInformation.BlockSize);
            }

            camera.SetLocation(camLoc.X, camLoc.Y, camLoc.Z);
            camera.SetLookAtLocation(camLoc.X, camLoc.Y, 0);
            camera.Update();
        }

        protected override void OnUnload(EventArgs e)
        {
            blockList.DeleteBlocks();

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
    }
}
