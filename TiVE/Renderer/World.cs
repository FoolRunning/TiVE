using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace ProdigalSoftware.TiVE.Renderer
{
    public sealed class World
    {
        private readonly ushort[, ,] world;
        private readonly int xSize;
        private readonly int ySize;
        private readonly int zSize;
        private readonly List<Sprite> sprites = new List<Sprite>();
        private List<Block> blockList;

        internal World(int xSize, int ySize, int zSize)
        {
            this.xSize = xSize;
            this.ySize = ySize;
            this.zSize = zSize;
            world = new ushort[xSize, ySize, zSize];
        }

        public void SetBlockList(List<Block> newBlockList)
        {
            this.blockList = newBlockList;
        }

        public void SetBlock(int x, int y, int z, ushort blockIndex)
        {
            world[x, y, z] = blockIndex;
        }

        public void AddSprite(Sprite toAdd)
        {
            sprites.Add(toAdd);
        }

        public int Draw(Camera camera)
        {
            int minX, maxX, minY, maxY;

            GetWorldView(camera, camera.Location.Z, out minX, out maxX, out minY, out maxY);

            minX = Math.Max(minX, 0);
            minY = Math.Max(minY, 0);
            maxX = Math.Min(maxX, xSize);
            maxY = Math.Min(maxY, ySize);

            Matrix4 viewProjectionMatrix = camera.ViewMatrix * camera.ProjectionMatrix;

            int polygonCount = 0;
            Matrix4 translationMatrix = Matrix4.Identity;
            for (int z = 0; z < 2; z++)
            {
                translationMatrix.M43 = z * Block.Block_Size;
                for (int x = minX; x < maxX; x++)
                {
                    translationMatrix.M41 = x * Block.Block_Size;

                    for (int y = minY; y < maxY; y++)
                    {
                        translationMatrix.M42 = y * Block.Block_Size;

                        Matrix4 viewProjectionModelMatrix = translationMatrix * viewProjectionMatrix;

                        Block block = blockList[world[x, y, z]];
                        block.Render(ref viewProjectionModelMatrix);
                        polygonCount += block.PolygonCount;
                    }
                }
            }

            for (int s = 0; s < sprites.Count; s++)
            {
                Sprite sprite = sprites[s];

                translationMatrix.M41 = sprite.X;
                translationMatrix.M42 = sprite.Y;
                translationMatrix.M43 = sprite.Z;
                Matrix4 viewProjectionModelMatrix = translationMatrix * viewProjectionMatrix;

                sprites[s].Render(ref viewProjectionModelMatrix);
                polygonCount += sprites[s].PolygonCount;
            }

            return polygonCount;
        }

        private static void GetWorldView(Camera camera, float distance, out int minX, out int maxX, out int minY, out int maxY)
        {
            float Hfar = 2.0f * (float)Math.Tan(camera.FoV / 2) * distance;
            float Wfar = Hfar * camera.AspectRatio;

            Vector3 fc = camera.Location + new Vector3(0, 0, -1) * distance;
            Vector3 topLeft = fc + (Vector3.UnitY * Hfar / 2) - (Vector3.UnitX * Wfar / 2);
            Vector3 bottomRight = fc - (Vector3.UnitY * Hfar / 2) + (Vector3.UnitX * Wfar / 2);

            minX = (int)Math.Floor(topLeft.X / Block.Block_Size);
            maxX = (int)Math.Ceiling(bottomRight.X / Block.Block_Size);
            minY = (int)Math.Floor(bottomRight.Y / Block.Block_Size);
            maxY = (int)Math.Ceiling(topLeft.Y / Block.Block_Size);
        }
    }
}
