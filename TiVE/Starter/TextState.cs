using System.Drawing;

namespace ProdigalSoftware.TiVE.Starter
{
    /// <summary>
    /// Holds current state information for drawing text in the MessageView
    /// </summary>
    internal sealed class TextState
    {
        /// <summary>The margin (in pixels) for the drawn text</summary>
        public const int MarginWidth = 5;
        
        /// <summary>The width of the drawable area</summary>
        public readonly int Width;

        /// <summary>The x location to draw the text</summary>
        public int X = MarginWidth;

        /// <summary>The y location to draw the text</summary>
        public int Y = 0;

        /// <summary>The current font</summary>
        public Font Font;

        /// <summary>
        /// Creates a new TextState with the specified parameters
        /// </summary>
        /// <param name="width">The width of the drawable area</param>
        /// <param name="font">The current font</param>
        public TextState(int width, Font font, int x, int y)
        {
            Width = width;
            Font = font;
            X = x;
            Y = y;
        }
    }
}
