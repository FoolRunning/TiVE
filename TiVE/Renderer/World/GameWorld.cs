using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    /// <summary>
    /// Contains the information about the game world. 
    /// </summary>
    internal sealed class GameWorld : IGameWorld
    {
        private readonly ushort[] worldBlocks;
        private readonly byte[] worldBiomes;
        private readonly int xSize;
        private readonly int ySize;
        private readonly int zSize;
        private readonly int xSizePowOf2;
        private readonly int xySizePowOf2;
        //private readonly List<Sprite> sprites = new List<Sprite>();
        private readonly BlockList blockList;

        internal GameWorld(int xSize, int ySize, int zSize, BlockList blockList)
        {
            int ySizePowOf2, zSizePowOf2;
            this.xSize = GetPow2(xSize, out xSizePowOf2);
            this.ySize = GetPow2(ySize, out ySizePowOf2);
            this.zSize = GetPow2(zSize, out zSizePowOf2);
            xySizePowOf2 = xSizePowOf2 + ySizePowOf2;

            this.blockList = blockList;
            worldBlocks = new ushort[this.xSize * this.ySize * this.zSize];
            worldBiomes = new byte[this.xSize * this.ySize];
            
            Debug.WriteLine("Created world: " + this.xSize + ", " + this.ySize + ", " + this.zSize);
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

            worldBlocks[CalcWorldOffset(x, y, z)] = blockIndex;
        }

        public void SetBiome(int x, int y, byte biomeId)
        {
            worldBiomes[CalcBiomOffset(x, y)] = biomeId;
        }

        public ushort GetBlock(int x, int y, int z)
        {
            return worldBlocks[CalcWorldOffset(x, y, z)];
        }

        public byte GetBiome(int x, int y)
        {
            return worldBiomes[CalcBiomOffset(x, y)];
        }

        //public void AddSprite(Sprite toAdd)
        //{
        //    sprites.Add(toAdd);
        //}

        //public void Draw(Camera camera, out int drawCount, out int polygonCount)
        //{
        //    int minX, maxX, minY, maxY;
        //    GetWorldView(camera, camera.Location.Z, out minX, out maxX, out minY, out maxY);

        //    minX = Math.Max(minX, 0);
        //    minY = Math.Max(minY, 0);
        //    maxX = Math.Min(maxX, xSize);
        //    maxY = Math.Min(maxY, ySize);

        //    Matrix4 viewProjectionMatrix = FastMult(camera.ViewMatrix, camera.ProjectionMatrix);

        //    polygonCount = 0;
        //    drawCount = 0;
        //    Matrix4 translationMatrix = Matrix4.Identity;
        //    for (int z = 0; z < zSize; z++)
        //    {
        //        translationMatrix.M43 = z * BlockInformation.BlockSize;
        //        for (int x = minX; x < maxX; x++)
        //        {
        //            translationMatrix.M41 = x * BlockInformation.BlockSize;

        //            for (int y = minY; y < maxY; y++)
        //            {
        //                Block block = blockList[GetBlock(x, y, z)];
        //                if (block != null)
        //                {
        //                    translationMatrix.M42 = y * BlockInformation.BlockSize;

        //                    Matrix4 viewProjectionModelMatrix = FastMult(translationMatrix, viewProjectionMatrix);

        //                    block.Render(ref viewProjectionModelMatrix);
        //                    polygonCount += block.PolygonCount;
        //                    drawCount++;
        //                }
        //            }
        //        }
        //    }

        //    //for (int s = 0; s < sprites.Count; s++)
        //    //{
        //    //    Sprite sprite = sprites[s];

        //    //    translationMatrix.M41 = sprite.X;
        //    //    translationMatrix.M42 = sprite.Y;
        //    //    translationMatrix.M43 = sprite.Z;
        //    //    Matrix4 viewProjectionModelMatrix = translationMatrix * viewProjectionMatrix;

        //    //    sprites[s].Render(ref viewProjectionModelMatrix);
        //    //    drawCount++;
        //    //    polygonCount += sprites[s].PolygonCount;
        //    //}
        //}

        //private static Matrix4 FastMult(Matrix4 left, Matrix4 right)
        //{
        //    return new Matrix4(
        //        left.M11 * right.M11 + left.M12 * right.M21 + left.M13 * right.M31 + left.M14 * right.M41, 
        //        left.M11 * right.M12 + left.M12 * right.M22 + left.M13 * right.M32 + left.M14 * right.M42, 
        //        left.M11 * right.M13 + left.M12 * right.M23 + left.M13 * right.M33 + left.M14 * right.M43, 
        //        left.M11 * right.M14 + left.M12 * right.M24 + left.M13 * right.M34 + left.M14 * right.M44, 
        //        left.M21 * right.M11 + left.M22 * right.M21 + left.M23 * right.M31 + left.M24 * right.M41, 
        //        left.M21 * right.M12 + left.M22 * right.M22 + left.M23 * right.M32 + left.M24 * right.M42, 
        //        left.M21 * right.M13 + left.M22 * right.M23 + left.M23 * right.M33 + left.M24 * right.M43, 
        //        left.M21 * right.M14 + left.M22 * right.M24 + left.M23 * right.M34 + left.M24 * right.M44,
        //        left.M31 * right.M11 + left.M32 * right.M21 + left.M33 * right.M31 + left.M34 * right.M41, 
        //        left.M31 * right.M12 + left.M32 * right.M22 + left.M33 * right.M32 + left.M34 * right.M42, 
        //        left.M31 * right.M13 + left.M32 * right.M23 + left.M33 * right.M33 + left.M34 * right.M43, 
        //        left.M31 * right.M14 + left.M32 * right.M24 + left.M33 * right.M34 + left.M34 * right.M44, 
        //        left.M41 * right.M11 + left.M42 * right.M21 + left.M43 * right.M31 + left.M44 * right.M41, 
        //        left.M41 * right.M12 + left.M42 * right.M22 + left.M43 * right.M32 + left.M44 * right.M42, 
        //        left.M41 * right.M13 + left.M42 * right.M23 + left.M43 * right.M33 + left.M44 * right.M43, 
        //        left.M41 * right.M14 + left.M42 * right.M24 + left.M43 * right.M34 + left.M44 * right.M44);
        //}

        //private static void GetWorldView(Camera camera, float distance, out int minX, out int maxX, out int minY, out int maxY)
        //{
        //    float hfar = 2.0f * (float)Math.Tan(camera.FoV / 2) * distance;
        //    float wfar = hfar * camera.AspectRatio;

        //    Vector3 fc = camera.Location + new Vector3(0, 0, -1) * distance;
        //    Vector3 topLeft = fc + (Vector3.UnitY * hfar / 2) - (Vector3.UnitX * wfar / 2);
        //    Vector3 bottomRight = fc - (Vector3.UnitY * hfar / 2) + (Vector3.UnitX * wfar / 2);

        //    minX = (int)Math.Floor(topLeft.X / BlockInformation.BlockSize);
        //    maxX = (int)Math.Ceiling(bottomRight.X / BlockInformation.BlockSize);
        //    minY = (int)Math.Floor(bottomRight.Y / BlockInformation.BlockSize);
        //    maxY = (int)Math.Ceiling(topLeft.Y / BlockInformation.BlockSize);
        //}

        private static int GetPow2(int value, out int incrCount)
        {
            int pow = 1;
            incrCount = 0;
            while (pow < value)
            {
                pow += pow;
                incrCount++;
            }
            return pow;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CalcWorldOffset(int x, int y, int z)
        {
            return x + (y << xSizePowOf2) + (z << xySizePowOf2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CalcBiomOffset(int x, int y)
        {
            return x + (y << xSizePowOf2);
        }
    }
}
