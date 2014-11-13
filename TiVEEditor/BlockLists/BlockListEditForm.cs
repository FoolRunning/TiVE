using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEEditor.BlockLists
{
    public partial class BlockListEditForm : Form
    {
        #region Member variables
        private readonly List<BlockInList> blocksInList = new List<BlockInList>();
        private readonly GameWorld gameWorld;
        private readonly BlockList blockList;
        private readonly string titleFormatString;
        private string filePath;
        private bool hasUnsavedChanges;
        #endregion

        #region Constructors
        private BlockListEditForm()
        {
            InitializeComponent();
        }

        public BlockListEditForm(string filePath) : this()
        {
            this.filePath = filePath;
            titleFormatString = Text;

            blockList = filePath != null ? BlockList.FromBlockListFile(filePath) : new BlockList();
            gameWorld = new GameWorld(11, 11, 5, blockList);
            gameWorld.AmbientLight = new Color3f(200, 200, 200);
            UpdateState();
        }
        #endregion

        #region Properties
        private BlockInformation SelectedBlock
        {
            get
            {
                int index = lstBxBlocks.SelectedIndex;
                return index >= 0 ? blocksInList[index].Block : BlockInformation.Empty;
            }
        }
        #endregion

        #region Overrides of Form
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            cntrlCurrentBlock.SetGameWorld(blockList, gameWorld);
            cntrlCurrentBlock.Camera.UpVector = Vector3.UnitZ;
            cntrlCurrentBlock.Camera.Location = new Vector3(59, 40, 30);
            cntrlCurrentBlock.Camera.LookAtLocation = new Vector3(59, 59, 14);
            UpdateBlocksInList();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (hasUnsavedChanges)
            {
                DialogResult result = MessageBox.Show(this, "Do you want to save your changes before closing?",
                    "Block List Editor", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                    e.Cancel = !SaveBlockList();
                else if (result == DialogResult.Cancel)
                    e.Cancel = true;
            }
            base.OnClosing(e);
        }
        #endregion

        #region Event Handlers
        private void btnSaveBlockList_Click(object sender, EventArgs e)
        {
            SaveBlockList();
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

        private void lstBxBlocks_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index == -1 || e.Index >= blocksInList.Count)
                return;

            BlockInList bil = blocksInList[e.Index];

            e.Graphics.DrawImageUnscaled(bil.Preview, e.Bounds.X + 3, e.Bounds.Y + 3);

            using (Font font = new Font("Arial", 14, FontStyle.Regular))
            {
                int height = font.Height;
                e.Graphics.DrawString(bil.Block.BlockName, font, new SolidBrush(e.ForeColor), e.Bounds.X + 50, e.Bounds.Y + (e.Bounds.Height - height) / 2);
            }
            
            if (lstBxBlocks.ContainsFocus)
                e.DrawFocusRectangle();
        }

        private void lstBxBlocks_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (gameWorld != null)
                gameWorld[6, 6, 1] = SelectedBlock;
            cntrlCurrentBlock.RefreshLevel();

            UpdateState();
        }
        #endregion

        #region Private helper methods
        private void UpdateState()
        {
            btnSaveBlockList.Enabled = hasUnsavedChanges;
            btnDeleteBlock.Enabled = (lstBxBlocks.SelectedIndex != -1);
            Text = string.Format(titleFormatString, string.IsNullOrEmpty(filePath) ? "Unknown" : Path.GetFileNameWithoutExtension(filePath));
        }

        private bool SaveBlockList()
        {
            if (filePath == null)
            {
                // TODO: Prompt user for save location

            }

            if (filePath == null)
                return false;

            blockList.SaveToBlockListFile(filePath);
            hasUnsavedChanges = false;
            return true;
        }

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
        #endregion

        #region BlockInList class
        private sealed class BlockInList
        {
            public readonly BlockInformation Block;
            public readonly Image Preview;

            public BlockInList(BlockInformation block)
            {
                Block = block;

                Bitmap bitmap = new Bitmap(44, 44, PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    using (Brush brush = new SolidBrush(Color.Black))
                        g.FillRectangle(brush, 0, 0, 44, 44);

                    for (int y = BlockInformation.VoxelSize - 1; y >= 0; y--)
                    {
                        for (int x = 0; x < BlockInformation.VoxelSize; x++)
                        {
                            for (int z = 0; z < BlockInformation.VoxelSize; z++)
                            {
                                int color = (int)block[x, y, z];
                                if (color == 0)
                                    continue;

                                using (Brush brush = new SolidBrush(Color.FromArgb(color)))
                                    g.FillRectangle(brush, x * 2 + 18 - BlockInformation.VoxelSize + y, 28 - y - z * 2 + x, 3, 3);
                            }
                        }
                    }
                }
                Preview = bitmap;
            }
        }
        #endregion
    }
}
