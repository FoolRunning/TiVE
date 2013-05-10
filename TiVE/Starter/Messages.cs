using System;
using System.Drawing;
using System.Diagnostics;
using ProdigalSoftware.TiVE.Properties;

namespace ProdigalSoftware.TiVE.Starter
{
    /// <summary>
    /// Allows static access to the MessageView
    /// </summary>
    public static class Messages
    {
        #region Constants
        /// <summary>Maximum number of lines to show</summary>
        internal const int MAX_LINE_LIMIT = 50000;
        
        /// <summary>Default style of the text</summary>
        internal static readonly FontStyle DEFAULT_FONT_STYLE = FontStyle.Regular;
        
        /// <summary>Color of the lines to show on each line of text</summary>
        internal static readonly Color BASE_LINE_COLOR = Color.FromArgb(39, 39, 39);

        /// <summary>Default color of text</summary>
        internal static readonly Color DEFAULT_GRAY = Color.FromArgb(192, 192, 192);

        /// <summary>Nova engine blue</summary>
        internal static readonly Color TiVE_BLUE = Color.FromArgb(64, 192, 255);

        /// <summary>Nova engine dark blue</summary>
        internal static readonly Color TiVE_BLUE_DARK = Color.FromArgb(32, 96, 128);

        /// <summary>Color for misc text</summary>
        internal static readonly Color MISC_COLOR = Color.Orange;

        /// <summary>Color for the "Done" text</summary>
        internal static readonly Color DONE_COLOR = Color.Green;

        /// <summary>Color for error text</summary>
        internal static readonly Color ERROR_COLOR = Color.Red;

        /// <summary>Color for warning text</summary>
        internal static readonly Color WARNING_COLOR = Color.Yellow;

        /// <summary>Color for debug text</summary>
        internal static readonly Color DEBUG_COLOR = Color.FromArgb(0, 102, 255);
        #endregion

        #region Events
        /// <summary>Event fired when text is added to the messages</summary>
        internal static event Action TextAdded;
        /// <summary>Event fired when the message screen is cleared</summary>
        internal static event Action TextCleared;
        #endregion

        #region Member variables
        private static readonly MessageViewControl m_createdScreen;
        private static bool m_debugMode;
        #endregion

