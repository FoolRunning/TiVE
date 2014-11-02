using System;
using System.Windows.Forms;
using OpenTK.Graphics;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVEBlockEditor
{
    internal partial class FillBlockForm : Form
    {
        private readonly EditorForm editorForm;
        private readonly Random random = new Random();

        public FillBlockForm(EditorForm editorForm)
        {
            this.editorForm = editorForm;

            InitializeComponent();
            UpdateBlock();
            UpdateState();
            editorForm.ForceBlockUpdate();
        }

        private void btnChooseColor_Click(object sender, EventArgs e)
        {
            ChooseColor();
        }

        private void pnlChosenColor_MouseClick(object sender, MouseEventArgs e)
        {
            ChooseColor();
        }

        private void ckBxColorVariation_CheckedChanged(object sender, EventArgs e)
        {
            UpdateBlock();
            UpdateState();
            editorForm.ForceBlockUpdate();
        }

        private void sldrFillDensity_Scroll(object sender, EventArgs e)
        {
            UpdateBlock();
            editorForm.ForceBlockUpdate();
        }

        private void sldrColorVariation_Scroll(object sender, EventArgs e)
        {
            UpdateBlock();
            editorForm.ForceBlockUpdate();
        }

        private void ChooseColor()
        {
            colorDialog.AnyColor = true;
            colorDialog.Color = pnlChosenColor.BackColor;
            if (colorDialog.ShowDialog(this) == DialogResult.OK)
                pnlChosenColor.BackColor = colorDialog.Color;

            UpdateBlock();
            editorForm.ForceBlockUpdate();
        }

        private void UpdateState()
        {

            sldrColorVariation.Enabled = ckBxColorVariation.Checked;
            lblVariationValue.Enabled = ckBxColorVariation.Checked;
        }

        private void UpdateBlock()
        {
            BlockInformation block = editorForm.CurrentBlock;
            for (int x = 0; x < BlockInformation.VoxelSize; x++)
            {
                for (int y = 0; y < BlockInformation.VoxelSize; y++)
                {
                    for (int z = 0; z < BlockInformation.VoxelSize; z++)
                    {
                        if (random.NextDouble() > (sldrFillDensity.Value / 100.0))
                            block[x, y, z] = 0;
                        else
                            block[x, y, z] = FromColor(CreateColorFromColor(pnlChosenColor.BackColor));
                    }
                }
            }
        }
        
        private static uint FromColor(Color4 color)
        {
            return (uint)color.ToArgb();
        }

        private Color4 CreateColorFromColor(Color4 seed)
        {
            float variation = ckBxColorVariation.Checked ? sldrColorVariation.Value / 50.0f : 0.0f;
            float scale = (float)(random.NextDouble() * variation - variation / 2.0 + 1.0);
            return new Color4(Math.Max(Math.Min(seed.R * scale, 1.0f), 0.0f), 
                Math.Max(Math.Min(seed.G * scale, 1.0f), 0.0f),
                Math.Max(Math.Min(seed.B * scale, 1.0f), 0.0f), 
                seed.A);
        }
    }
}
