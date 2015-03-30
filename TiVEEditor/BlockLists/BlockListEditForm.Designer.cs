using System.ComponentModel;
using System.Windows.Forms;
using ProdigalSoftware.TiVEEditor.Common;

namespace ProdigalSoftware.TiVEEditor.BlockLists
{
    partial class BlockListEditForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                blockPreviewCache.Dispose();
                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BlockListEditForm));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnSaveBlockList = new System.Windows.Forms.ToolStripButton();
            this.btnImportBlock = new System.Windows.Forms.ToolStripButton();
            this.btnDeleteBlock = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lstBxBlocks = new System.Windows.Forms.ListBox();
            this.cntrlCurrentBlock = new ProdigalSoftware.TiVEEditor.Common.TiVEGameControl();
            this.picName = new System.Windows.Forms.PictureBox();
            this.txtBlockName = new System.Windows.Forms.TextBox();
            this.picEffect = new System.Windows.Forms.PictureBox();
            this.cmbEffect = new System.Windows.Forms.ComboBox();
            this.btnLightColor = new System.Windows.Forms.Button();
            this.spnLightLocX = new System.Windows.Forms.NumericUpDown();
            this.spnLightLocY = new System.Windows.Forms.NumericUpDown();
            this.spnLightLocZ = new System.Windows.Forms.NumericUpDown();
            this.picLight = new System.Windows.Forms.PictureBox();
            this.toolStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picEffect)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnLightLocX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnLightLocY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnLightLocZ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLight)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnSaveBlockList,
            this.btnImportBlock,
            this.btnDeleteBlock});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(604, 35);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnSaveBlockList
            // 
            this.btnSaveBlockList.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSaveBlockList.Image = global::ProdigalSoftware.TiVEEditor.Properties.Resources.disk_return_black;
            this.btnSaveBlockList.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnSaveBlockList.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSaveBlockList.Name = "btnSaveBlockList";
            this.btnSaveBlockList.Padding = new System.Windows.Forms.Padding(2);
            this.btnSaveBlockList.Size = new System.Drawing.Size(32, 32);
            this.btnSaveBlockList.Text = "Save Changes";
            this.btnSaveBlockList.Click += new System.EventHandler(this.btnSaveBlockList_Click);
            // 
            // btnImportBlock
            // 
            this.btnImportBlock.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnImportBlock.Image = global::ProdigalSoftware.TiVEEditor.Properties.Resources.block__plus;
            this.btnImportBlock.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnImportBlock.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnImportBlock.Name = "btnImportBlock";
            this.btnImportBlock.Padding = new System.Windows.Forms.Padding(2, 0, 0, 0);
            this.btnImportBlock.Size = new System.Drawing.Size(30, 32);
            this.btnImportBlock.Text = "Import block";
            this.btnImportBlock.Click += new System.EventHandler(this.btnImportBlock_Click);
            // 
            // btnDeleteBlock
            // 
            this.btnDeleteBlock.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnDeleteBlock.Image = global::ProdigalSoftware.TiVEEditor.Properties.Resources.block__minus;
            this.btnDeleteBlock.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnDeleteBlock.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDeleteBlock.Name = "btnDeleteBlock";
            this.btnDeleteBlock.Padding = new System.Windows.Forms.Padding(2);
            this.btnDeleteBlock.Size = new System.Drawing.Size(32, 32);
            this.btnDeleteBlock.Text = "Delete selected block";
            this.btnDeleteBlock.Click += new System.EventHandler(this.btnDeleteBlock_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 7;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.lstBxBlocks, 6, 0);
            this.tableLayoutPanel1.Controls.Add(this.cntrlCurrentBlock, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.picName, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtBlockName, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.picEffect, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.cmbEffect, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.btnLightColor, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.spnLightLocX, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.spnLightLocY, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.spnLightLocZ, 4, 2);
            this.tableLayoutPanel1.Controls.Add(this.picLight, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 35);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(604, 467);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // lstBxBlocks
            // 
            this.lstBxBlocks.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstBxBlocks.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstBxBlocks.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstBxBlocks.FormattingEnabled = true;
            this.lstBxBlocks.IntegralHeight = false;
            this.lstBxBlocks.ItemHeight = 50;
            this.lstBxBlocks.Location = new System.Drawing.Point(298, 3);
            this.lstBxBlocks.Name = "lstBxBlocks";
            this.tableLayoutPanel1.SetRowSpan(this.lstBxBlocks, 5);
            this.lstBxBlocks.Size = new System.Drawing.Size(303, 461);
            this.lstBxBlocks.TabIndex = 7;
            this.lstBxBlocks.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lstBxBlocks_DrawItem);
            this.lstBxBlocks.SelectedIndexChanged += new System.EventHandler(this.lstBxBlocks_SelectedIndexChanged);
            // 
            // cntrlCurrentBlock
            // 
            this.cntrlCurrentBlock.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cntrlCurrentBlock.BackColor = System.Drawing.Color.Black;
            this.tableLayoutPanel1.SetColumnSpan(this.cntrlCurrentBlock, 6);
            this.cntrlCurrentBlock.Location = new System.Drawing.Point(3, 3);
            this.cntrlCurrentBlock.Name = "cntrlCurrentBlock";
            this.cntrlCurrentBlock.Size = new System.Drawing.Size(289, 254);
            this.cntrlCurrentBlock.TabIndex = 1;
            this.cntrlCurrentBlock.VSync = false;
            this.cntrlCurrentBlock.MouseDown += new System.Windows.Forms.MouseEventHandler(this.cntrlCurrentBlock_MouseDown);
            this.cntrlCurrentBlock.MouseLeave += new System.EventHandler(this.cntrlCurrentBlock_MouseLeave);
            this.cntrlCurrentBlock.MouseMove += new System.Windows.Forms.MouseEventHandler(this.cntrlCurrentBlock_MouseMove);
            this.cntrlCurrentBlock.MouseUp += new System.Windows.Forms.MouseEventHandler(this.cntrlCurrentBlock_MouseUp);
            // 
            // picName
            // 
            this.picName.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.picName.Image = global::ProdigalSoftware.TiVEEditor.Properties.Resources.ui_text_field_medium;
            this.picName.Location = new System.Drawing.Point(3, 268);
            this.picName.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.picName.Name = "picName";
            this.picName.Padding = new System.Windows.Forms.Padding(2);
            this.picName.Size = new System.Drawing.Size(28, 28);
            this.picName.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picName.TabIndex = 14;
            this.picName.TabStop = false;
            // 
            // txtBlockName
            // 
            this.txtBlockName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.txtBlockName, 4);
            this.txtBlockName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBlockName.Location = new System.Drawing.Point(37, 269);
            this.txtBlockName.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.txtBlockName.MaxLength = 100;
            this.txtBlockName.Name = "txtBlockName";
            this.txtBlockName.Size = new System.Drawing.Size(223, 26);
            this.txtBlockName.TabIndex = 15;
            this.txtBlockName.TextChanged += new System.EventHandler(this.txtBlockName_TextChanged);
            // 
            // picEffect
            // 
            this.picEffect.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.picEffect.Image = global::ProdigalSoftware.TiVEEditor.Properties.Resources.fire_big;
            this.picEffect.Location = new System.Drawing.Point(3, 336);
            this.picEffect.Name = "picEffect";
            this.picEffect.Padding = new System.Windows.Forms.Padding(2);
            this.picEffect.Size = new System.Drawing.Size(28, 28);
            this.picEffect.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picEffect.TabIndex = 16;
            this.picEffect.TabStop = false;
            // 
            // cmbEffect
            // 
            this.cmbEffect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.cmbEffect, 4);
            this.cmbEffect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEffect.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbEffect.FormattingEnabled = true;
            this.cmbEffect.Location = new System.Drawing.Point(37, 338);
            this.cmbEffect.Name = "cmbEffect";
            this.cmbEffect.Size = new System.Drawing.Size(223, 24);
            this.cmbEffect.TabIndex = 17;
            this.cmbEffect.SelectedIndexChanged += new System.EventHandler(this.cmbEffect_SelectedIndexChanged);
            // 
            // btnLightColor
            // 
            this.btnLightColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLightColor.BackColor = System.Drawing.Color.White;
            this.btnLightColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLightColor.Location = new System.Drawing.Point(37, 304);
            this.btnLightColor.Name = "btnLightColor";
            this.btnLightColor.Size = new System.Drawing.Size(55, 23);
            this.btnLightColor.TabIndex = 9;
            this.btnLightColor.UseVisualStyleBackColor = false;
            this.btnLightColor.Click += new System.EventHandler(this.btnLightColor_Click);
            // 
            // spnLightLocX
            // 
            this.spnLightLocX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.spnLightLocX.Location = new System.Drawing.Point(98, 306);
            this.spnLightLocX.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.spnLightLocX.Name = "spnLightLocX";
            this.spnLightLocX.Size = new System.Drawing.Size(50, 20);
            this.spnLightLocX.TabIndex = 10;
            this.spnLightLocX.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.spnLightLocX.ValueChanged += new System.EventHandler(this.lightLoc_ValueChanged);
            // 
            // spnLightLocY
            // 
            this.spnLightLocY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.spnLightLocY.Location = new System.Drawing.Point(154, 306);
            this.spnLightLocY.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.spnLightLocY.Name = "spnLightLocY";
            this.spnLightLocY.Size = new System.Drawing.Size(50, 20);
            this.spnLightLocY.TabIndex = 11;
            this.spnLightLocY.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.spnLightLocY.ValueChanged += new System.EventHandler(this.lightLoc_ValueChanged);
            // 
            // spnLightLocZ
            // 
            this.spnLightLocZ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.spnLightLocZ.Location = new System.Drawing.Point(210, 306);
            this.spnLightLocZ.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.spnLightLocZ.Name = "spnLightLocZ";
            this.spnLightLocZ.Size = new System.Drawing.Size(50, 20);
            this.spnLightLocZ.TabIndex = 12;
            this.spnLightLocZ.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.spnLightLocZ.ValueChanged += new System.EventHandler(this.lightLoc_ValueChanged);
            // 
            // picLight
            // 
            this.picLight.Image = global::ProdigalSoftware.TiVEEditor.Properties.Resources.light_bulb;
            this.picLight.Location = new System.Drawing.Point(3, 302);
            this.picLight.Name = "picLight";
            this.picLight.Padding = new System.Windows.Forms.Padding(2);
            this.picLight.Size = new System.Drawing.Size(28, 28);
            this.picLight.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picLight.TabIndex = 18;
            this.picLight.TabStop = false;
            // 
            // BlockListEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(604, 502);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.toolStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "BlockListEditForm";
            this.Text = "Block List Editor - {0}";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picEffect)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnLightLocX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnLightLocY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spnLightLocZ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLight)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ToolStrip toolStrip1;
        private ToolStripButton btnImportBlock;
        private ToolStripButton btnDeleteBlock;
        private ToolStripButton btnSaveBlockList;
        private TiVEGameControl cntrlCurrentBlock;
        private TableLayoutPanel tableLayoutPanel1;
        private ListBox lstBxBlocks;
        private Button btnLightColor;
        private NumericUpDown spnLightLocX;
        private NumericUpDown spnLightLocY;
        private NumericUpDown spnLightLocZ;
        private PictureBox picName;
        private TextBox txtBlockName;
        private PictureBox picEffect;
        private ComboBox cmbEffect;
        private PictureBox picLight;
    }
}