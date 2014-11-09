namespace ProdigalSoftware.TiVEEditor
{
    partial class MapListEditForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapListEditForm));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnAddMap = new System.Windows.Forms.ToolStripButton();
            this.btnDeleteMap = new System.Windows.Forms.ToolStripButton();
            this.btnResizeMap = new System.Windows.Forms.ToolStripButton();
            this.btnSaveMap = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnSaveMap,
            this.btnAddMap,
            this.btnDeleteMap,
            this.btnResizeMap});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(784, 35);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnAddMap
            // 
            this.btnAddMap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAddMap.Image = global::ProdigalSoftware.TiVEEditor.Properties.Resources.map__plus;
            this.btnAddMap.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnAddMap.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAddMap.Name = "btnAddMap";
            this.btnAddMap.Padding = new System.Windows.Forms.Padding(2);
            this.btnAddMap.Size = new System.Drawing.Size(32, 32);
            this.btnAddMap.Text = "Add New Map";
            this.btnAddMap.Click += new System.EventHandler(this.btnAddMap_Click);
            // 
            // btnDeleteMap
            // 
            this.btnDeleteMap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnDeleteMap.Image = global::ProdigalSoftware.TiVEEditor.Properties.Resources.map__minus;
            this.btnDeleteMap.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnDeleteMap.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDeleteMap.Name = "btnDeleteMap";
            this.btnDeleteMap.Padding = new System.Windows.Forms.Padding(2);
            this.btnDeleteMap.Size = new System.Drawing.Size(32, 32);
            this.btnDeleteMap.Text = "Delete Map";
            this.btnDeleteMap.Click += new System.EventHandler(this.btnDeleteMap_Click);
            // 
            // btnResizeMap
            // 
            this.btnResizeMap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnResizeMap.Image = global::ProdigalSoftware.TiVEEditor.Properties.Resources.map_resize;
            this.btnResizeMap.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnResizeMap.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnResizeMap.Name = "btnResizeMap";
            this.btnResizeMap.Padding = new System.Windows.Forms.Padding(2);
            this.btnResizeMap.Size = new System.Drawing.Size(32, 32);
            this.btnResizeMap.Text = "Resize Map";
            this.btnResizeMap.Click += new System.EventHandler(this.btnResizeMap_Click);
            // 
            // btnSaveMap
            // 
            this.btnSaveMap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSaveMap.Image = global::ProdigalSoftware.TiVEEditor.Properties.Resources.disk_return_black;
            this.btnSaveMap.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnSaveMap.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSaveMap.Name = "btnSaveMap";
            this.btnSaveMap.Padding = new System.Windows.Forms.Padding(2);
            this.btnSaveMap.Size = new System.Drawing.Size(32, 32);
            this.btnSaveMap.Text = "Save Changes";
            this.btnSaveMap.Click += new System.EventHandler(this.btnSaveMap_Click);
            // 
            // MapListEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 462);
            this.Controls.Add(this.toolStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MapListEditForm";
            this.Text = "Map List Editor - {0}";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnAddMap;
        private System.Windows.Forms.ToolStripButton btnDeleteMap;
        private System.Windows.Forms.ToolStripButton btnResizeMap;
        private System.Windows.Forms.ToolStripButton btnSaveMap;
    }
}