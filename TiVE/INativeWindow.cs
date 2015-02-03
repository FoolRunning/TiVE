using System;
using System.Drawing;

namespace ProdigalSoftware.TiVE
{
    /// <summary>
    /// Represents a native display created by the backend. 
    /// </summary>
    internal interface INativeDisplay : IDisposable
    {
        /// <summary>
        /// Fired when the native display is resized
        /// </summary>
        event Action<Rectangle> DisplayResized;

        /// <summary>
        /// Fired when the native display is getting ready to close. 
        /// The current implementation context should still be valid when this is called.
        /// </summary>
        event EventHandler DisplayClosing;

        /// <summary>
        /// Gets the current client bounds of the display (i.e. the part of the display that should display the rendered contents)
        /// </summary>
        Rectangle ClientBounds { get; }

        /// <summary>
        /// Sets the display title if windowed
        /// </summary>
        string WindowTitle { set; }

        /// <summary>
        /// Sets the icon to use for the display
        /// </summary>
        Icon Icon { set; }

        /// <summary>
        /// Requests the native display to close itself
        /// </summary>
        void CloseWindow();

        /// <summary>
        /// Requests that the native display process any pending native events in the event queue
        /// </summary>
        void ProcessNativeEvents();

        /// <summary>
        /// Updates the display with the new frame (typically by swapping buffers)
        /// </summary>
        void UpdateDisplayContents();
    }
}
