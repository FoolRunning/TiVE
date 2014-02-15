using System.Drawing;
using OpenTK.Graphics;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal class Sprite : IndexedVoxelGroup
    {
        public float X;
        public float Y;
        public float Z;

        public Sprite(int spriteXSize, int spriteYSize, int spriteZSize) : base(spriteXSize, spriteYSize, spriteZSize)
        {
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
                            newSprite.SetVoxel(x, y, z, (uint)c.ToArgb());
                    }
                }
            }
            return newSprite;
        }
    }
}
