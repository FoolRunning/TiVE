using System;
using System.ComponentModel;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using ProdigalSoftware.TiVE.Renderer;

namespace ProdigalSoftware.TiVE.Core.Backend.OpenTKImpl
{
    internal sealed class OpenTKDisplay : GameWindow, INativeDisplay
    {
        public OpenTKDisplay(DisplaySetting displaySetting, FullScreenMode fullScreenMode, bool vsync, int antiAliasAmount)
            : base(displaySetting.Width, displaySetting.Height, new GraphicsMode(32, 16, 0, antiAliasAmount), "TiVE",
                fullScreenMode == FullScreenMode.FullScreen ? GameWindowFlags.Fullscreen : GameWindowFlags.Default,
                DisplayDevice.Default, 3, 1, GraphicsContextFlags.ForwardCompatible)
        {
            if (fullScreenMode == FullScreenMode.WindowFullScreen)
            {
                Width = DisplayDevice.Default.Width;
                Height = DisplayDevice.Default.Height;
                WindowBorder = WindowBorder.Hidden;
                WindowState = WindowState.Fullscreen;
            }
            else if (fullScreenMode == FullScreenMode.FullScreen)
            {
                DisplayResolution resolution = DisplayDevice.Default.SelectResolution(
                    displaySetting.Width, displaySetting.Height, 32, displaySetting.RefreshRate);
                DisplayDevice.Default.ChangeResolution(resolution);

                // This seems to be needed when multiple monitors are present and the chosen resolution would push a 
                // centered window onto the other monitor when the resolution is changed.
                X = Y = 0;

                // Not sure why, but some times when switching to fullscreen, the window will get the
                // size of a secondary monitor. Reset the size just in case.
                Width = resolution.Width;
                Height = resolution.Height;
            }
            else // Windowed
            {
                WindowBorder = WindowBorder.Resizable;
                WindowState = WindowState.Normal;
            }

            VSync = vsync ? VSyncMode.On : VSyncMode.Off;

            Closing += OpenGLDisplay_Closing;
            Resize += OpenGLDisplay_Resize;
            CursorVisible = false;
        }

        public event Action<Rectangle> DisplayResized;
        public event EventHandler DisplayClosing;

        public Rectangle ClientBounds
        {
            get { return ClientRectangle; }
        }

        public bool ShowMouseCursor
        {
            set { CursorVisible = value; }
        }

        public string WindowTitle
        {
            set { Title = value; }
        }

        public void CloseWindow()
        {
            Exit();

            DisplayDevice.Default.RestoreResolution();
        }

        public void ProcessNativeEvents()
        {
            ProcessEvents();
        }

        public void UpdateDisplayContents()
        {
            GlUtils.CheckGLErrors();
            SwapBuffers();
        }

        void OpenGLDisplay_Closing(object sender, CancelEventArgs e)
        {
            // Although this looks weird, we need to cancel the disposing of the OpenGL context, but we will still exit after firing the event
            e.Cancel = true;

            if (DisplayClosing != null)
                DisplayClosing(this, EventArgs.Empty);
        }

        void OpenGLDisplay_Resize(object sender, EventArgs e)
        {
            if (DisplayResized != null)
                DisplayResized(ClientRectangle);
        }
    }
}
