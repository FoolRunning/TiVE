using System;
using System.Drawing;

namespace ProdigalSoftware.TiVE
{
    /// <summary>
    /// Represents a native window created by the backend. 
    /// </summary>
    internal interface INativeWindow : IDisposable
    {
        /// <summary>
        /// Fired when the native window is resized
        /// </summary>
        event Action<Rectangle> WindowResized;

        /// <summary>
        /// Fired when the native window is getting ready to close. 
        /// The current implementation context should still be valid when this is called.
        /// </summary>
        event EventHandler WindowClosing;

        /// <summary>
        /// Gets the current client bounds of the window (i.e. the part of the window that should display the rendered contents)
        /// </summary>
        Rectangle ClientBounds { get; }

        /// <summary>
        /// Sets the window title
        /// </summary>
        string WindowTitle { set; }

        /// <summary>
        /// Gets an implementation of the keyboard interface
        /// </summary>
        IKeyboard KeyboardImplementation { get; }

        /// <summary>
        /// Sets the icon to use for the window
        /// </summary>
        Icon Icon { set; }

        /// <summary>
        /// Requests the native window to close itself
        /// </summary>
        void CloseWindow();

        /// <summary>
        /// Requests that the native window process any pending native events in the event queue
        /// </summary>
        void ProcessNativeEvents();

        /// <summary>
        /// Updates the display with the new frame (typically by swapping buffers)
        /// </summary>
        void UpdateDisplayContents();
    }
}
