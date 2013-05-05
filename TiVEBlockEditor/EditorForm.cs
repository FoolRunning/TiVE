using System.Collections.Generic;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVEBlockEditor
{
    internal partial class EditorForm : Form
    {
        private bool glLoaded;
        private readonly List<BlockInformation> blocks = new List<BlockInformation>();
        private int currentBlockIndex = -1;

        public EditorForm()
        {
            InitializeComponent();
        }

        private void btnNewBlock_Click(object sender, System.EventArgs e)
        {
            currentBlockIndex = blocks.Count;
            blocks.Add(new BlockInformation("", new uint[BlockInformation.BlockSize, BlockInformation.BlockSize, BlockInformation.BlockSize]));
            UpdateState();
        }

        private void glCurBlock_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void glCurBlock_Load(object sender, System.EventArgs e)
        {
            glLoaded = true;
        }

        private void glCurBlock_Paint(object sender, PaintEventArgs e)
        {
            if (!glLoaded)
                return;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            if (currentBlockIndex >= 0)
                ImmediateBlockRenderer.Draw(blocks[currentBlockIndex].Voxels);

            glCurBlock.SwapBuffers();
        }

        private void redrawTimer_Tick(object sender, System.EventArgs e)
        {
            glCurBlock.Invalidate();
        }

        private void UpdateState()
        {
            fillBlockToolStripMenuItem.Enabled = (currentBlockIndex >= 0);
        }
    }
}
