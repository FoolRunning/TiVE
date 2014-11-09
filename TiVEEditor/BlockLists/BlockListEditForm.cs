using System;
using System.Windows.Forms;
using ProdigalSoftware.TiVE.Renderer.World;

namespace ProdigalSoftware.TiVEEditor.BlockLists
{
    public partial class BlockListEditForm : Form
    {
        private readonly BlockList blockList;
        private string filePath;
        private bool hasUnsavedChanges;

        public BlockListEditForm()
        {
            InitializeComponent();
            blockList = new BlockList();

            hasUnsavedChanges = true; // New block lists are always unsaved

            UpdateState();
        }

        public BlockListEditForm(string filePath) : this()
        {
            this.filePath = filePath;
            blockList = BlockList.FromBlockListFile(filePath);
            
            hasUnsavedChanges = false; // Block list came from a file, so it is saved by definition

            UpdateState();
        }

        private void btnSaveBlockList_Click(object sender, EventArgs e)
        {
            if (filePath == null)
            {
                // TODO: Prompt user for save location
                filePath = null;
            }

            if (filePath == null)
                return;

            blockList.SaveToBlockListFile(filePath);
            hasUnsavedChanges = false;
            UpdateState();
        }

        private void btnImportBlock_Click(object sender, EventArgs e)
        {
            hasUnsavedChanges = true;
            UpdateState();
        }

        private void btnDeleteBlock_Click(object sender, EventArgs e)
        {
            hasUnsavedChanges = true;
            UpdateState();
        }

        private void btnBlockProperties_Click(object sender, EventArgs e)
        {
            hasUnsavedChanges = true;
            UpdateState();
        }

        private void UpdateState()
        {
            btnSaveBlockList.Enabled = hasUnsavedChanges;
        }

        private void lstBxBlocks_DrawItem(object sender, DrawItemEventArgs e)
        {

        }

        private void lstBxBlocks_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
