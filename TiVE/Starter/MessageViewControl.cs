﻿using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Text;

namespace ProdigalSoftware.TiVE.Starter
{
    /// <summary>
    /// Light-weight Control that prints out text with color and format properties.
    /// </summary>
    internal sealed partial class MessageViewControl : ListBox
    {
        #region Constants
        /// <summary>Default font size</summary>
        public const int DEFAULT_FONT_SIZE = 14;

        private static readonly SolidBrush s_backgroundBrush = new SolidBrush(Color.Black);
        private static readonly Pen s_baselinePen = new Pen(Messages.BASE_LINE_COLOR);
        private static readonly FontFamily DEFAULT_FONT_FAMILY = new FontFamily(GenericFontFamilies.SansSerif);
        private static readonly Font DEFAULT_FONT = new Font(DEFAULT_FONT_FAMILY, DEFAULT_FONT_SIZE, FontStyle.Regular);
        #endregion

        #region Member variables
        private MessageLine m_currentLine;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new MessageViewControl
        /// </summary>
        internal MessageViewControl()
        {
            InitializeComponent();
            BackColor = Color.Black;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the contents of the message screen as a string
        /// </summary>
        internal string AllText
        {
            get
            {
                StringBuilder toReturn = new StringBuilder();
                lock (Items)
                {
                    for (int i = 0; i < Items.Count; i++)
                        toReturn.Append(((MessageLine)Items[i]).Text);
                }
                return toReturn.ToString();
            }
        }
        #endregion

        #region Internal methods
        /// <summary>
        /// Adds the specified IMessage to this MessageView
        /// </summary>
        internal void AddMessage(Message info)
        {
            lock (Items)
                m_currentLine.AddMessage(info);
        }

        internal void StartNewLine()
        {
            if (IsHandleCreated && InvokeRequired)
            {
                Invoke(new Action(StartNewLine));
                return;
            }

            if (m_currentLine != null)
            {
                AdjustSize(m_currentLine);
                lock (Items)
                    Items.Add(m_currentLine);
            }
            
            TopIndex = Items.Count - 1;

            m_currentLine = new MessageLine();
        }

        /// <summary>
        /// Resets the text on the MessageView
        /// </summary>
        internal void ClearText()
        {
            if (IsHandleCreated && InvokeRequired)
            {
                Invoke(new Action(ClearText));
                return;
            }
            
            lock (Items)
                Items.Clear();
            StartNewLine();
        }
        #endregion

        #region Overridden methods

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            e.Graphics.FillRectangle(s_backgroundBrush, e.Bounds);
            if (e.Index > 5)
            {
                e.Graphics.DrawLine(s_baselinePen, TextState.MarginWidth, e.Bounds.Bottom - 1
                    , e.Bounds.Width - TextState.MarginWidth, e.Bounds.Bottom - 1);
            }

            TextState state = new TextState(e.Bounds.Width, DEFAULT_FONT, e.Bounds.X, e.Bounds.Y);
            ((MessageLine)Items[e.Index]).Draw(state, e.Graphics);
        }

        protected override void OnMeasureItem(MeasureItemEventArgs e)
        {
            e.ItemHeight = ((MessageLine)Items[e.Index]).Height;
        }
        #endregion

        #region Private helper methods
        /// <summary>
        /// Adjusts the size of the MessageView to make room for all the text.
        /// </summary>
        private void AdjustSize(MessageLine newMessageline)
        {
            Graphics g = CreateGraphics();

            TextState state = new TextState(ClientRectangle.Width, DEFAULT_FONT, 0, 0);
            newMessageline.CalculateHeight(state, g);
        }
        #endregion
    }
}
