namespace ProdigalSoftware.TiVE.Starter
{
    partial class StarterForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tbMessages = new System.Windows.Forms.TabPage();
            this.tbDisplay = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pnlDisplayOptionsList = new System.Windows.Forms.TableLayoutPanel();
            this.tbControls = new System.Windows.Forms.TabPage();
            this.tbSound = new System.Windows.Forms.TabPage();
            this.tbAdvanced = new System.Windows.Forms.TabPage();
            this.panel2 = new System.Windows.Forms.Panel();
            this.pnlAdvancedOptionsList = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbProject = new System.Windows.Forms.ComboBox();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnCopyText = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tbDisplay.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tbAdvanced.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(800, 500);
            this.tableLayoutPanel1.TabIndex = 0;
            this.tableLayoutPanel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tableLayoutPanel1_MouseDown);
            this.tableLayoutPanel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.tableLayoutPanel1_MouseMove);
            this.tableLayoutPanel1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tableLayoutPanel1_MouseUp);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tbMessages);
            this.tabControl1.Controls.Add(this.tbDisplay);
            this.tabControl1.Controls.Add(this.tbControls);
            this.tabControl1.Controls.Add(this.tbSound);
            this.tabControl1.Controls.Add(this.tbAdvanced);
            this.tabControl1.Location = new System.Drawing.Point(3, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(794, 447);
            this.tabControl1.TabIndex = 0;
            // 
            // tbMessages
            // 
            this.tbMessages.Location = new System.Drawing.Point(4, 22);
            this.tbMessages.Name = "tbMessages";
            this.tbMessages.Padding = new System.Windows.Forms.Padding(3);
            this.tbMessages.Size = new System.Drawing.Size(786, 421);
            this.tbMessages.TabIndex = 0;
            this.tbMessages.Text = "Messages";
            // 
            // tbDisplay
            // 
            this.tbDisplay.Controls.Add(this.panel1);
            this.tbDisplay.Location = new System.Drawing.Point(4, 22);
            this.tbDisplay.Name = "tbDisplay";
            this.tbDisplay.Size = new System.Drawing.Size(786, 421);
            this.tbDisplay.TabIndex = 1;
            this.tbDisplay.Text = "Display";
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.pnlDisplayOptionsList);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(786, 421);
            this.panel1.TabIndex = 0;
            // 
            // pnlDisplayOptionsList
            // 
            this.pnlDisplayOptionsList.AutoSize = true;
            this.pnlDisplayOptionsList.ColumnCount = 2;
            this.pnlDisplayOptionsList.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.pnlDisplayOptionsList.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlDisplayOptionsList.Location = new System.Drawing.Point(3, 3);
            this.pnlDisplayOptionsList.Name = "pnlDisplayOptionsList";
            this.pnlDisplayOptionsList.RowCount = 1;
            this.pnlDisplayOptionsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlDisplayOptionsList.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 116F));
            this.pnlDisplayOptionsList.Size = new System.Drawing.Size(743, 116);
            this.pnlDisplayOptionsList.TabIndex = 0;
            // 
            // tbControls
            // 
            this.tbControls.Location = new System.Drawing.Point(4, 22);
            this.tbControls.Name = "tbControls";
            this.tbControls.Size = new System.Drawing.Size(786, 421);
            this.tbControls.TabIndex = 3;
            this.tbControls.Text = "Controls";
            // 
            // tbSound
            // 
            this.tbSound.Location = new System.Drawing.Point(4, 22);
            this.tbSound.Name = "tbSound";
            this.tbSound.Size = new System.Drawing.Size(786, 421);
            this.tbSound.TabIndex = 4;
            this.tbSound.Text = "Sound";
            // 
            // tbAdvanced
            // 
            this.tbAdvanced.Controls.Add(this.panel2);
            this.tbAdvanced.Location = new System.Drawing.Point(4, 22);
            this.tbAdvanced.Name = "tbAdvanced";
            this.tbAdvanced.Size = new System.Drawing.Size(786, 421);
            this.tbAdvanced.TabIndex = 2;
            this.tbAdvanced.Text = "Advanced";
            // 
            // panel2
            // 
            this.panel2.AutoScroll = true;
            this.panel2.Controls.Add(this.pnlAdvancedOptionsList);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(786, 421);
            this.panel2.TabIndex = 0;
            // 
            // pnlAdvancedOptionsList
            // 
            this.pnlAdvancedOptionsList.AutoSize = true;
            this.pnlAdvancedOptionsList.ColumnCount = 2;
            this.pnlAdvancedOptionsList.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.pnlAdvancedOptionsList.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlAdvancedOptionsList.Location = new System.Drawing.Point(3, 3);
            this.pnlAdvancedOptionsList.Name = "pnlAdvancedOptionsList";
            this.pnlAdvancedOptionsList.RowCount = 1;
            this.pnlAdvancedOptionsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlAdvancedOptionsList.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 116F));
            this.pnlAdvancedOptionsList.Size = new System.Drawing.Size(743, 116);
            this.pnlAdvancedOptionsList.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.ColumnCount = 6;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.cmbProject, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnExit, 5, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnCopyText, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnStart, 2, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 456);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(794, 41);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.label1.Location = new System.Drawing.Point(3, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Start Project:";
            // 
            // cmbProject
            // 
            this.cmbProject.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbProject.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbProject.FormattingEnabled = true;
            this.cmbProject.Location = new System.Drawing.Point(92, 10);
            this.cmbProject.Name = "cmbProject";
            this.cmbProject.Size = new System.Drawing.Size(244, 21);
            this.cmbProject.TabIndex = 1;
            this.cmbProject.SelectedIndexChanged += new System.EventHandler(this.cmbProject_SelectedIndexChanged);
            // 
            // btnExit
            // 
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExit.Location = new System.Drawing.Point(716, 9);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 23);
            this.btnExit.TabIndex = 2;
            this.btnExit.Text = "E&xit";
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnCopyText
            // 
            this.btnCopyText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCopyText.Location = new System.Drawing.Point(626, 9);
            this.btnCopyText.Name = "btnCopyText";
            this.btnCopyText.Size = new System.Drawing.Size(84, 23);
            this.btnCopyText.TabIndex = 3;
            this.btnCopyText.Text = "&Copy text";
            this.btnCopyText.UseVisualStyleBackColor = false;
            this.btnCopyText.Click += new System.EventHandler(this.btnCopyText_Click);
            // 
            // btnStart
            // 
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.btnStart.Location = new System.Drawing.Point(342, 3);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(98, 35);
            this.btnStart.TabIndex = 4;
            this.btnStart.Text = "&Start";
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // StarterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 500);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StarterForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TiVE Launcher";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tbDisplay.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tbAdvanced.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tbMessages;
        private System.Windows.Forms.TabPage tbDisplay;
        private System.Windows.Forms.TabPage tbAdvanced;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbProject;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnCopyText;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel pnlDisplayOptionsList;
        private System.Windows.Forms.TabPage tbControls;
        private System.Windows.Forms.TabPage tbSound;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TableLayoutPanel pnlAdvancedOptionsList;
    }
}