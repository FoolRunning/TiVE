using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProdigalSoftware.TiVE.Starter
{
    #region Message class
    /// <summary>
    /// Represents a single message item in the MessageView
    /// </summary>
    internal abstract class Message
    {
        /// <summary>The next message in the linked list</summary>
        public Message NextMessage;

        /// <summary>The size (in pixels) of this message</summary>
        public Size Size { get; set; }

        /// <summary>
        /// Draws any text used by this message using the specified graphics object
        /// </summary>
        public virtual void DrawText(TextState state, Graphics g)
        {
        }

        /// <summary>
        /// Updates the drawing state of the specified text state object
        /// </summary>
        public virtual void UpdateState(TextState state)
        {
        }
    }
    #endregion

    #region TextMessage class
    /// <summary>
    /// Simple Message that just adds the text and adjusts the location for the next text
    /// </summary>
    internal class TextMessage : Message
    {
        private static readonly SolidBrush stringBrush = new SolidBrush(Color.Transparent);

        /// <summary>The text to display</summary>
        private readonly string text;
        /// <summary>Color of the text</summary>
        private readonly Color color;

        /// <summary>
        /// Creates a new SimpleTextInfo with the specified text and color
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <param name="color">The color of the text</param>
        internal TextMessage(string text, Color color)
        {
            this.text = text ?? "(null)";
            this.color = color;
        }

        /// <summary>
        /// Gets the size of this message (in pixels) when drawn with the specified graphics in the specified state
        /// </summary>
        public Size GetSize(TextState state, Graphics g)
        {
            return TextRenderer.MeasureText(g, !string.IsNullOrEmpty(text) ? text : " ", state.Font);
        }

        public override void DrawText(TextState state, Graphics g)
        {
            stringBrush.Color = color;
            TextRenderer.DrawText(g, text, state.Font, new Point(state.X, state.Y), color);
        }

        /// <summary>
        /// Gets the text of this message
        /// </summary>
        public string Text
        { 
            get { return text; }
        }

        public override string ToString()
        {
            return text;
        }
    }
    #endregion

    #region CenteredTextMessage class
    /// <summary>
    /// Simple TextMessage that just adds the text centered.
    /// </summary>
    internal sealed class CenteredTextMessage : TextMessage
    {
        /// <summary>
        /// Creates a new CenteredTextMessage with the specified text and color
        /// </summary>
        internal CenteredTextMessage(String text, Color color) : base(text, color)
        {
        }

        public override void DrawText(TextState state, Graphics g)
        {
            state.X = (state.Width - Size.Width) / 2;
            base.DrawText(state, g);
        }
    }
    #endregion

    #region RightJustifyTextMessage class
    /// <summary>
    /// Simple TextMessage that just adds the text to the right side of the view.
    /// </summary>
    internal sealed class RightJustifyTextMessage : TextMessage
    {
        /// <summary>
        /// Creates a new RightJustifyTextMessage with the specified text and color
        /// </summary>
        internal RightJustifyTextMessage(String text, Color color) : base(text, color)
        {
        }

        public override void DrawText(TextState state, Graphics g)
        {
            state.X = state.Width - Size.Width - TextState.MarginWidth;
            base.DrawText(state, g);
        }
    }
    #endregion

    #region TabbedTextMessage class
    /// <summary>
    /// Simple TextMessage that just adds the text tabbed to the nearest tab boundary
    /// </summary>
    internal sealed class TabbedTextMessage : TextMessage
    {
        /// <summary>Number of pixels to tab over</summary>
        private const int TabPixelSize = 35;

        /// <summary>
        /// Creates a new TabbedTextInfo with the specified text and color
        /// </summary>
        internal TabbedTextMessage(String text, Color color) : base(text, color)
        {
        }

        /// <summary>
        /// Draws the text stored in this TextInfo tabbed over
        /// </summary>
        public override void DrawText(TextState state, Graphics g)
        {
            state.X = ((state.X / TabPixelSize + 1) * TabPixelSize);
            base.DrawText(state, g);
        }
    }
    #endregion

    #region FontSizeChangeMessage class
    /// <summary>
    /// Simple Message that changes the font style
    /// </summary>
    internal sealed class FontSizeChangeMessage : Message
    {
        private readonly int size;
        private Font cachedFont;

        /// <summary>
        /// Creates a new FontSizeChangeMessage with the specified font size
        /// </summary>
        internal FontSizeChangeMessage(int size)
        {
            this.size = size;
        }

        public override void UpdateState(TextState state)
        {
            if (cachedFont == null)
                cachedFont = new Font(state.Font.FontFamily, size);
            state.Font = cachedFont;
        }
    }
    #endregion

    #region FontStyleChangeMessage class
    /// <summary>
    /// Simple Message that changes the font style
    /// </summary>
    internal sealed class FontStyleChangeMessage : Message
    {
        private readonly FontStyle style;
        private Font cachedFont;

        /// <summary>
        /// Creates a new FontStyleChangeMessage with the specified font style
        /// </summary>
        internal FontStyleChangeMessage(FontStyle style)
        {
            this.style = style;
        }

        public override void UpdateState(TextState state)
        {
            if (cachedFont == null)
                cachedFont = new Font(state.Font, style);
            state.Font = cachedFont;
        }
    }
    #endregion
}
