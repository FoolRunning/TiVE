namespace ProdigalSoftware.TiVEBlockEditor
{
    partial class FillBlockForm
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
            this.lblFillColor = new System.Windows.Forms.Label();
            this.pnlChosenColor = new System.Windows.Forms.Panel();
            this.btnChooseColor = new System.Windows.Forms.Button();
            this.ckBxColorVariation = new System.Windows.Forms.CheckBox();
            this.lblFillDensity = new System.Windows.Forms.Label();
            this.sldrColorVariation = new System.Windows.Forms.TrackBar();
            this.sldrFillDensity = new System.Windows.Forms.TrackBar();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.lblDensityValue = new System.Windows.Forms.Label();
            this.lblVariationValue = new System.Windows.Forms.Label();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.lblColorValues = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sldrColorVariation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sldrFillDensity)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.lblFillColor, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.pnlChosenColor, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnChooseColor, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.ckBxColorVariation, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblFillDensity, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.sldrColorVariation, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.sldrFillDensity, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnCancel, 4, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblDensityValue, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblVariationValue, 4, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnOk, 3, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblColorValues, 3, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(684, 170);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // lblFillColor
            // 
            this.lblFillColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFillColor.AutoSize = true;
            this.lblFillColor.Location = new System.Drawing.Point(3, 10);
            this.lblFillColor.Margin = new System.Windows.Forms.Padding(3);
            this.lblFillColor.Name = "lblFillColor";
            this.lblFillColor.Size = new System.Drawing.Size(94, 13);
            this.lblFillColor.TabIndex = 2;
            this.lblFillColor.Text = "Fill Color:";
            // 
            // pnlChosenColor
            // 
            this.pnlChosenColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlChosenColor.BackColor = System.Drawing.Color.Gray;
            this.pnlChosenColor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlChosenColor.Location = new System.Drawing.Point(103, 3);
            this.pnlChosenColor.Name = "pnlChosenColor";
            this.pnlChosenColor.Size = new System.Drawing.Size(200, 27);
            this.pnlChosenColor.TabIndex = 5;
            this.pnlChosenColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnlChosenColor_MouseClick);
            // 
            // btnChooseColor
            // 
            this.btnChooseColor.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnChooseColor.Location = new System.Drawing.Point(309, 5);
            this.btnChooseColor.Name = "btnChooseColor";
            this.btnChooseColor.Size = new System.Drawing.Size(75, 23);
            this.btnChooseColor.TabIndex = 6;
            this.btnChooseColor.Text = "&Choose...";
            this.btnChooseColor.UseVisualStyleBackColor = true;
            this.btnChooseColor.Click += new System.EventHandler(this.btnChooseColor_Click);
            // 
            // ckBxColorVariation
            // 
            this.ckBxColorVariation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.ckBxColorVariation.AutoSize = true;
            this.ckBxColorVariation.Location = new System.Drawing.Point(3, 101);
            this.ckBxColorVariation.Name = "ckBxColorVariation";
            this.ckBxColorVariation.Size = new System.Drawing.Size(94, 17);
            this.ckBxColorVariation.TabIndex = 3;
            this.ckBxColorVariation.Text = "Color &Variation";
            this.ckBxColorVariation.UseVisualStyleBackColor = true;
            this.ckBxColorVariation.CheckedChanged += new System.EventHandler(this.ckBxColorVariation_CheckedChanged);
            // 
            // lblFillDensity
            // 
            this.lblFillDensity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFillDensity.AutoSize = true;
            this.lblFillDensity.Location = new System.Drawing.Point(3, 52);
            this.lblFillDensity.Name = "lblFillDensity";
            this.lblFillDensity.Size = new System.Drawing.Size(94, 13);
            this.lblFillDensity.TabIndex = 7;
            this.lblFillDensity.Text = "Fill &Density:";
            // 
            // sldrColorVariation
            // 
            this.sldrColorVariation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.sldrColorVariation, 3);
            this.sldrColorVariation.Location = new System.Drawing.Point(103, 87);
            this.sldrColorVariation.Maximum = 50;
            this.sldrColorVariation.Name = "sldrColorVariation";
            this.sldrColorVariation.Size = new System.Drawing.Size(497, 45);
            this.sldrColorVariation.TabIndex = 4;
            this.sldrColorVariation.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.sldrColorVariation.Value = 25;
            this.sldrColorVariation.Scroll += new System.EventHandler(this.sldrColorVariation_Scroll);
            // 
            // sldrFillDensity
            // 
            this.sldrFillDensity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.sldrFillDensity, 3);
            this.sldrFillDensity.Location = new System.Drawing.Point(103, 36);
            this.sldrFillDensity.Maximum = 100;
            this.sldrFillDensity.Name = "sldrFillDensity";
            this.sldrFillDensity.Size = new System.Drawing.Size(497, 45);
            this.sldrFillDensity.TabIndex = 8;
            this.sldrFillDensity.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.sldrFillDensity.Value = 100;
            this.sldrFillDensity.Scroll += new System.EventHandler(this.sldrFillDensity_Scroll);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(606, 144);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(525, 144);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            // 
            // lblDensityValue
            // 
            this.lblDensityValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDensityValue.AutoSize = true;
            this.lblDensityValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDensityValue.Location = new System.Drawing.Point(606, 48);
            this.lblDensityValue.Name = "lblDensityValue";
            this.lblDensityValue.Size = new System.Drawing.Size(75, 20);
            this.lblDensityValue.TabIndex = 9;
            this.lblDensityValue.Text = "#%";
            // 
            // lblVariationValue
            // 
            this.lblVariationValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVariationValue.AutoSize = true;
            this.lblVariationValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVariationValue.Location = new System.Drawing.Point(606, 99);
            this.lblVariationValue.Name = "lblVariationValue";
            this.lblVariationValue.Size = new System.Drawing.Size(75, 20);
            this.lblVariationValue.TabIndex = 10;
            this.lblVariationValue.Text = "#%";
            // 
            // lblColorValues
            // 
            this.lblColorValues.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblColorValues.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblColorValues, 2);
            this.lblColorValues.Location = new System.Drawing.Point(390, 10);
            this.lblColorValues.Name = "lblColorValues";
            this.lblColorValues.Size = new System.Drawing.Size(291, 13);
            this.lblColorValues.TabIndex = 11;
            this.lblColorValues.Text = "#";
            // 
            // FillBlockForm
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(684, 170);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FillBlockForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Fill Block";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sldrColorVariation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sldrFillDensity)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblFillColor;
        private System.Windows.Forms.Panel pnlChosenColor;
        private System.Windows.Forms.Button btnChooseColor;
        private System.Windows.Forms.CheckBox ckBxColorVariation;
        private System.Windows.Forms.Label lblFillDensity;
        private System.Windows.Forms.TrackBar sldrColorVariation;
        private System.Windows.Forms.TrackBar sldrFillDensity;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.Label lblDensityValue;
        private System.Windows.Forms.Label lblVariationValue;
        private System.Windows.Forms.Label lblColorValues;
    }
}