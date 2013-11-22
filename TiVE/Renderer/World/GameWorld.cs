using System;
using System.Collections.Generic;
using OpenTK;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    /// <summary>
    /// Contains the information about the game world. 
    /// </summary>
    internal sealed class GameWorld : IGameWorld
    {
        private readonly ushort[, ,] worldBlocks;
        private readonly byte[,] worldBiomes;
        private readonly int xSize;
        private readonly int ySize;
        private readonly int zSize;
        private readonly List<Sprite> sprites = new List<Sprite>();
        private readonly BlockList blockList;

        internal GameWorld(int xSize, int ySize, int zSize, BlockList blockList)
        {
            this.xSize = xSize;
            this.ySize = ySize;
            this.zSize = zSize;
            this.blockList = blockList;
            worldBlocks = new ushort[xSize, ySize, zSize];
            worldBiomes = new byte[xSize, ySize];

        }

        public int Xsize
        {
            get { return xSize; }
        }

        public int Ysize
        {
            get { return ySize; }
        }

        public int Zsize
        {
            get { return zSize; }
        }

        public void SetBlock(int x, int y, int z, ushort blockIndex)
        {
            if (blockIndex >= blockList.BlockCount)
                throw new ArgumentOutOfRangeException("blockIndex", "Block with the specified index does not exist");

            worldBlocks[x, y, z] = blockIndex;
        }

        public void SetBiome(int x, int y, byte biomeId)
        {
            worldBiomes[x, y] = biomeId;
        }

        public ushort GetBlock(int x, int y, int z)
        {
            return worldBlocks[x, y, z];
        }

        public byte GetBiome(int x, int y)
        {
            return worldBiomes[x, y];
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
            for (int z = 0; z < zSize; z++)
            {
                translationMatrix.M43 = z * BlockInformation.BlockSize;
                for (int x = minX; x < maxX; x++)
                {
                    translationMatrix.M41 = x * BlockInformation.BlockSize;

                    for (int y = minY; y < maxY; y++)
                    {
                        Block block = blockList[worldBlocks[x, y, z]];
                        if (block != null)
                        {
                            translationMatrix.M42 = y * BlockInformation.BlockSize;

                            Matrix4 viewProjectionModelMatrix = translationMatrix * viewProjectionMatrix;

                            block.Render(ref viewProjectionModelMatrix);
                            polygonCount += block.PolygonCount;
                        }
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
            float hfar = 2.0f * (float)Math.Tan(camera.FoV / 2) * distance;
            float wfar = hfar * camera.AspectRatio;

            Vector3 fc = camera.Location + new Vector3(0, 0, -1) * distance;
            Vector3 topLeft = fc + (Vector3.UnitY * hfar / 2) - (Vector3.UnitX * wfar / 2);
            Vector3 bottomRight = fc - (Vector3.UnitY * hfar / 2) + (Vector3.UnitX * wfar / 2);

            minX = (int)Math.Floor(topLeft.X / BlockInformation.BlockSize);
            maxX = (int)Math.Ceiling(bottomRight.X / BlockInformation.BlockSize);
            minY = (int)Math.Floor(bottomRight.Y / BlockInformation.BlockSize);
            maxY = (int)Math.Ceiling(topLeft.Y / BlockInformation.BlockSize);
        }
    }
}
