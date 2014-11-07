using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace ProdigalSoftware.TiVE.Starter
{
    internal partial class StarterForm : Form
    {
        /// <summary>string containing the copyright information</summary>
        private const string CopyrightString = "© 2013-2014 Prodigal Software";

        private Point startOfDrag;
        private Point startingLocation;

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

            btnStart.Enabled = false;
            tbMessages.Enabled = false;
        }

        public void EnableControls()
        {
            Invoke(new Action(() =>
                {
                    tbMessages.Enabled = true;
                    btnStart.Enabled = true;
                }));
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
            Messages.PrintCenter(Properties.Resources.kstidEngineTitle1, Messages.TiVE_BLUE);
            Messages.AddFontSizeChange(20);
            Messages.PrintCenter(Properties.Resources.kstidEngineTitle2, Messages.TiVE_BLUE_DARK);
            Messages.AddFontSizeChange(10);
            Messages.AddFontStyleChange(FontStyle.Regular);
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Messages.PrintCenter("version " + version, Color.Gray);
            Messages.AddFontSizeChange(8);
            Messages.PrintCenter(CopyrightString);
            Messages.Println();
            Messages.SetDefaultFontStyle();
            Messages.Println("*** Witty message here ***", Messages.MISC_COLOR);

            //Messages.MessageTest();
        }

        private void tableLayoutPanel1_MouseDown(object sender, MouseEventArgs e)
        {
            startOfDrag = PointToScreen(e.Location);
            startingLocation = Location;
        }

        private void tableLayoutPanel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (startOfDrag != Point.Empty)
            {
                Point locationOnScreen = PointToScreen(e.Location);
                Location = new Point(startingLocation.X + (locationOnScreen.X - startOfDrag.X), 
                    startingLocation.Y + (locationOnScreen.Y - startOfDrag.Y));
            }
        }

        private void tableLayoutPanel1_MouseUp(object sender, MouseEventArgs e)
        {
            startOfDrag = Point.Empty;
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
