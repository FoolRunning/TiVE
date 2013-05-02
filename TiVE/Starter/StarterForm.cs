using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using ProdigalSoftware.TiVE.Properties;

namespace ProdigalSoftware.TiVE.Starter
{
    internal partial class StarterForm : Form
    {
        /// <summary>string containing the copyright information</summary>
        private const string COPYRIGHT_STRING = "© 2013 Prodigal Software";

        private Point m_startOfDrag;
        private Point m_startingLocation;

        /// <summary>
        /// Creates a new NovaStarterForm
        /// </summary>
        public StarterForm()
        {
            InitializeComponent();

            Messages.TextAdded += Messages_TextAdded;
            Messages.TextCleared += Messages_TextCleared;

            MessageViewControl messageView = Messages.MessageView;
            tbMessages.Controls.Add(messageView);
        }

        /// <summary>
        /// Initializes the form when it's loaded
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Messages.ClearText();

            Messages.MessageView.Select();
        }

        #region Event handlers
        /// <summary>
        /// Called when text is added to the messages
        /// </summary>
        private void Messages_TextAdded()
        {
            // Switch to the message tab so the user can see the new message
            if (InvokeRequired)
            {
                Invoke(new Action(Messages_TextAdded));
                return;
            }

            if (tabControl1.SelectedTab != tbMessages)
                tabControl1.SelectedTab = tbMessages;
        }

        private void Messages_TextCleared()
        {
            Messages.AddFontSizeChange(50);
            Messages.AddFontStyleChange(FontStyle.Bold);
            Messages.PrintCenter(Resources.kstidEngineTitle1, Messages.TiVE_BLUE);
            Messages.AddFontSizeChange(20);
            Messages.PrintCenter(Resources.kstidEngineTitle2, Messages.TiVE_BLUE_DARK);
            Messages.AddFontSizeChange(10);
            Messages.AddFontStyleChange(FontStyle.Regular);
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Messages.PrintCenter("version " + version, Color.Gray);
            Messages.AddFontSizeChange(8);
            Messages.PrintCenter(COPYRIGHT_STRING);
            Messages.Println();
            Messages.SetDefaultFontStyle();
            Messages.Println("*** Witty message here ***", Messages.MISC_COLOR);

            //Messages.MessageTest();
        }

        private void tableLayoutPanel1_MouseDown(object sender, MouseEventArgs e)
        {
            m_startOfDrag = PointToScreen(e.Location);
            m_startingLocation = Location;
        }

        private void tableLayoutPanel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_startOfDrag != Point.Empty)
            {
                Point locationOnScreen = PointToScreen(e.Location);
                Location = new Point(m_startingLocation.X + (locationOnScreen.X - m_startOfDrag.X), 
                    m_startingLocation.Y + (locationOnScreen.Y - m_startOfDrag.Y));
            }
        }

        private void tableLayoutPanel1_MouseUp(object sender, MouseEventArgs e)
        {
            m_startOfDrag = Point.Empty;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            TiVEController.RunEngine();
        }

        private void btnCopyText_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(Messages.AllText, TextDataFormat.UnicodeText);
        }
        #endregion
    }
}
