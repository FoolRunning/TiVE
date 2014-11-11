using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEEditor.BlockLists
{
    public partial class BlockListEditForm : Form
    {
        private readonly List<BlockInList> blocksInList = new List<BlockInList>();
        private readonly GameWorld gameWorld;
        private readonly BlockList blockList;
        private string filePath;
        private bool hasUnsavedChanges;

        private BlockListEditForm()
        {
            InitializeComponent();
        }

        public BlockListEditForm(string filePath) : this()
        {
            this.filePath = filePath;
            blockList = filePath != null ? BlockList.FromBlockListFile(filePath) : new BlockList();
            UpdateBlocksInList();
            
            hasUnsavedChanges = (filePath == null); // Block lists from a file are considered saved at the start

            gameWorld = new GameWorld(11, 11, 5, blockList);
            gameWorld.AmbientLight = new Color3f(200, 200, 200);
            UpdateState();
        }

        private BlockInformation SelectedBlock
        {
            get
            {
                int index = lstBxBlocks.SelectedIndex;
                return index >= 0 ? blocksInList[index].Block : BlockInformation.Empty;
            }
        }

        #region Overrides of Form
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            cntrlCurrentBlock.SetGameWorld(blockList, gameWorld);
            cntrlCurrentBlock.Camera.UpVector = Vector3.UnitZ;
            cntrlCurrentBlock.Camera.Location = new Vector3(59, 40, 30);
            cntrlCurrentBlock.Camera.LookAtLocation = new Vector3(59, 59, 14);
        }
        #endregion

        #region Event Handlers
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
            UpdateBlocksInList();
            UpdateState();
        }

        private void btnDeleteBlock_Click(object sender, EventArgs e)
        {
            hasUnsavedChanges = true;
            UpdateBlocksInList();
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
            btnDeleteBlock.Enabled = (lstBxBlocks.SelectedIndex != -1);
        }

        private void lstBxBlocks_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index == -1 || e.Index >= blocksInList.Count)
                return;

            using (Font font = new Font("Arial", 14, FontStyle.Regular))
            {
                int height = font.Height;
                e.Graphics.DrawString(blocksInList[e.Index].Block.BlockName, font, new SolidBrush(Color.Black), e.Bounds.X + 50, e.Bounds.Y + (e.Bounds.Height - height) / 2);
            }
        }

        private void lstBxBlocks_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (gameWorld != null)
                gameWorld[6, 6, 1] = SelectedBlock;
            cntrlCurrentBlock.RefreshLevel();

            UpdateState();
        }
        #endregion

        private void UpdateBlocksInList()
        {
            int prevSelectedIndex = lstBxBlocks.SelectedIndex;

            lstBxBlocks.BeginUpdate();
            lstBxBlocks.Items.Clear();

            blocksInList.Clear();
            foreach (BlockInformation block in blockList.AllBlocks)
            {
                blocksInList.Add(new BlockInList(block));
                lstBxBlocks.Items.Add(block);
            }
            lstBxBlocks.EndUpdate();

            if (blocksInList.Count > 0)
            {
                if (prevSelectedIndex < 0)
                    lstBxBlocks.SelectedIndex = 0;
                else
                    lstBxBlocks.SelectedIndex = (prevSelectedIndex < blocksInList.Count) ? prevSelectedIndex : blocksInList.Count - 1;
            }
        }

        private sealed class BlockInList
        {
            public readonly BlockInformation Block;

            public BlockInList(BlockInformation block)
            {
                Block = block;
            }
        }
    }
}
