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
        private const int PreviewImageSize = 44;

        private readonly Dictionary<BlockInformation, Image> previewCache = new Dictionary<BlockInformation, Image>();

        public void Dispose()
        {
            Clear();
        }

        public void Clear()
        {
            foreach (Image image in previewCache.Values)
                image.Dispose();
            previewCache.Clear();
        }

        public Image GetPreview(BlockInformation block)
        {
            Image preview;
            if (!previewCache.TryGetValue(block, out preview))
                previewCache[block] = preview = CreatePreviewForBlock(block);
            return preview;
        }

        private static Image CreatePreviewForBlock(BlockInformation block)
        {
            Bitmap bitmap = new Bitmap(PreviewImageSize, PreviewImageSize, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bitmap))
            using (SolidBrush brush = new SolidBrush(Color.Black))
            {
                g.FillRectangle(brush, 0, 0, PreviewImageSize, PreviewImageSize);

                for (int y = BlockInformation.VoxelSize - 1; y >= 0; y--)
                {
                    for (int x = 0; x < BlockInformation.VoxelSize; x++)
                    {
                        for (int z = 0; z < BlockInformation.VoxelSize; z++)
                        {
                            int color = (int)block[x, y, z];
                            if (color == 0)
                                continue;

                            brush.Color = Color.FromArgb(color);
                            g.FillRectangle(brush, x * 2 + 18 - BlockInformation.VoxelSize + y, 28 - y - z * 2 + x, 3, 3);
                        }
                    }
                }
            }
            return bitmap;
        }
    }
}
