using System;
using System.Drawing;

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
        private static readonly SolidBrush s_stringBrush = new SolidBrush(Color.Transparent);

        /// <summary>The text to display</summary>
        private readonly string m_text;
        /// <summary>Color of the text</summary>
        private readonly Color m_color;

        /// <summary>
        /// Creates a new SimpleTextInfo with the specified text and color
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <param name="color">The color of the text</param>
        internal TextMessage(string text, Color color)
        {
            m_text = text ?? "(null)";
            m_color = color;
        }

        /// <summary>
        /// Gets the size of this message (in pixels) when drawn with the specified graphics in the specified state
        /// </summary>
        public virtual Size GetSize(TextState state, Graphics g)
        {
            return g.MeasureString(!string.IsNullOrEmpty(m_text) ? m_text : " ", state.Font).ToSize();
        }

        public override void DrawText(TextState state, Graphics g)
        {
            s_stringBrush.Color = m_color;
            g.DrawString(m_text, state.Font, s_stringBrush, state.X, state.Y);
        }

        /// <summary>
        /// Gets the text of this message
        /// </summary>
        public virtual string Text
        { 
            get { return m_text; }
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
        public const int TAB_PIXEL_SIZE = 35;

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
            state.X = ((state.X / TAB_PIXEL_SIZE + 1) * TAB_PIXEL_SIZE);
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
        private readonly int m_size;
        private Font cachedFont;

        /// <summary>
        /// Creates a new FontSizeChangeMessage with the specified font size
        /// </summary>
        internal FontSizeChangeMessage(int size)
        {
            m_size = size;
        }

        public override void UpdateState(TextState state)
        {
            if (cachedFont == null)
                cachedFont = new Font(state.Font.FontFamily, m_size);
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
        private readonly FontStyle m_style;
        private Font cachedFont;

        /// <summary>
        /// Creates a new FontStyleChangeMessage with the specified font style
        /// </summary>
        internal FontStyleChangeMessage(FontStyle style)
        {
            m_style = style;
        }

        public override void UpdateState(TextState state)
        {
            if (cachedFont == null)
                cachedFont = new Font(state.Font, m_style);
            state.Font = cachedFont;
        }
    }
    #endregion
}
