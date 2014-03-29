using System.Drawing;
using OpenTK.Graphics;
using ProdigalSoftware.TiVE.Renderer.Voxels;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal class Sprite
    {
        public float X;
        public float Y;
        public float Z;

        private readonly IndexedVoxelGroup voxels;
        public Sprite(int spriteXSize, int spriteYSize, int spriteZSize)
        {
            voxels = new IndexedVoxelGroup(spriteXSize, spriteYSize, spriteZSize);
        }

        public static Sprite FromImage(Bitmap image)
        {
            int xSize = image.Width;
            int ySize = image.Height;

            Sprite newSprite = new Sprite(xSize, ySize, 3);
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    Color4 c = image.GetPixel(x, ySize - y - 1);
                    if (c.A > 0)
                    {
                        for (int z = 0; z < 3; z++)
                            newSprite.voxels.SetVoxel(x, y, z, (uint)c.ToArgb());
                    }
                }
            }
            return newSprite;
        }
    }
}
