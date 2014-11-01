using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using ProdigalSoftware.TiVE.Renderer;
using ProdigalSoftware.TiVE.Renderer.Voxels;
using ProdigalSoftware.TiVEPluginFramework;
using KeyPressEventArgs = System.Windows.Forms.KeyPressEventArgs;

namespace ProdigalSoftware.TiVEBlockEditor
{
    internal partial class EditorForm : Form
    {
        private bool glLoaded;
        private readonly List<BlockInformation> blocks = new List<BlockInformation>();
        private int currentBlockIndex = -1;
        private SimpleVoxelGroup currentBlock;
        private readonly Camera camera = new Camera();
        private bool forceUpdateBlock;

        public EditorForm()
        {
            InitializeComponent();
            UpdateState();
            
            camera.Location = new Vector3(8, 8, 31);
            camera.LookAtLocation = new Vector3(8, 8, 0);
        }

        public BlockInformation CurrentBlock
        {
            get { return currentBlockIndex >= 0 ? blocks[currentBlockIndex] : null; }
        }

        public void ForceBlockUpdate()
        {
            forceUpdateBlock = true;
            glCurBlock.Invalidate();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (currentBlock != null)
                currentBlock.Dispose();
            base.OnClosing(e);
        }

        private void btnNewBlock_Click(object sender, System.EventArgs e)
        {
            currentBlockIndex = blocks.Count;
            blocks.Add(new BlockInformation("New Block"));
            UpdateState();
            ForceBlockUpdate();
        }

        private void glCurBlock_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        private void glCurBlock_Load(object sender, System.EventArgs e)
        {
            glLoaded = true;
            glCurBlock.MakeCurrent();
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            camera.AspectRatio = glCurBlock.Width / (float)glCurBlock.Height;
        }

        private void glCurBlock_SizeChanged(object sender, System.EventArgs e)
        {
            camera.AspectRatio = glCurBlock.Width / (float)glCurBlock.Height;
            glCurBlock.Invalidate();
        }

        private void glCurBlock_Paint(object sender, PaintEventArgs e)
        {
            if (!glLoaded)
                return;
            
            glCurBlock.MakeCurrent();

            GL.Viewport(0, 0, glCurBlock.Width, glCurBlock.Height);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (currentBlock != null)
            {
                camera.Update();
                
                Matrix4 viewProjectionMatrix = camera.ViewMatrix * camera.ProjectionMatrix;
                currentBlock.Render(ref viewProjectionMatrix);
            }

            glCurBlock.SwapBuffers();
        }

        private void redrawTimer_Tick(object sender, System.EventArgs e)
        {
            if (currentBlockIndex >= 0 && (currentBlock == null || forceUpdateBlock))
            {
                if (currentBlock != null)
                    currentBlock.Dispose();
                //blocks[currentBlockIndex].Voxels;
                currentBlock = new SimpleVoxelGroup(1, 1, 1);
                forceUpdateBlock = false;
            }

            glCurBlock.Invalidate();
        }

        private void fillBlockToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            using (FillBlockForm form = new FillBlockForm(this))
                form.ShowDialog(this);
        }

        private void exitToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void newToolStripMenuItem_Click(object sender, System.EventArgs e)
        {

        }

        private void openToolStripMenuItem_Click(object sender, System.EventArgs e)
        {

        }

        private void saveBlockListToolStripMenuItem_Click(object sender, System.EventArgs e)
        {

        }

        private void btnPrevious_Click(object sender, System.EventArgs e)
        {
            if (blocks.Count > 0 && currentBlockIndex > 0)
                currentBlockIndex--;
            ForceBlockUpdate();
        }

        private void btnNext_Click(object sender, System.EventArgs e)
        {
            if (blocks.Count > 0 && currentBlockIndex < blocks.Count - 1)
                currentBlockIndex++;
            ForceBlockUpdate();
        }

        private void UpdateState()
        {
            btnNext.Enabled = currentBlockIndex < blocks.Count - 1;
            btnPrevious.Enabled = currentBlockIndex > 0;
            fillBlockToolStripMenuItem.Enabled = (currentBlockIndex >= 0);
            txtBlockId.Enabled = (currentBlockIndex >= 0);
            txtBlockId.Text = currentBlockIndex >= 0 ? blocks[currentBlockIndex].BlockName : "";
            Text = "TiVE Block Editor - Unnamed List";
        }
    }
}
