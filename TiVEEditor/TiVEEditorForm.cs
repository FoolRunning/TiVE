using System.Windows.Forms;
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
        private void exitToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void newMapToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            ShowMapListEditDialog();
        }

        private void newScriptToolStripMenuItem_Click(object sender, System.EventArgs e)
        {

        }

        private void newBlockListToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            ShowBlockListEditDialog();
        }

        private void openMapListMenuItem_Click(object sender, System.EventArgs e)
        {
            ShowMapListEditDialog();
        }

        private void openScriptListMenuItem_Click(object sender, System.EventArgs e)
        {

        }

        private void openBlockListMenuItem_Click(object sender, System.EventArgs e)
        {
            ShowBlockListEditDialog();
        }
        #endregion

        private void ShowMapListEditDialog()
        {
            MapListEditForm form = new MapListEditForm();
            form.MdiParent = this;
            form.Show();
        }

        private void ShowBlockListEditDialog()
        {
            BlockListEditForm form = new BlockListEditForm();
            form.MdiParent = this;
            form.Show();
        }
    }
}
