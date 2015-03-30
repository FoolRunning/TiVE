using System.ComponentModel;
using System.Windows.Forms;

namespace ProdigalSoftware.TiVEEditor
{
    partial class TiVEEditorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TiVEEditorForm));
            this.mainMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newBlockListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openMapListMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openScriptListMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openBlockListMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.status = new System.Windows.Forms.StatusStrip();
            this.mainMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu
            // 
            this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.mainMenu.Location = new System.Drawing.Point(0, 0);
            this.mainMenu.Name = "mainMenu";
            this.mainMenu.Size = new System.Drawing.Size(984, 24);
            this.mainMenu.TabIndex = 0;
            this.mainMenu.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newMapToolStripMenuItem,
            this.newScriptToolStripMenuItem,
            this.newBlockListToolStripMenuItem});
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.newToolStripMenuItem.Text = "New";
            // 
            // newMapToolStripMenuItem
            // 
            this.newMapToolStripMenuItem.Image = global::ProdigalSoftware.TiVEEditor.Properties.Resources.map;
            this.newMapToolStripMenuItem.Name = "newMapToolStripMenuItem";
            this.newMapToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.newMapToolStripMenuItem.Text = "Map List...";
            this.newMapToolStripMenuItem.Click += new System.EventHandler(this.newMapToolStripMenuItem_Click);
            // 
            // newScriptToolStripMenuItem
            // 
            this.newScriptToolStripMenuItem.Image = global::ProdigalSoftware.TiVEEditor.Properties.Resources.script;
            this.newScriptToolStripMenuItem.Name = "newScriptToolStripMenuItem";
            this.newScriptToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.newScriptToolStripMenuItem.Text = "Script List...";
            this.newScriptToolStripMenuItem.Click += new System.EventHandler(this.newScriptToolStripMenuItem_Click);
            // 
            // newBlockListToolStripMenuItem
            // 
            this.newBlockListToolStripMenuItem.Image = global::ProdigalSoftware.TiVEEditor.Properties.Resources.block;
            this.newBlockListToolStripMenuItem.Name = "newBlockListToolStripMenuItem";
            this.newBlockListToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.newBlockListToolStripMenuItem.Text = "Block List...";
            this.newBlockListToolStripMenuItem.Click += new System.EventHandler(this.newBlockListToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openMapListMenuItem,
            this.openScriptListMenuItem,
            this.openBlockListMenuItem});
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.openToolStripMenuItem.Text = "Open";
            // 
            // openMapListMenuItem
            // 
            this.openMapListMenuItem.Image = global::ProdigalSoftware.TiVEEditor.Properties.Resources.map;
            this.openMapListMenuItem.Name = "openMapListMenuItem";
            this.openMapListMenuItem.Size = new System.Drawing.Size(152, 22);
            this.openMapListMenuItem.Text = "Map List...";
            this.openMapListMenuItem.Click += new System.EventHandler(this.openMapListMenuItem_Click);
            // 
            // openScriptListMenuItem
            // 
            this.openScriptListMenuItem.Image = global::ProdigalSoftware.TiVEEditor.Properties.Resources.script;
            this.openScriptListMenuItem.Name = "openScriptListMenuItem";
            this.openScriptListMenuItem.Size = new System.Drawing.Size(152, 22);
            this.openScriptListMenuItem.Text = "Script List...";
            this.openScriptListMenuItem.Click += new System.EventHandler(this.openScriptListMenuItem_Click);
            // 
            // openBlockListMenuItem
            // 
            this.openBlockListMenuItem.Image = global::ProdigalSoftware.TiVEEditor.Properties.Resources.block;
            this.openBlockListMenuItem.Name = "openBlockListMenuItem";
            this.openBlockListMenuItem.Size = new System.Drawing.Size(152, 22);
            this.openBlockListMenuItem.Text = "Block List...";
            this.openBlockListMenuItem.Click += new System.EventHandler(this.openBlockListMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(149, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.undoToolStripMenuItem.Text = "Undo";
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.redoToolStripMenuItem.Text = "Redo";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Image = global::ProdigalSoftware.TiVEEditor.Properties.Resources.P_button;
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.aboutToolStripMenuItem.Text = "About...";
            // 
            // status
            // 
            this.status.Location = new System.Drawing.Point(0, 640);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(984, 22);
            this.status.TabIndex = 1;
            this.status.Text = "statusStrip1";
            // 
            // TiVEEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 662);
            this.Controls.Add(this.status);
            this.Controls.Add(this.mainMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.mainMenu;
            this.MinimumSize = new System.Drawing.Size(370, 240);
            this.Name = "TiVEEditorForm";
            this.Text = "TiVE Editor";
            this.mainMenu.ResumeLayout(false);
            this.mainMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MenuStrip mainMenu;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem undoToolStripMenuItem;
        private ToolStripMenuItem redoToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private StatusStrip status;
        private ToolStripMenuItem newToolStripMenuItem;
        private ToolStripMenuItem newMapToolStripMenuItem;
        private ToolStripMenuItem newScriptToolStripMenuItem;
        private ToolStripMenuItem newBlockListToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem openMapListMenuItem;
        private ToolStripMenuItem openScriptListMenuItem;
        private ToolStripMenuItem openBlockListMenuItem;
    }
}

