using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Cyotek.Windows.Forms;
using JetBrains.Annotations;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Lighting;
using ProdigalSoftware.Utils;

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
        private const float CenterZ = BlockInformation.VoxelSize + (BlockInformation.VoxelSize + 1) / 2;
        #endregion

        #region Member variables
        private static readonly Properties.Settings settings = Properties.Settings.Default;
        private readonly string messageBoxCaption = "Block List Editor";
        private readonly BlockPreviewCache blockPreviewCache = new BlockPreviewCache();
        private readonly List<BlockInformation> blocksInList = new List<BlockInformation>();
        private readonly GameWorld gameWorld;
        private readonly BlockList blockList;
        private readonly string titleFormatString;
        private readonly BlockInformation[] floorBlocks = new BlockInformation[10];

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
            spnLightLocX.Maximum = BlockInformation.VoxelSize - 1;
            spnLightLocY.Maximum = BlockInformation.VoxelSize - 1;
            spnLightLocZ.Maximum = BlockInformation.VoxelSize - 1;

            blockList = BlockList.FromFile(filePath) ?? new BlockList();
            gameWorld = new GameWorld(WorldSize, WorldSize, 5);

            Random random = new Random();
            for (int i = 0; i < floorBlocks.Length; i++)
            {
                floorBlocks[i] = new BlockInformation("Floor" + i);
                for (int x = 0; x < BlockInformation.VoxelSize; x++)
                {
                    for (int y = 0; y < BlockInformation.VoxelSize; y++)
                        floorBlocks[i][x, y, BlockInformation.VoxelSize - 1] = (uint)(random.Next(0xFFFFFF) | 0xFF000000);
                }

                for (int s = 0; s < BlockInformation.VoxelSize; s++)
                {
                    floorBlocks[i][s, 0, BlockInformation.VoxelSize - 1] = 0xFFFFFFFF;
                    floorBlocks[i][s, BlockInformation.VoxelSize - 1, BlockInformation.VoxelSize - 1] = 0xFFFFFFFF;
                    floorBlocks[i][0, s, BlockInformation.VoxelSize - 1] = 0xFFFFFFFF;
                    floorBlocks[i][BlockInformation.VoxelSize - 1, s, BlockInformation.VoxelSize - 1] = 0xFFFFFFFF;
                }
            }
            
            for (int x = 0; x < gameWorld.BlockSize.X; x++)
            {
                for (int y = 0; y < gameWorld.BlockSize.Y; y++)
                    gameWorld[x, y, 0] = floorBlocks[random.Next(floorBlocks.Length)];
            }

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
                dialog.Filter = string.Format("Block List Files ({0})|{0}", "*." + BlockList.FileExtension);
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
            
            cntrlCurrentBlock.SetGameWorld(blockList, gameWorld);
            cntrlCurrentBlock.LightProvider.AmbientLight = new Color3f(230, 230, 230);
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
                dialog.InitialDirectory = !string.IsNullOrEmpty(settings.BlockEditorImportLastDir) ? settings.BlockEditorImportLastDir : Environment.CurrentDirectory;
                dialog.Filter = "Voxel files (*.vox)|*.vox|All files (*.*)|*.*";
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
                    settings.BlockEditorImportLastDir = Path.GetDirectoryName(dialog.FileName);
                    settings.Save();

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
            BlockInformation previouseBlock = BlockInformation.Empty;
            BlockInformation newBlock = SelectedBlock;
            if (gameWorld != null)
            {
                previouseBlock = gameWorld[WorldCenter, WorldCenter, 1];
                gameWorld[WorldCenter, WorldCenter, 1] = newBlock; // Put the block in the middle of the game world
            }

            cntrlCurrentBlock.LightProvider.AmbientLight = newBlock.Light != null ? new Color3f(20, 20, 20) : new Color3f(230, 230, 230);
            cntrlCurrentBlock.RefreshLevel(previouseBlock.Light != null || newBlock.Light != null);

            UpdateState();
        }

        private void lstBxBlocks_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index == -1 || e.Index >= blocksInList.Count)
                return;

            BlockInformation block = blocksInList[e.Index];

            e.Graphics.DrawImageUnscaled(blockPreviewCache.GetPreview(block), e.Bounds.X + 3, e.Bounds.Y + 3);

            Font font = lstBxBlocks.Font;
            int height = font.Height;
            e.Graphics.DrawString(block.BlockName, font, new SolidBrush(e.ForeColor), 
                e.Bounds.X + BlockPreviewCache.PreviewImageSize + 4, e.Bounds.Y + (e.Bounds.Height - height) / 2);
        }

        private void txtBlockName_TextChanged(object sender, EventArgs e)
        {
            if (ignoreValueChange)
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
            using (ColorPickerDialog dialog = new ColorPickerDialog())
            {
                BlockInformation block = SelectedBlock;
                dialog.Color = block.Light != null ? (Color)block.Light.Color : Color.Black;
                dialog.ShowAlphaChannel = false;

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    if (dialog.Color.ToArgb() == Color.Black.ToArgb())
                        block.Light = null;
                    else
                    {
                        block.Light = new PointLight(new Vector3b((byte)spnLightLocX.Value, (byte)spnLightLocY.Value, (byte)spnLightLocZ.Value),
                            (Color3f)dialog.Color, 0.001f);
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

            BlockInformation block = SelectedBlock;
            block.Light = new PointLight(new Vector3b((byte)spnLightLocX.Value, (byte)spnLightLocY.Value, (byte)spnLightLocZ.Value),
                (Color3f)btnLightColor.BackColor, 0.001f);

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

            ignoreValueChange = true;
            txtBlockName.Text = hasSelectedItem ? block.BlockName : "";
            btnLightColor.BackColor = hasLight ? Color.FromArgb((int)((Color4b)block.Light.Color).ToArgb()) : Color.Black;
            spnLightLocX.Value = hasLight ? block.Light.Location.X : BlockInformation.VoxelSize / 2;
            spnLightLocY.Value = hasLight ? block.Light.Location.Y : BlockInformation.VoxelSize / 2;
            spnLightLocZ.Value = hasLight ? block.Light.Location.Z : BlockInformation.VoxelSize / 2;
            ignoreValueChange = false;

            btnDeleteBlock.Enabled = hasSelectedItem;
            string title = string.Format(titleFormatString, string.IsNullOrEmpty(filePath) ? "Unknown" : Path.GetFileNameWithoutExtension(filePath));
            Text = hasUnsavedChanges ? "* " + title : title;
        }

        private void UpdateCameraPos()
        {
            float centerX = cntrlCurrentBlock.GameWorld.VoxelSize.X / 2.0f;
            float centerY = cntrlCurrentBlock.GameWorld.VoxelSize.Y / 2.0f;
            float circleX = (float)(Math.Sin(camAngleAxisY) * camDist);
            float circleY = (float)(Math.Cos(camAngleAxisY) * camDist);
            cntrlCurrentBlock.Camera.Location = new Vector3(centerX + circleX, centerY + circleY, CenterZ + 30);
            cntrlCurrentBlock.Invalidate();
        }

        private bool SaveBlockList()
        {
            if (filePath == null)
                filePath = ChooseBlockListFile(this, true);

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
