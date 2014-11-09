namespace ProdigalSoftware.TiVEEditor.BlockLists
{
    partial class BlockListEditForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
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
            this.btnBlockProperties = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.cntrlCurrentBlock = new ProdigalSoftware.TiVEEditor.Common.TiVEGameControl();
            this.sldrAmbientLightIntensity = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.lstBxBlocks = new System.Windows.Forms.ListBox();
            this.toolStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sldrAmbientLightIntensity)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnSaveBlockList,
            this.btnImportBlock,
            this.btnDeleteBlock,
            this.btnBlockProperties});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(638, 35);
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
            // btnBlockProperties
            // 
            this.btnBlockProperties.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnBlockProperties.Image = global::ProdigalSoftware.TiVEEditor.Properties.Resources.block__pencil;
            this.btnBlockProperties.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnBlockProperties.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnBlockProperties.Name = "btnBlockProperties";
            this.btnBlockProperties.Padding = new System.Windows.Forms.Padding(2);
            this.btnBlockProperties.Size = new System.Drawing.Size(32, 32);
            this.btnBlockProperties.Text = "Edit properties of current block";
            this.btnBlockProperties.Click += new System.EventHandler(this.btnBlockProperties_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.cntrlCurrentBlock, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.sldrAmbientLightIntensity, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lstBxBlocks, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 35);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(638, 469);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // cntrlCurrentBlock
            // 
            this.cntrlCurrentBlock.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cntrlCurrentBlock.BackColor = System.Drawing.Color.Black;
            this.cntrlCurrentBlock.Location = new System.Drawing.Point(3, 3);
            this.cntrlCurrentBlock.Name = "cntrlCurrentBlock";
            this.tableLayoutPanel1.SetRowSpan(this.cntrlCurrentBlock, 2);
            this.cntrlCurrentBlock.Size = new System.Drawing.Size(383, 463);
            this.cntrlCurrentBlock.TabIndex = 1;
            this.cntrlCurrentBlock.VSync = false;
            // 
            // sldrAmbientLightIntensity
            // 
            this.sldrAmbientLightIntensity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.sldrAmbientLightIntensity.Location = new System.Drawing.Point(469, 3);
            this.sldrAmbientLightIntensity.Maximum = 255;
            this.sldrAmbientLightIntensity.Name = "sldrAmbientLightIntensity";
            this.sldrAmbientLightIntensity.Size = new System.Drawing.Size(166, 45);
            this.sldrAmbientLightIntensity.TabIndex = 5;
            this.sldrAmbientLightIntensity.TickFrequency = 10;
            this.sldrAmbientLightIntensity.TickStyle = System.Windows.Forms.TickStyle.Both;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(392, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Ambient Light";
            // 
            // lstBxBlocks
            // 
            this.lstBxBlocks.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.lstBxBlocks, 2);
            this.lstBxBlocks.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstBxBlocks.FormattingEnabled = true;
            this.lstBxBlocks.IntegralHeight = false;
            this.lstBxBlocks.ItemHeight = 50;
            this.lstBxBlocks.Location = new System.Drawing.Point(392, 54);
            this.lstBxBlocks.Name = "lstBxBlocks";
            this.lstBxBlocks.Size = new System.Drawing.Size(243, 412);
            this.lstBxBlocks.TabIndex = 7;
            this.lstBxBlocks.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lstBxBlocks_DrawItem);
            this.lstBxBlocks.SelectedIndexChanged += new System.EventHandler(this.lstBxBlocks_SelectedIndexChanged);
            // 
            // BlockListEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(638, 504);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.toolStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(250, 180);
            this.Name = "BlockListEditForm";
            this.Text = "Block List Editor - {0}";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sldrAmbientLightIntensity)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnImportBlock;
        private System.Windows.Forms.ToolStripButton btnDeleteBlock;
        private System.Windows.Forms.ToolStripButton btnBlockProperties;
        private System.Windows.Forms.ToolStripButton btnSaveBlockList;
        private Common.TiVEGameControl cntrlCurrentBlock;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TrackBar sldrAmbientLightIntensity;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox lstBxBlocks;
    }
}