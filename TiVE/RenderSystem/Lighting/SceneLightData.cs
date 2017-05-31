using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVE.RenderSystem.World;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.RenderSystem.Lighting
{
    #region LightCullType enum
    internal enum LightCullType
    {
        Fast,
        Accurate
    }
    #endregion

    internal sealed class SceneLightData
    {
        #region Constants
        public const int MaxLightsPerChunk = 50;
        private const int HalfBlockVoxelSize = BlockLOD32.VoxelSize / 2;
        #endregion

        #region Member variables
        private readonly Scene scene;
        private readonly LightingModel lightingModel;
        private readonly List<ushort>[] chunkLightInfo;
        private Vector3i chunkSize;
        #endregion

        #region Constructor
        public SceneLightData(Scene scene)
        {
            this.scene = scene;

            GameWorld gameWorld = scene.GameWorldInternal;
            chunkSize = new Vector3i((int)Math.Ceiling(gameWorld.BlockSize.X / (float)ChunkComponent.BlockSize),
                (int)Math.Ceiling(gameWorld.BlockSize.Y / (float)ChunkComponent.BlockSize),
                (int)Math.Ceiling(gameWorld.BlockSize.Z / (float)ChunkComponent.BlockSize));

            chunkLightInfo = new List<ushort>[chunkSize.X * chunkSize.Y * chunkSize.Z];

            lightingModel = LightingModel.Get(gameWorld.LightingModelType);
        }
        #endregion

        #region Properties
        public LightInfo[] LightList { get; private set; }
        #endregion

        #region Public methods
        /// <summary>
        /// Caches lighting information for the scene
        /// </summary>
        public void Calculate()
        {
            Messages.Print("Calculating block lights...");
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < chunkLightInfo.Length; i++)
                chunkLightInfo[i] = new List<ushort>(100);

            int numThreads = Environment.ProcessorCount > 3 ? Environment.ProcessorCount - 1 : 1;
            Thread[] threads = new Thread[numThreads];
            List<LightInfo> lightInfos = new List<LightInfo>(20000);
            lightInfos.Add(null);
            for (int i = 0; i < numThreads; i++)
                threads[i] = StartLightCalculationThread("Light " + (i + 1), lightInfos, i * scene.GameWorld.BlockSize.X / numThreads, (i + 1) * scene.GameWorld.BlockSize.X / numThreads);

            foreach (Thread thread in threads)
                thread.Join();

            for (int i = 0; i < chunkLightInfo.Length; i++)
                chunkLightInfo[i].TrimExcess();

            LightList = lightInfos.ToArray();

            sw.Stop();
            Messages.AddDoneText();
            Messages.AddDebug($"Lighting for {lightInfos.Count - 1} lights took {sw.ElapsedMilliseconds}ms");


            int maxLightInChunks = 0;
            for (int i = 0; i < chunkLightInfo.Length; i++)
            {
                if (chunkLightInfo[i].Count > maxLightInChunks)
                    maxLightInChunks = chunkLightInfo[i].Count;
            }

            Messages.AddDebug($"Max lights in chunks: {maxLightInChunks}");

            //if (!TiVEController.UserSettings.Get(UserSettings.PreLoadLightingKey))
            //    return;

            //Messages.Print("Pre-loading lighting...");
            //sw.Restart();
            //int preLoadNumThreads = Environment.ProcessorCount > 3 ? Environment.ProcessorCount - 2 : 1;
            //threads = new Thread[preLoadNumThreads];
            //List<ChunkComponent> allChunks = scene.GetEntitiesWithComponent<ChunkComponent>().Select(e => e.GetComponent<ChunkComponent>()).ToList();
            //for (int i = 0; i < preLoadNumThreads; i++)
            //    threads[i] = StartLightPreLoadThread("Light preload " + (i + 1), allChunks, i * allChunks.Count / preLoadNumThreads, (i + 1) * allChunks.Count / preLoadNumThreads);
            //foreach (Thread thread in threads)
            //    thread.Join();

            //sw.Stop();
            //Messages.AddDoneText();
            //Messages.AddDebug(string.Format("Pre-load lighting took {0}ms", sw.ElapsedMilliseconds));
        }

        public RenderedLight[] GetLightsInChunk(int cx, int cy, int cz, int maxLights = MaxLightsPerChunk)
        {
            // TODO: Cache this in the chunk somehow
            List<ushort> lights = chunkLightInfo[chunkSize.GetArrayOffset(cx, cy, cz)];
            return lights.Take(maxLights).Select(l => new RenderedLight(LightList[l].Location, LightList[l].LightColor, LightList[l].CachedLightCalc)).ToArray();
        }
        #endregion

        #region Private helper methods
        private Thread StartLightCalculationThread(string threadName, List<LightInfo> lightInfos, int startX, int endX)
        {
            Thread thread = new Thread(() =>
            {
                GameWorld gameWorld = scene.GameWorldInternal;
                int sizeY = gameWorld.BlockSize.Y;
                int sizeZ = gameWorld.BlockSize.Z;

                for (int x = startX; x < endX; x++)
                {
                    for (int z = 0; z < sizeZ; z++)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
                            LightComponent light = gameWorld[x, y, z].GetComponent<LightComponent>();
                            if (light != null)
                                FillChunksWithLight(light, lightInfos, x, y, z);
                        }
                    }
                }
            });

            thread.Name = threadName;
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.Normal;
            thread.Start();
            return thread;
        }

        //private Thread StartLightPreLoadThread(string threadName, List<ChunkComponent> allChunks, int startIndex, int endIndex)
        //{
        //    Thread thread = new Thread(() =>
        //    {
        //        endIndex = Math.Min(endIndex, allChunks.Count);
        //        for (int i = startIndex; i < endIndex; i++)
        //            CacheLightsInBlocksForChunk(allChunks[i].ChunkLoc.X, allChunks[i].ChunkLoc.Y, allChunks[i].ChunkLoc.Z);
        //    });

        //    thread.Name = threadName;
        //    thread.IsBackground = true;
        //    thread.Priority = ThreadPriority.Normal;
        //    thread.Start();
        //    return thread;
        //}

        private void FillChunksWithLight(LightComponent light, List<LightInfo> lightInfos, int blockX, int blockY, int blockZ)
        {
            int startX = Math.Max(0, (blockX - light.LightBlockDist) / ChunkComponent.BlockSize);
            int startY = Math.Max(0, (blockY - light.LightBlockDist) / ChunkComponent.BlockSize);
            int startZ = Math.Max(0, (blockZ - light.LightBlockDist) / ChunkComponent.BlockSize);
            int endX = Math.Min(chunkSize.X, (int)Math.Ceiling((blockX + light.LightBlockDist + 1) / (float)ChunkComponent.BlockSize));
            int endY = Math.Min(chunkSize.Y, (int)Math.Ceiling((blockY + light.LightBlockDist + 1) / (float)ChunkComponent.BlockSize));
            int endZ = Math.Min(chunkSize.Z, (int)Math.Ceiling((blockZ + light.LightBlockDist + 1) / (float)ChunkComponent.BlockSize));

            ushort lightIndex;
            LightInfo lightInfo = new LightInfo(blockX, blockY, blockZ, light, lightingModel.GetCacheLightCalculation(light));
            lock (lightInfos)
            {
                if (lightInfos.Count == ushort.MaxValue - 1)
                    Messages.AddWarning("Too many lights in game world");
                if (lightInfos.Count == ushort.MaxValue)
                    return; // Too many lights in the world

                lightIndex = (ushort)lightInfos.Count;
                lightInfos.Add(lightInfo);
            }

            for (int cz = startZ; cz < endZ; cz++)
            {
                for (int cx = startX; cx < endX; cx++)
                {
                    for (int cy = startY; cy < endY; cy++)
                    {
                        //if (LightHitsChunk(blockX, blockY, blockZ, cx, cy, cz))
                        {
                            int vx = cx * ChunkComponent.VoxelSize + ChunkComponent.VoxelSize / 2;
                            int vy = cy * ChunkComponent.VoxelSize + ChunkComponent.VoxelSize / 2;
                            int vz = cz * ChunkComponent.VoxelSize + ChunkComponent.VoxelSize / 2;
                            int newLightDist = DistSquared(vx, vy, vz, lightInfo);
                            List<ushort> lightsInChunk = chunkLightInfo[chunkSize.GetArrayOffset(cx, cy, cz)];
                            lock (lightsInChunk)
                            {
                                // Calculate lighting information
                                // Sort lights by highest percentage to lowest
                                int leastLightIndex = lightsInChunk.Count;
                                for (int i = 0; i < lightsInChunk.Count; i++)
                                {
                                    if (DistSquared(vx, vy, vz, lightInfos[lightsInChunk[i]]) > newLightDist)
                                    {
                                        leastLightIndex = i;
                                        break;
                                    }
                                }

                                lightsInChunk.Insert(leastLightIndex, lightIndex);
                            }
                        }
                    }
                }
            }
        }

        private static int DistSquared(int vx, int vy, int vz, LightInfo lightInfo)
        {
            int diffX = (int)lightInfo.Location.X - vx;
            int diffY = (int)lightInfo.Location.Y - vy;
            int diffZ = (int)lightInfo.Location.Z - vz;
            return diffX * diffX + diffY * diffY + diffZ * diffZ;
        }

        private bool LightHitsChunk(int blockX, int blockY, int blockZ, int cx, int cy, int cz)
        {
            Vector3i blockSize = scene.GameWorld.BlockSize;
            for (int chunkBlockZ = 0; chunkBlockZ < ChunkComponent.BlockSize; chunkBlockZ++)
            {
                int bz = chunkBlockZ + cz * ChunkComponent.BlockSize;
                for (int chunkBlockX = 0; chunkBlockX < ChunkComponent.BlockSize; chunkBlockX++)
                {
                    int bx = chunkBlockX + cx * ChunkComponent.BlockSize;
                    for (int chunkBlockY = 0; chunkBlockY < ChunkComponent.BlockSize; chunkBlockY++)
                    {
                        int by = chunkBlockY + cy * ChunkComponent.BlockSize;
                        if (bx < blockSize.X && by < blockSize.Y && bz < blockSize.Z && !CullLightFast(blockX, blockY, blockZ, bx, by, bz))
                            return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Determines if the light located at (lbx, lby, lbz) should be considered to be unreachable to the block at (bx, by, bz)
        /// </summary>
        private bool CullLightFast(int lbx, int lby, int lbz, int bx, int by, int bz)
        {
            GameWorld gameWorld = scene.GameWorldInternal;
            int startX = (lbx < bx) ? Math.Max(0, bx - 1) : bx;
            int startY = (lby < by) ? Math.Max(0, by - 1) : by;
            int startZ = (lbz < bz) ? Math.Max(0, bz - 1) : bz;
            int endX = (lbx > bx) ? Math.Min(bx + 1, gameWorld.BlockSize.X - 1) : bx;
            int endY = (lby > by) ? Math.Min(by + 1, gameWorld.BlockSize.Y - 1) : by;
            int endZ = (lbz > bz) ? Math.Min(bz + 1, gameWorld.BlockSize.Z - 1) : bz;

            // Typically this loop ends up being a 2x2x2 grid of checks with one corner being the block (bx, by, bz)
            for (int testX = startX; testX <= endX; testX++)
            {
                for (int testY = startY; testY <= endY; testY++)
                {
                    for (int testZ = startZ; testZ <= endZ; testZ++)
                    {
                        if (gameWorld.NoBlocksInLine(lbx, lby, lbz, testX, testY, testZ))
                            return false;
                    }
                }
            }
            return true;
        }
        #endregion
    }
}
