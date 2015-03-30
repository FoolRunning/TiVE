using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ProdigalSoftware.TiVEEditor.BlockLists;
using ProdigalSoftware.TiVEEditor.Properties;

namespace ProdigalSoftware.TiVEEditor
{
    public partial class TiVEEditorForm : Form
    {
        private readonly Settings settings = Settings.Default;

        public TiVEEditorForm()
        {
            InitializeComponent();
        }

        #region Overrides of Form
        protected override void OnLoad(EventArgs e)
        {
            Rectangle newBounds = settings.MainEditorBounds;
            if (newBounds != Rectangle.Empty)
                Bounds = settings.MainEditorBounds; // Must be set before state to get the proper restore bounds
            WindowState = settings.MainEditorState;
            base.OnLoad(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            settings.MainEditorBounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
            settings.MainEditorState = WindowState != FormWindowState.Minimized ? WindowState : FormWindowState.Normal;
            
            settings.Save();
            base.OnClosing(e);
        }
        #endregion

        #region Event Handlers
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void newMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowMapListEditDialog();
        }

        private void newScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void newBlockListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowBlockListEditDialog();
        }

        private void openMapListMenuItem_Click(object sender, EventArgs e)
        {
            ShowMapListEditDialog();
        }

        private void openScriptListMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void openBlockListMenuItem_Click(object sender, EventArgs e)
        {
            string blockFilePath = BlockListEditForm.ChooseBlockListFile(this, false);
            if (blockFilePath != null)
                ShowBlockListEditDialog(blockFilePath);
        }
        #endregion

        private void ShowMapListEditDialog()
        {
            MapListEditForm form = new MapListEditForm();
            form.MdiParent = this;
            form.Show();
        }

        private void ShowBlockListEditDialog(string blockFilePath = null)
        {
            BlockListEditForm form = new BlockListEditForm(blockFilePath);
            form.MdiParent = this;
            form.Show();
        }
    }
}
