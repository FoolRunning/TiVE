using System.Diagnostics;
using System.Threading;
using ProdigalSoftware.TiVEPluginFramework.Lighting;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Resources
{
    internal sealed class LightManager
    {
        private ILight ambientLight;

        public void UpdateCameraPos(int camMinX, int camMaxX, int camMinY, int camMaxY)
        {
            Debug.Assert(Thread.CurrentThread.Name == "Main UI");

            //GameWorld gameWorld = ResourceManager.GameWorldManager.GameWorld;
            //chunkMinX = Math.Max(0, Math.Min(gameWorld.XChunkSize, camMinX / GameWorldVoxelChunk.TileSize - 1));
            //chunkMaxX = Math.Max(0, Math.Min(gameWorld.XChunkSize, (int)Math.Ceiling(camMaxX / (float)GameWorldVoxelChunk.TileSize) + 1));
            //chunkMinY = Math.Max(0, Math.Min(gameWorld.YChunkSize, camMinY / GameWorldVoxelChunk.TileSize - 1));
            //chunkMaxY = Math.Max(0, Math.Min(gameWorld.YChunkSize, (int)Math.Ceiling(camMaxY / (float)GameWorldVoxelChunk.TileSize) + 1));
            //chunkMaxZ = Math.Max((int)Math.Ceiling(gameWorld.Zsize / (float)GameWorldVoxelChunk.TileSize), 1);

            //for (int i = 0; i < loadedChunksList.Count; i++)
            //{
            //    GameWorldVoxelChunk chunk = loadedChunksList[i];
            //    if (!chunk.IsInside(chunkMinX, chunkMinY, chunkMaxX, chunkMaxY))
            //        chunksToDelete.Add(chunk);
            //}

            //for (int chunkZ = chunkMaxZ - 1; chunkZ >= 0; chunkZ--)
            //{
            //    for (int chunkX = chunkMinX; chunkX < chunkMaxX; chunkX++)
            //    {
            //        for (int chunkY = chunkMinY; chunkY < chunkMaxY; chunkY++)
            //        {
            //            GameWorldVoxelChunk chunk = gameWorld.GetChunk(chunkX, chunkY, chunkZ);
            //            if (!loadedChunks.Contains(chunk))
            //            {
            //                chunk.PrepareForLoad();
            //                using (new PerformanceLock(chunkLoadQueue))
            //                    chunkLoadQueue.Enqueue(chunk);
            //                loadedChunks.Add(chunk);
            //                loadedChunksList.Add(chunk);
            //            }
            //        }
            //    }
            //}
        }

        public Color4b GetLightAt(int worldX, int worldY, int worldZ)
        {
            Color4b light = new Color4b();

            if (ambientLight != null)
                light += ambientLight.GetColorAtDist(0.0f);

            return light;
        }
    }
}
