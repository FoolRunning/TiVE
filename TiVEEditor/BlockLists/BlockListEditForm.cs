using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEEditor.BlockLists
{
    public partial class BlockListEditForm : Form
    {
        #region Constants
        private const float CameraMovementSpeed = (3.141592f / 200.0f);
        private const float CameraZoomSpeed = 0.004f;
        private const float CenterZ = BlockInformation.VoxelSize + (BlockInformation.VoxelSize + 1) / 2;
        #endregion

        #region Member variables
        private readonly string messageBoxCaption = "Block List Editor";
        private readonly BlockPreviewCache blockPreviewCache = new BlockPreviewCache();
        private readonly List<BlockInformation> blocksInList = new List<BlockInformation>();
        private readonly GameWorld gameWorld;
        private readonly BlockList blockList;
        private readonly string titleFormatString;

        private Point prevMouseLocation;
        private bool draggingMouse;
        private float camAngleAxisY;
        private float camDist = 25; 

        private string filePath;
        private bool hasUnsavedChanges;
        private bool ignoreNameChange;
        #endregion

        #region Constructors
        private BlockListEditForm()
        {
            InitializeComponent();
            cntrlCurrentBlock.MouseWheel += cntrlCurrentBlock_MouseWheel;
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
        /// <summary>
        /// Gets the currently selected block or the empty block if there is no selected block.
        /// </summary>
        [NotNull]
        private BlockInformation SelectedBlock
        {
            get { return (BlockInformation)lstBxBlocks.SelectedItem ?? BlockInformation.Empty; }
        }
        #endregion

        #region Overrides of Form
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            cntrlCurrentBlock.SetGameWorld(blockList, gameWorld);
            cntrlCurrentBlock.Camera.UpVector = Vector3.UnitZ;
            float centerX = cntrlCurrentBlock.GameWorld.VoxelSize.X / 2.0f;
            float centerY = cntrlCurrentBlock.GameWorld.VoxelSize.Y / 2.0f;
            cntrlCurrentBlock.Camera.FoV = (float)Math.PI / 4; // 45 degrees
            cntrlCurrentBlock.Camera.LookAtLocation = new Vector3(centerX, centerY, CenterZ);
            UpdateCameraPos();
            UpdateBlocksInList();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (hasUnsavedChanges)
            {
                DialogResult result = MessageBox.Show(this, "Do you want to save your changes before closing?",
                    messageBoxCaption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

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
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = @"C:\Programming\Utilities\MagicaVoxel\vox";
                dialog.Filter = "Voxel filess (*.vox)|*.vox|All files (*.*)|*.*";
                dialog.CheckPathExists = true;
                dialog.Multiselect = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string[] files = dialog.FileNames;
                    List<BlockInformation> blocks = new List<BlockInformation>();
                    foreach (string file in files)
                    {
                        using (BinaryReader reader = new BinaryReader(new FileStream(file, FileMode.Open)))
                        {
                            string blockName = Path.GetFileNameWithoutExtension(file);
                            if (blockList.AllBlocks.Any(b => b.BlockName == blockName))
                            {
                                DialogResult result = MessageBox.Show(this, 
                                    string.Format("Block with the name {0} already exists.\nDo you want to replace it?", blockName),
                                    messageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                if (result == DialogResult.No)
                                {
                                    blockName += "2";
                                    int num = 3;
                                    while (blockList.AllBlocks.Any(b => b.BlockName == blockName))
                                        blockName = blockName.Substring(0, blockName.Length - 1) + (num++);
                                }
                            }
                            BlockInformation block = MagicaVoxelImporter.CreateBlock(reader, blockName);
                            blocks.Add(block);
                        }
                    }

                    blockList.AddBlocks(blocks);
                    hasUnsavedChanges = true;
                    UpdateBlocksInList();
                    UpdateState();
                }
            }
        }

        private void btnDeleteBlock_Click(object sender, EventArgs e)
        {
            BlockInformation block = SelectedBlock;
            DialogResult result = MessageBox.Show(this, string.Format("Are you sure you want to delete the block: {0}?", block.BlockName), 
                messageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                blockList.RemoveBlock(block.BlockName);
                hasUnsavedChanges = true;
                UpdateBlocksInList();
                UpdateState();
            }
        }

        private void UpdateCameraPos()
        {
            float centerX = cntrlCurrentBlock.GameWorld.VoxelSize.X / 2.0f;
            float centerY = cntrlCurrentBlock.GameWorld.VoxelSize.Y / 2.0f;
            float circleX = (float)(Math.Sin(camAngleAxisY) * camDist);
            float circleY = (float)(Math.Cos(camAngleAxisY) * camDist);
            cntrlCurrentBlock.Camera.Location = new Vector3(centerX + circleX, centerY + circleY, CenterZ);
            cntrlCurrentBlock.Invalidate();
        }

        private void cntrlCurrentBlock_MouseDown(object sender, MouseEventArgs e)
        {
            prevMouseLocation = e.Location;
            draggingMouse = true;
        }

        private void cntrlCurrentBlock_MouseMove(object sender, MouseEventArgs e)
        {
            if (draggingMouse)
            {
                int deltaX = e.X - prevMouseLocation.X;
                camAngleAxisY += deltaX * CameraMovementSpeed;
                UpdateCameraPos();
            }
            prevMouseLocation = e.Location;
        }

        void cntrlCurrentBlock_MouseWheel(object sender, MouseEventArgs e)
        {
            camDist = Math.Max(Math.Min(camDist - e.Delta * CameraZoomSpeed, 30), 10);
            UpdateCameraPos();
        }

        private void cntrlCurrentBlock_MouseUp(object sender, MouseEventArgs e)
        {
            draggingMouse = false;
        }

        private void cntrlCurrentBlock_MouseLeave(object sender, EventArgs e)
        {
            draggingMouse = false;
        }

        private void lstBxBlocks_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (gameWorld != null)
                gameWorld[5, 5, 1] = SelectedBlock; // Put the block in the middle of the game world
            cntrlCurrentBlock.RefreshLevel();

            UpdateState();
        }

        private void lstBxBlocks_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index == -1 || e.Index >= blocksInList.Count)
                return;

            BlockInformation block = blocksInList[e.Index];

            e.Graphics.DrawImageUnscaled(blockPreviewCache.GetPreview(block), e.Bounds.X + 3, e.Bounds.Y + 3);

            Font font = Font;
            int height = font.Height;
            e.Graphics.DrawString(block.BlockName, font, new SolidBrush(e.ForeColor), e.Bounds.X + 50, e.Bounds.Y + (e.Bounds.Height - height) / 2);
        }

        private void cmbEffect_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void txtBlockName_TextChanged(object sender, EventArgs e)
        {
            if (ignoreNameChange)
                return;

            int prevBlockIndex = lstBxBlocks.SelectedIndex;
            BlockInformation block = SelectedBlock;
            block.BlockName = txtBlockName.Text;
            blockList.UpdateNameIndex();
            hasUnsavedChanges = true;

            // If the order of the blocks has changes with the new name, then reload the whole block list.
            blocksInList.Clear();
            blocksInList.AddRange(blockList.AllBlocks.OrderBy(b => b.BlockName));
            int newBlockIndex = blocksInList.IndexOf(block);
            if (newBlockIndex != prevBlockIndex)
                UpdateBlocksInList();
            else
                lstBxBlocks.Invalidate();
            UpdateState();
        }

        private void btnLightColor_Click(object sender, EventArgs e)
        {

        }

        private void lightLoc_ValueChanged(object sender, EventArgs e)
        {

        }
        #endregion

        #region Private helper methods
        private void UpdateState()
        {
            BlockInformation block = SelectedBlock;
            bool hasSelectedItem = (block != BlockInformation.Empty);
            bool hasLight = (block.Light != null);

            btnSaveBlockList.Enabled = hasUnsavedChanges;
            picName.Enabled = hasSelectedItem;
            txtBlockName.Enabled = hasSelectedItem;
            picLight.Enabled = hasSelectedItem;
            btnLightColor.Enabled = hasSelectedItem;
            spnLightLocX.Enabled = hasLight;
            spnLightLocY.Enabled = hasLight;
            spnLightLocZ.Enabled = hasLight;
            picEffect.Enabled = hasSelectedItem;
            cmbEffect.Enabled = hasSelectedItem;

            ignoreNameChange = true;
            txtBlockName.Text = hasSelectedItem ? block.BlockName : "";
            ignoreNameChange = false;
            btnLightColor.BackColor = hasLight ? Color.FromArgb((int)((Color4b)block.Light.Color).ToArgb()) : Color.Black;
            spnLightLocX.Value = hasLight ? block.Light.Location.X : 4;
            spnLightLocY.Value = hasLight ? block.Light.Location.Y : 4;
            spnLightLocZ.Value = hasLight ? block.Light.Location.Z : 4;

            btnDeleteBlock.Enabled = hasSelectedItem;
            string title = string.Format(titleFormatString, string.IsNullOrEmpty(filePath) ? "Unknown" : Path.GetFileNameWithoutExtension(filePath));
            Text = hasUnsavedChanges ? "* " + title : title;
        }

        private bool SaveBlockList()
        {
            if (filePath == null)
            {
                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    // TODO: Set save dialog properties
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        filePath = dialog.FileName;
                    }
                }
            }

            if (filePath == null)
                return false;

            blockList.SaveToBlockListFile(filePath);
            hasUnsavedChanges = false;
            return true;
        }

        private void UpdateBlocksInList()
        {
            BlockInformation prevSelectedBlock = SelectedBlock;
            int prevSelectedIndex = lstBxBlocks.SelectedIndex;

            lstBxBlocks.BeginUpdate();
            lstBxBlocks.Items.Clear();

            blocksInList.Clear();
            foreach (BlockInformation block in blockList.AllBlocks.OrderBy(b => b.BlockName))
            {
                blocksInList.Add(block);
                lstBxBlocks.Items.Add(block);
            }
            lstBxBlocks.EndUpdate();

            if (blocksInList.Count > 0)
            {
                // Look for the same block to re-select
                if (prevSelectedBlock != BlockInformation.Empty)
                    lstBxBlocks.SelectedItem = prevSelectedBlock;

                if (lstBxBlocks.SelectedIndex == -1)
                {
                    // Couldn't find the previously selected block, so try select the same index.
                    if (prevSelectedIndex < 0)
                        lstBxBlocks.SelectedIndex = 0;
                    else
                        lstBxBlocks.SelectedIndex = (prevSelectedIndex < blocksInList.Count) ? prevSelectedIndex : blocksInList.Count - 1;
                }
            }
            else
                lstBxBlocks_SelectedIndexChanged(null, null); // Update block display when no blocks are left
        }
        #endregion
    }
}
