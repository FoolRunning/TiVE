using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using ProdigalSoftware.TiVE.Settings;

namespace ProdigalSoftware.TiVE.Starter
{
    internal partial class StarterForm : Form
    {
        /// <summary>string containing the copyright information</summary>
        private const string CopyrightString = "© 2013-2015 Prodigal Software";

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

        public void AfterInitialLoad()
        {
            Invoke(new Action(() =>
                {
                    tbMessages.Enabled = true;
                    btnStart.Enabled = true;
                    InitializeOptions();
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
            TiVEController.UserSettings.Save();
            TiVEController.RunEngine();
        }

        private void btnCopyText_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(Messages.AllText, TextDataFormat.UnicodeText);
        }

        private void UserOption_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            UserSettingOptions optionChanged = (UserSettingOptions)comboBox.Tag;
            TiVEController.UserSettings.Set(optionChanged.SettingKey, ((UserSettingOption)comboBox.SelectedItem).Value);
        }
        #endregion

        private void InitializeOptions()
        {
            ClearOptionsOnPanel(pnlDisplayOptionsList);
            ClearOptionsOnPanel(pnlAdvancedOptionsList);

            int row = 0;
            foreach (UserSettingOptions options in TiVEController.UserSettings.AllUserSettingOptions)
            {
                TableLayoutPanel panelToAddTo;
                switch (options.OptionTab)
                {
                    case UserOptionTab.Display: panelToAddTo = pnlDisplayOptionsList; break;
                    case UserOptionTab.Controls:
                    case UserOptionTab.Sound:
                    default: panelToAddTo = pnlAdvancedOptionsList; break;
                }

                Label label = new Label();
                label.Font = new Font(label.Font.FontFamily, 12);
                label.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                label.AutoSize = true;
                label.Text = options.Description;
                panelToAddTo.Controls.Add(label, 0, row);

                ComboBox comboBox = new ComboBox();
                comboBox.Font = new Font(comboBox.Font.FontFamily, 12);
                comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                comboBox.Anchor = AnchorStyles.Left;
                comboBox.Tag = options;
                comboBox.Width = 250;
                int index = 0;
                int indexToSelect = 0;
                Setting currentSetting = TiVEController.UserSettings.Get(options.SettingKey);
                foreach (UserSettingOption option in options.ValidOptions)
                {
                    comboBox.Items.Add(option);
                    if (option.Value == currentSetting)
                        indexToSelect = index;
                    index++;
                }

                comboBox.SelectedIndex = indexToSelect;
                comboBox.SelectedIndexChanged += UserOption_SelectedIndexChanged;

                panelToAddTo.Controls.Add(comboBox, 1, row);
                panelToAddTo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                row++;
            }

            tabControl1.TabPages.Remove(tbControls);
            tabControl1.TabPages.Remove(tbSound);
        }

        private static void ClearOptionsOnPanel(TableLayoutPanel panel)
        {
            panel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panel.RowCount = 0;
            panel.RowStyles.Clear();
        }
    }
}
