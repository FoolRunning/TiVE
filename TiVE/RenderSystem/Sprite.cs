using System.Drawing;
using ProdigalSoftware.TiVE.RenderSystem.Voxels;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.RenderSystem
{
    internal class Sprite
    {
        public float X;
        public float Y;
        public float Z;

        private readonly VoxelGroup voxels;

        public Sprite(int spriteXSize, int spriteYSize, int spriteZSize)
        {
            voxels = new VoxelGroup(spriteXSize, spriteYSize, spriteZSize);
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
                    Color color = image.GetPixel(x, ySize - y - 1);
                    Color4b c = new Color4b(color.R, color.G, color.B, color.A);
                    if (c.A > 0)
                    {
                        for (int z = 0; z < 3; z++)
                            newSprite.voxels[x, y, z] = c.ToArgb();
                    }
                }
            }
            return newSprite;
        }
    }
}
