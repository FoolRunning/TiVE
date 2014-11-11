using System;
using System.Windows.Forms;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVEEditor.BlockLists;

namespace ProdigalSoftware.TiVEEditor
{
    public partial class TiVEEditorForm : Form
    {
        public TiVEEditorForm()
        {
            InitializeComponent();
        }

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
            string blockFilePath;
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.CheckFileExists = true;
                dialog.Multiselect = false;
                dialog.InitialDirectory = Environment.CurrentDirectory;
                dialog.Title = "Open Block List File";
                dialog.Filter = string.Format("Block List Files ({0})|{0}", "*." + BlockList.FileExtension);
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                blockFilePath = dialog.FileName;
            }
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
