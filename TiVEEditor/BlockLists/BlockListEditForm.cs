﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Cyotek.Windows.Forms;
using JetBrains.Annotations;
using ProdigalSoftware.TiVE.RenderSystem.World;
using ProdigalSoftware.TiVEEditor.Importers;
using ProdigalSoftware.TiVEEditor.Properties;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVEEditor.BlockLists
{
    public partial class BlockListEditForm : Form
    {
        #region Constants
        private const int WorldSize = 21;
        private const int WorldCenter = WorldSize / 2;
        private const float CameraMovementSpeed = (3.141592f / 200.0f);
        private const float CameraMaxDist = 50;
        private const float CameraMinDist = 10;
        private const float CameraZoomSpeed = 0.04f;
        private const float CenterZ = BlockLOD32.VoxelSize + (BlockLOD32.VoxelSize + 1) / 2;
        #endregion

        #region Member variables
        private static readonly Settings settings = Settings.Default;
        private readonly string messageBoxCaption = "Block List Editor";
        private readonly BlockPreviewCache blockPreviewCache = new BlockPreviewCache();
        private readonly List<Block> blocksInList = new List<Block>();
        private readonly GameWorld gameWorld;
        //private readonly BlockList blockList;
        private readonly string titleFormatString;

        private Point prevMouseLocation;
        private bool draggingMouse;
        private float camAngleAxisY = (float)Math.PI * 5.0f / 6.0f;
        private float camDist = 25; 

        private string filePath;
        private bool hasUnsavedChanges;
        private bool ignoreValueChange;
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

            lstBxBlocks.ItemHeight = BlockPreviewCache.PreviewImageSize + 6;
            spnLightLocX.Maximum = BlockLOD32.VoxelSize - 1;
            spnLightLocY.Maximum = BlockLOD32.VoxelSize - 1;
            spnLightLocZ.Maximum = BlockLOD32.VoxelSize - 1;

            //blockList = BlockList.FromFile(filePath) ?? new BlockList();
            gameWorld = new GameWorld(WorldSize, WorldSize, ChunkComponent.BlockSize);

            Random random = new Random();
            Block[] floorBlocks = new Block[10];
            for (int i = 0; i < floorBlocks.Length; i++)
            {
                floorBlocks[i] = new Block("Floor" + i);
                for (int x = 0; x < BlockLOD32.VoxelSize; x++)
                {
                    for (int y = 0; y < BlockLOD32.VoxelSize; y++)
                        floorBlocks[i].LOD32[x, y, BlockLOD32.VoxelSize - 1] = (Voxel)((uint)random.Next(0xFFFFFF) | 0xFF000000);
                }

                for (int s = 0; s < BlockLOD32.VoxelSize; s++)
                {
                    floorBlocks[i].LOD32[s, 0, BlockLOD32.VoxelSize - 1] = Voxel.White;
                    floorBlocks[i].LOD32[s, BlockLOD32.VoxelSize - 1, BlockLOD32.VoxelSize - 1] = Voxel.White;
                    floorBlocks[i].LOD32[0, s, BlockLOD32.VoxelSize - 1] = Voxel.White;
                    floorBlocks[i].LOD32[BlockLOD32.VoxelSize - 1, s, BlockLOD32.VoxelSize - 1] = Voxel.White;
                }

                floorBlocks[i].GenerateLODLevels();
            }
            
            //for (int x = 0; x < gameWorld.BlockSize.X; x++)
            //{
            //    for (int y = 0; y < gameWorld.BlockSize.Y; y++)
            //        gameWorld[x, y, 0] = floorBlocks[random.Next(floorBlocks.Length)];
            //}

            UpdateState();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the currently selected block or the empty block if there is no selected block.
        /// </summary>
        [NotNull]
        private Block SelectedBlock
        {
            get { return (Block)lstBxBlocks.SelectedItem ?? Block.Empty; }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Lets the user choose a file for a block list (either for saving or loading). 
        /// <para>Returns the full path to the chosen file or null if the user cancels.</para>
        /// </summary>
        [CanBeNull]
        public static string ChooseBlockListFile(Form dialogOwner, bool forSave)
        {
            using (FileDialog dialog = forSave ? (FileDialog)new SaveFileDialog() : new OpenFileDialog())
            {
                if (forSave)
                {
                    SaveFileDialog saveDialog = (SaveFileDialog)dialog;
                    saveDialog.OverwritePrompt = true;
                    saveDialog.FileName = "NewBlockList";
                    saveDialog.ValidateNames = true;
                    saveDialog.Title = "Save Block List File";
                }
                else
                {
                    OpenFileDialog openDialog = (OpenFileDialog)dialog;
                    openDialog.Multiselect = false;
                    openDialog.CheckFileExists = true;
                    openDialog.Title = "Open Block List File";
                }
                dialog.InitialDirectory = !string.IsNullOrEmpty(settings.BlockEditorBlockListLastDir) ? 
                    settings.BlockEditorBlockListLastDir : Environment.CurrentDirectory;
                //dialog.Filter = string.Format("Block List Files ({0})|{0}", "*" + BlockList.FileExtension);
                if (dialog.ShowDialog(dialogOwner) == DialogResult.OK)
                {
                    settings.BlockEditorBlockListLastDir = Path.GetDirectoryName(dialog.FileName);
                    settings.Save();
                    return dialog.FileName;
                }
            }
            return null;
        }
        #endregion

        #region Overrides of Form
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            //cntrlCurrentBlock.SetGameWorld(blockList, gameWorld);
            cntrlCurrentBlock.Scene.AmbientLight = new Color3f(230, 230, 230);
            cntrlCurrentBlock.Camera.UpVector = Vector3f.UnitZ;
            float centerX = cntrlCurrentBlock.GameWorld.VoxelSize32.X / 2.0f;
            float centerY = cntrlCurrentBlock.GameWorld.VoxelSize32.Y / 2.0f;
            cntrlCurrentBlock.Camera.FieldOfView = (float)Math.PI / 4; // 45 degrees
            cntrlCurrentBlock.Camera.LookAtLocation = new Vector3f(centerX, centerY, CenterZ);
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
                dialog.InitialDirectory = !string.IsNullOrEmpty(settings.BlockEditorImportLastDir) ? settings.BlockEditorImportLastDir : Environment.CurrentDirectory;
                dialog.Filter = "Voxel files (*.vox)|*.vox|All files (*.*)|*.*";
                dialog.CheckPathExists = true;
                dialog.Multiselect = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string[] files = dialog.FileNames;
                    List<Block> blocks = new List<Block>();
                    foreach (string file in files)
                    {
                        string blockName = Path.GetFileNameWithoutExtension(file);
                        //if (blockList.AllBlocks.Any(b => b.Name == blockName))
                        //{
                        //    DialogResult result = MessageBox.Show(this,
                        //        string.Format("Block with the name {0} already exists.\n\nDo you want to replace it?\n(Clicking new will create a new name)", blockName),
                        //        messageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        //    if (result == DialogResult.No)
                        //    {
                        //        blockName += "2";
                        //        int num = 3;
                        //        while (blockList.AllBlocks.Any(b => b.Name == blockName))
                        //            blockName = blockName.Substring(0, blockName.Length - 1) + (num++);
                        //    }
                        //}

                        blocks.Add(MagicaVoxelImporter.CreateBlock(file, blockName));
                    }
                    settings.BlockEditorImportLastDir = Path.GetDirectoryName(dialog.FileName);
                    settings.Save();

                    //blockList.AddBlocks(blocks);
                    hasUnsavedChanges = true;
                    UpdateBlocksInList();
                    UpdateState();
                }
            }
        }

        private void btnDeleteBlock_Click(object sender, EventArgs e)
        {
            Block block = SelectedBlock;
            DialogResult result = MessageBox.Show(this, string.Format("Are you sure you want to delete the block: {0}?", block.Name), 
                messageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                //blockList.RemoveBlock(block.Name);
                hasUnsavedChanges = true;
                UpdateBlocksInList();
                UpdateState();
            }
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
            camDist = Math.Max(Math.Min(camDist - e.Delta * CameraZoomSpeed, CameraMaxDist), CameraMinDist);
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
            Block newBlock = SelectedBlock;
            if (gameWorld != null)
                gameWorld[WorldCenter, WorldCenter, 1] = newBlock; // Put the block in the middle of the game world

            cntrlCurrentBlock.Scene.AmbientLight = newBlock.HasComponent<LightComponent>() ? new Color3f(20, 20, 20) : new Color3f(230, 230, 230);
            cntrlCurrentBlock.RefreshLevel(true);

            UpdateState();
        }

        private void lstBxBlocks_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index == -1 || e.Index >= blocksInList.Count)
                return;

            Block block = blocksInList[e.Index];

            e.Graphics.DrawImageUnscaled(blockPreviewCache.GetPreview(block), e.Bounds.X + 3, e.Bounds.Y + 3);

            Font font = lstBxBlocks.Font;
            int height = font.Height;
            e.Graphics.DrawString(block.Name, font, new SolidBrush(e.ForeColor), 
                e.Bounds.X + BlockPreviewCache.PreviewImageSize + 4, e.Bounds.Y + (e.Bounds.Height - height) / 2);
        }

        private void txtBlockName_TextChanged(object sender, EventArgs e)
        {
            if (ignoreValueChange)
                return;

            int prevBlockIndex = lstBxBlocks.SelectedIndex;
            Block block = SelectedBlock;
            //block.SetName(txtBlockName.Text);
            //blockList.UpdateNameIndex();
            hasUnsavedChanges = true;

            // If the order of the blocks has changes with the new name, then reload the whole block list.
            blocksInList.Clear();
            //blocksInList.AddRange(blockList.AllBlocks);
            int newBlockIndex = blocksInList.IndexOf(block);
            if (newBlockIndex != prevBlockIndex)
                UpdateBlocksInList();
            else
                lstBxBlocks.Invalidate();
            UpdateState();
        }

        private void btnLightColor_Click(object sender, EventArgs e)
        {
            using (ColorPickerDialog dialog = new ColorPickerDialog())
            {
                Block block = SelectedBlock;
                LightComponent light = block.GetComponent<LightComponent>();
                dialog.Color = light != null ? (Color)light.Color : Color.Black;
                dialog.ShowAlphaChannel = false;

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    if (dialog.Color.ToArgb() == Color.Black.ToArgb())
                        block.RemoveComponent<LightComponent>();
                    else
                    {
                        block.AddComponent(new LightComponent(new Vector3b((byte)spnLightLocX.Value, (byte)spnLightLocY.Value, (byte)spnLightLocZ.Value),
                            (Color3f)dialog.Color, 10));
                    }

                    hasUnsavedChanges = true;
                    UpdateState();
                    cntrlCurrentBlock.RefreshLevel(true);
                }
            }
        }

        private void lightLoc_ValueChanged(object sender, EventArgs e)
        {
            if (ignoreValueChange)
                return;

            Block block = SelectedBlock;
            block.RemoveComponent<LightComponent>();
            block.AddComponent(new LightComponent(new Vector3b((byte)spnLightLocX.Value, (byte)spnLightLocY.Value, (byte)spnLightLocZ.Value),
                (Color3f)btnLightColor.BackColor, 10));;

            hasUnsavedChanges = true;
            UpdateState();
            cntrlCurrentBlock.RefreshLevel(true);
        }

        private void cmbEffect_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (ignoreValueChange)
            //    return;

            // TODO: Implement effects
        }
        #endregion

        #region Private helper methods
        private void UpdateState()
        {
            Block block = SelectedBlock;
            bool hasSelectedItem = (block != Block.Empty);
            LightComponent light = block.GetComponent<LightComponent>();
            bool hasLight = light != null;

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

            ignoreValueChange = true;
            txtBlockName.Text = hasSelectedItem ? block.Name : "";
            btnLightColor.BackColor = hasLight ? Color.FromArgb((int)((Color4b)light.Color).ToArgb()) : Color.Black;
            spnLightLocX.Value = hasLight ? light.Location.X : BlockLOD32.VoxelSize / 2;
            spnLightLocY.Value = hasLight ? light.Location.Y : BlockLOD32.VoxelSize / 2;
            spnLightLocZ.Value = hasLight ? light.Location.Z : BlockLOD32.VoxelSize / 2;
            ignoreValueChange = false;

            btnDeleteBlock.Enabled = hasSelectedItem;
            string title = string.Format(titleFormatString, string.IsNullOrEmpty(filePath) ? "Unknown" : Path.GetFileNameWithoutExtension(filePath));
            Text = hasUnsavedChanges ? "* " + title : title;
        }

        private void UpdateCameraPos()
        {
            float centerX = cntrlCurrentBlock.GameWorld.VoxelSize32.X / 2.0f;
            float centerY = cntrlCurrentBlock.GameWorld.VoxelSize32.Y / 2.0f;
            float circleX = (float)(Math.Sin(camAngleAxisY) * camDist);
            float circleY = (float)(Math.Cos(camAngleAxisY) * camDist);
            cntrlCurrentBlock.Camera.Location = new Vector3f(centerX + circleX, centerY + circleY, CenterZ + 30);
            cntrlCurrentBlock.Invalidate();
        }

        private bool SaveBlockList()
        {
            if (filePath == null)
                filePath = ChooseBlockListFile(this, true);

            if (filePath == null)
                return false;

            //blockList.SaveToBlockListFile(filePath);
            hasUnsavedChanges = false;
            return true;
        }

        private void UpdateBlocksInList()
        {
            Block prevSelectedBlock = SelectedBlock;
            int prevSelectedIndex = lstBxBlocks.SelectedIndex;

            lstBxBlocks.BeginUpdate();
            lstBxBlocks.Items.Clear();

            blocksInList.Clear();
            //foreach (Block block in blockList.AllBlocks)
            //{
            //    blocksInList.Add(block);
            //    lstBxBlocks.Items.Add(block);
            //}
            lstBxBlocks.EndUpdate();

            if (blocksInList.Count > 0)
            {
                // Look for the same block to re-select
                if (prevSelectedBlock != Block.Empty)
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
