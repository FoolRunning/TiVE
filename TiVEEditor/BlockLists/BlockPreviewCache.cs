using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVEEditor.BlockLists
{
    /// <summary>
    /// Maintains a cache of 2D preview images for blocks.
    /// </summary>
    internal class BlockPreviewCache : IDisposable
    {
        //private const int PreviewImageSize = 44;
        public const int PreviewImageSize = 60;

        private readonly Dictionary<Block, Image> previewCache = new Dictionary<Block, Image>();

        public void Dispose()
        {
            Clear();
        }

        /// <summary>
        /// Gets the 2D preview for the specified block
        /// </summary>
        public Image GetPreview(Block block)
        {
            Image preview;
            if (!previewCache.TryGetValue(block, out preview))
                previewCache[block] = preview = CreatePreviewForBlock(block);
            return preview;
        }

        private void Clear()
        {
            foreach (Image image in previewCache.Values)
                image.Dispose();
            previewCache.Clear();
        }

        private static Image CreatePreviewForBlock(Block block)
        {
            Bitmap bitmap = new Bitmap(PreviewImageSize, PreviewImageSize, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bitmap))
            using (SolidBrush brush = new SolidBrush(Color.Black))
            {
                g.FillRectangle(brush, 0, 0, PreviewImageSize, PreviewImageSize);

                for (int y = Block.VoxelSize - 1; y >= 0; y--)
                {
                    for (int x = 0; x < Block.VoxelSize; x++)
                    {
                        for (int z = 0; z < Block.VoxelSize; z++)
                        {
                            Voxel color = block[x, y, z];
                            if (color == 0)
                                continue;

                            brush.Color = Color.FromArgb(color.A, color.R, color.G, color.B);
                            //g.FillRectangle(brush, x * 2 + 18 - BlockInformation.VoxelSize + y, 28 - y - z * 2 + x, 3, 3);
                            g.FillRectangle(brush, x * 2 + 24 - Block.VoxelSize + y, 42 - y - z * 2 + x, 3, 3);
                        }
                    }
                }
            }
            return bitmap;
        }
    }
}
