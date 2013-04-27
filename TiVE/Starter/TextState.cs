using System.Drawing;

namespace ProdigalSoftware.TiVE.Starter
{
    /// <summary>
    /// Holds current state information for drawing text in the MessageView
    /// </summary>
    public sealed class TextState
    {
        /// <summary>The margin (in pixels) for the drawn text</summary>
        public const int MARGIN_WIDTH = 5;
        
        /// <summary>The width of the drawable area</summary>
        public readonly int Width;

        /// <summary>The x location to draw the text</summary>
        public int X = MARGIN_WIDTH;

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
            this.Width = width;
            this.Font = font;
            this.X = x;
            this.Y = y;
        }
    }
}