        #region Constructor
        static Messages()
        {
            m_createdScreen = new MessageViewControl();
            ClearText();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the contents of the messages as a string
        /// </summary>
        public static string AllText
        {
            get { return m_createdScreen.AllText; }
        }

        /// <summary>
        /// Returns the created MessageView
        /// </summary>
        internal static MessageViewControl MessageView
        {
            get { return m_createdScreen; }
        }

        /// <summary>
        /// Gets or sets whether or not to show debug messages
        /// </summary>
        public static bool DebugMode
        {
            get { return m_debugMode; }
            set { m_debugMode = value; }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Adds a debug message to the messages
        /// </summary>
        public static void AddDebug(string message)
        {
            AddFontSizeChange(12);
            AddFontStyleChange(FontStyle.Regular);
            PrintTabbedln(message, DEBUG_COLOR);
        }

        /// <summary>
        /// Adds the default "done" text: "Done" in the color green
        /// </summary>
        public static void AddDoneText()
        {
            AddFontStyleChange(FontStyle.Bold);
            AddTextInfo(new RightJustifyTextMessage(Resources.kstidDone, DONE_COLOR));
            AddNewLine();
        }

        /// <summary>
        /// Adds an error message to the messages
        /// </summary>
        public static void AddError(string message)
        {
            AddFontStyleChange(FontStyle.Bold);
            Println(message, ERROR_COLOR);
            Debug.WriteLine(message);
        }

        /// <summary>
        /// Adds the default "fail" text: "FAILED!" in the color red
        /// </summary>
        public static void AddFailText()
        {
            AddFontStyleChange(FontStyle.Bold);
            AddTextInfo(new RightJustifyTextMessage(Resources.kstidFailed, ERROR_COLOR));
            AddNewLine();
        }

        /// <summary>
        /// Adds a font size change to the messages
        /// </summary>
        public static void AddFontSizeChange(int newSize)
        {
            AddTextInfo(new FontSizeChangeMessage(newSize));
        }

        /// <summary>
        /// Adds a font style change to the messages
        /// </summary>
        public static void AddFontStyleChange(FontStyle style)
        {
            AddTextInfo(new FontStyleChangeMessage(style));
        }

        /// <summary>
        /// Adds a debug message to the messages that should only be displayed if in debug mode
        /// </summary>
        public static void AddIfDebug(string message)
        {
            if (m_debugMode) 
                AddDebug(message);
        }

        /// <summary>
        /// Adds a stack trace to the messages
        /// </summary>
        public static void AddStackTrace(Exception e)
        {
            while (e != null)
            {
                AddError(e.Message);

                string[] stackTraceLines = e.StackTrace.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stackLine in stackTraceLines)
                {
                    AddFontSizeChange(10);
                    AddFontStyleChange(FontStyle.Regular);
                    Println(stackLine);
                }

                e = e.InnerException;
            }
        }

        /// <summary>
        /// Adds a warning message to the messages
        /// </summary>
        public static void AddWarning(string message)
        {
            AddFontSizeChange(10);
            AddFontStyleChange(FontStyle.Bold);
            Println(message, WARNING_COLOR);

            Debug.WriteLine(message);
        }

        /// <summary>
        /// Resets the text on the MessageView
        /// </summary>
        public static void ClearText()
        {
            m_createdScreen.ClearText();
            if (TextCleared != null) 
                TextCleared();
        }

        /// <summary>
        /// Adds the specified text to the messages with the color gray
        /// </summary>
        public static void Print(string text)
        {
            Print(text, DEFAULT_GRAY);
        }

        /// <summary>
        /// Adds the specified text to the messages with the specified color
        /// </summary>
        public static void Print(string text, Color color)
        {
            AddTextInfo(new TextMessage(text, color));
        }

        /// <summary>
        /// Adds the specified text to the messages with the color gray centered and then adds a newline
        /// </summary>
        public static void PrintCenter(string text)
        {
            AddTextInfo(new CenteredTextMessage(text, DEFAULT_GRAY));
            AddNewLine();
        }

        /// <summary>
        /// Adds the specified text to the messages with the specified color centered and then adds a newline
        /// </summary>
        public static void PrintCenter(string text, Color color)
        {
            AddTextInfo(new CenteredTextMessage(text, color));
            AddNewLine();
        }

        /// <summary>
        /// Adds a newline
        /// </summary>
        public static void Println()
        {
            Println("", DEFAULT_GRAY);
        }

        /// <summary>
        /// Adds the specified text to the messages with the color gray and then adds a newline
        /// </summary>
        public static void Println(string text)
        {
            Println(text, DEFAULT_GRAY);
        }

        /// <summary>
        /// Adds the specified text to the messages with the specified color and then adds a newline
        /// </summary>
        public static void Println(string text, Color color)
        {
            AddTextInfo(new TextMessage(text, color));
            AddNewLine();
        }

        /// <summary>
        /// Adds the specified text to the messages with the color gray right-justified and then adds a newline
        /// </summary>
        public static void PrintRight(string text)
        {
            AddTextInfo(new RightJustifyTextMessage(text, DEFAULT_GRAY));
            AddNewLine();
        }

        /// <summary>
        /// Adds the specified text to the messages with the specified color right-justified and then adds a newline
        /// </summary>
        public static void PrintRight(string text, Color color)
        {
            AddTextInfo(new RightJustifyTextMessage(text, color));
            AddNewLine();
        }

        /// <summary>
        /// Adds the specified text to the messages with the specified color tabbed to the nearest tab boundary
        /// </summary>
        public static void PrintTabbed(string text)
        {
            AddTextInfo(new TabbedTextMessage(text, DEFAULT_GRAY));
        }

        /// <summary>
        /// Adds the specified text to the messages with the specified color tabbed to the nearest tab boundary
        /// </summary>
        public static void PrintTabbed(string text, Color color)
        {
            AddTextInfo(new TabbedTextMessage(text, color));
        }

        /// <summary>
        /// Adds the specified text to the messages with the specified color tabbed to the nearest tab boundary and then adds a newline
        /// </summary>
        public static void PrintTabbedln(string text)
        {
            AddTextInfo(new TabbedTextMessage(text, DEFAULT_GRAY));
            AddNewLine();
        }

        /// <summary>
        /// Adds the specified text to the messages with the specified color tabbed to the nearest tab boundary and then adds a newline
        /// </summary>
        public static void PrintTabbedln(string text, Color color)
        {
            AddTextInfo(new TabbedTextMessage(text, color));
            AddNewLine();
        }

        /// <summary>
        /// Sets the font size and the font style to their default values
        /// </summary>
        public static void SetDefaultFontStyle()
        {
            AddFontSizeChange(MessageViewControl.DEFAULT_FONT_SIZE);
            AddFontStyleChange(DEFAULT_FONT_STYLE);
        }
        #endregion

        #region Debug code
        /// <summary>
        /// Adds text to test different abilities of the message window
        /// </summary>
        internal static void MessageTest()
        {
            SetDefaultFontStyle();
            AddDebug("----   Debug testing area   ----");
            Print("Test C", Color.FromArgb(0, 255, 0));
            Print("o", Color.FromArgb(85, 255, 0));
            Print("l", Color.FromArgb(170, 255, 0));
            Print("o", Color.FromArgb(255, 255, 0));
            Print("r", Color.FromArgb(255, 170, 0));
            Print("e", Color.FromArgb(255, 85, 0));
            Println("d Text", Color.FromArgb(255, 0, 0));
            Print("this");
            PrintTabbed("is");
            PrintTabbed("to");
            PrintTabbed("test");
            PrintTabbed("tabbing");
            PrintTabbedln("over");
            Println("left-justified");
            PrintCenter("centered");
            PrintRight("right-justified");
            AddDebug("This is debug text");
            AddWarning("This is warning text");
            AddError("This is error text");
            Print("Testing done text...");
            AddDoneText();
            Print("Testing failed text...");
            AddFailText();
            AddFontSizeChange(9);
            AddFontStyleChange(FontStyle.Regular);
            Print("Test ");
            AddFontSizeChange(13);
            Print("changing ");
            AddFontSizeChange(17);
            Print("text ");
            AddFontSizeChange(7);
            Println("size ");
            SetDefaultFontStyle();
            Print("Plain text - ");
            AddFontStyleChange(FontStyle.Italic);
            Print("Italic text - ");
            AddFontStyleChange(FontStyle.Bold);
            Print("Bold text - ");
            AddFontStyleChange(FontStyle.Bold | FontStyle.Italic);
            Println("Bold and italic text ");
            AddDebug("----   End debug testing area   ----");
            Println();
            Println();
        }
        #endregion

        #region Private helper methods
        /// <summary>
        /// Adds a new line to the messages
        /// </summary>
        private static void AddNewLine()
        {
            if (m_createdScreen.Items.Count < MAX_LINE_LIMIT)
            {
                m_createdScreen.StartNewLine();
            }
            else if (m_createdScreen.Items.Count == MAX_LINE_LIMIT)
            {
                m_createdScreen.StartNewLine();
                m_createdScreen.AddMessage(new FontSizeChangeMessage(14));
                m_createdScreen.AddMessage(new FontStyleChangeMessage(FontStyle.Bold | FontStyle.Italic));
                m_createdScreen.AddMessage(new CenteredTextMessage("----    " + Resources.kstidMaxLines + "    ----", ERROR_COLOR));
                m_createdScreen.StartNewLine();
            }
        }

        /// <summary>
        /// Adds the specified message to the messages
        /// </summary>
        private static void AddTextInfo(Message info)
        {
            m_createdScreen.AddMessage(info);
            if (TextAdded != null) 
                TextAdded();
        }
        #endregion
    }
}
