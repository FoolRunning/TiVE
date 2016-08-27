using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using ProdigalSoftware.TiVE.Core;
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

    internal sealed class GameWorldLightData
    {
        #region Constants
        private const int HalfBlockVoxelSize = BlockLOD32.VoxelSize / 2;
        #endregion

        #region Member variables
        private readonly Scene scene;
        private readonly LightingModel lightingModel;
        private readonly ChunkLights[] chunkLightInfo;
        private Vector3i chunkSize;
        //private volatile int totalComplete;
        //private volatile int lastPercentComplete;
        #endregion

        #region Constructor
        public GameWorldLightData(Scene scene)
        {
            this.scene = scene;

            GameWorld gameWorld = scene.GameWorldInternal;
            chunkSize = new Vector3i((int)Math.Ceiling(gameWorld.BlockSize.X / (float)ChunkComponent.BlockSize),
                (int)Math.Ceiling(gameWorld.BlockSize.Y / (float)ChunkComponent.BlockSize),
                (int)Math.Ceiling(gameWorld.BlockSize.Z / (float)ChunkComponent.BlockSize));

            chunkLightInfo = new ChunkLights[chunkSize.X * chunkSize.Y * chunkSize.Z];

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
                chunkLightInfo[i] = new ChunkLights(100);

            const int numThreads = 1; // Environment.ProcessorCount > 3 ? 2 : 1;
            Thread[] threads = new Thread[numThreads];
            List<LightInfo> lightInfos = new List<LightInfo>(20000);
            lightInfos.Add(null);
            for (int i = 0; i < numThreads; i++)
                threads[i] = StartLightCalculationThread("Light " + (i + 1), lightInfos, i * scene.GameWorld.BlockSize.X / numThreads, (i + 1) * scene.GameWorld.BlockSize.X / numThreads);

            foreach (Thread thread in threads)
                thread.Join();

            //for (int i = 0; i < chunkLightInfo.Length; i++)
            //    chunkLightInfo[i].LightsAffectingChunks.TrimExcess();

            LightList = lightInfos.ToArray();

            sw.Stop();
            Messages.AddDoneText();
            Messages.AddDebug(string.Format("Lighting for {0} lights took {1}ms", lightInfos.Count - 1, sw.ElapsedMilliseconds));

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

        public void CacheLightsInBlocksForChunk(int chunkX, int chunkY, int chunkZ)
        {
            int arrayOffset = chunkSize.GetArrayOffset(chunkX, chunkY, chunkZ);
            if (chunkLightInfo[arrayOffset].BlockLights != null)
                return; // Chunk data is already loaded

            List<ushort> lightsAffectingChunk = chunkLightInfo[arrayOffset].LightsAffectingChunks;

            int maxLightsPerBlock = TiVEController.UserSettings.Get(UserSettings.LightsPerBlockKey);
            ushort[][] blocksLights = new ushort[ChunkComponent.BlockSize * ChunkComponent.BlockSize * ChunkComponent.BlockSize][];
            for (int i = 0; i < blocksLights.Length; i++)
                blocksLights[i] = new ushort[maxLightsPerBlock];

            int startX = chunkX * ChunkComponent.BlockSize;
            int startY = chunkY * ChunkComponent.BlockSize;
            int startZ = chunkZ * ChunkComponent.BlockSize;
            GameWorld gameWorld = scene.GameWorldInternal;
            int sizeX = gameWorld.BlockSize.X;
            int sizeY = gameWorld.BlockSize.Y;
            int sizeZ = gameWorld.BlockSize.Z;
            int endX = Math.Min(sizeX, startX + ChunkComponent.BlockSize);
            int endY = Math.Min(sizeY, startY + ChunkComponent.BlockSize);
            int endZ = Math.Min(sizeZ, startZ + ChunkComponent.BlockSize);
            LightCullType lightCullType = (LightCullType)(int)TiVEController.UserSettings.Get(UserSettings.LightCullingTypeKey);
            for (int lightIndex = 0; lightIndex < lightsAffectingChunk.Count; lightIndex++)
            {
                ushort lightId = lightsAffectingChunk[lightIndex];
                LightInfo lightInfo = LightList[lightId];
                int lbx = lightInfo.BlockX;
                int lby = lightInfo.BlockY;
                int lbz = lightInfo.BlockZ;
                for (int bz = startZ; bz < endZ; bz++)
                {
                    int vz = (bz << BlockLOD32.VoxelSizeBitShift) + HalfBlockVoxelSize;
                    for (int bx = startX; bx < endX; bx++)
                    {
                        int vx = (bx << BlockLOD32.VoxelSizeBitShift) + HalfBlockVoxelSize;
                        for (int by = startY; by < endY; by++)
                        {
                            if ((lightCullType == LightCullType.Fast && CullLightFast(lbx, lby, lbz, bx, by, bz)) ||
                                (lightCullType == LightCullType.Accurate && CullLightAccurate(lbx, lby, lbz, bx, by, bz)))
                            {
                                continue; // The light won't actually hit the block
                            }

                            int vy = (by << BlockLOD32.VoxelSizeBitShift) + HalfBlockVoxelSize;
                            ushort[] blockLights = blocksLights[GetBlockLightOffset(bx - startX, by - startY, bz - startZ)];

                            // Calculate lighting information
                            // Sort lights by highest percentage to lowest
                            float newLightPercentage = lightInfo.GetLightPercentageDiffuse(vx, vy, vz, lightingModel);
                            int leastLightIndex = blockLights.Length;
                            for (int i = 0; i < blockLights.Length; i++)
                            {
                                if (blockLights[i] == 0 || LightList[blockLights[i]].GetLightPercentageDiffuse(vx, vy, vz, lightingModel) < newLightPercentage)
                                {
                                    leastLightIndex = i;
                                    break;
                                }
                            }

                            if (leastLightIndex < blockLights.Length)
                            {
                                for (int i = blockLights.Length - 1; i > leastLightIndex; i--)
                                    blockLights[i] = blockLights[i - 1];

                                blockLights[leastLightIndex] = lightId;
                            }
                        }
                    }
                }
            }

            chunkLightInfo[arrayOffset].BlockLights = blocksLights;
        }

        public ushort[] GetLightsForBlock(int worldBlockX, int worldBlockY, int worldBlockZ)
        {

            int chunkX = worldBlockX / ChunkComponent.BlockSize;
            int chunkY = worldBlockY / ChunkComponent.BlockSize;
            int chunkZ = worldBlockZ / ChunkComponent.BlockSize;
            ushort[][] chunkLights = chunkLightInfo[chunkSize.GetArrayOffset(chunkX, chunkY, chunkZ)].BlockLights;

            int chunkBlockX = worldBlockX % ChunkComponent.BlockSize;
            int chunkBlockY = worldBlockY % ChunkComponent.BlockSize;
            int chunkBlockZ = worldBlockZ % ChunkComponent.BlockSize;
            return chunkLights != null ? chunkLights[GetBlockLightOffset(chunkBlockX, chunkBlockY, chunkBlockZ)] : null;
        }

        public void RemoveLightsForChunk(ChunkComponent chunkData)
        {
            chunkLightInfo[chunkSize.GetArrayOffset(chunkData.ChunkLoc.X, chunkData.ChunkLoc.Y, chunkData.ChunkLoc.Z)].BlockLights = null;
        }
        #endregion

        #region Private helper methods
        private Thread StartLightCalculationThread(string threadName, List<LightInfo> lightInfos, int startX, int endX)
        {
            Thread thread = new Thread(() =>
            {
                GameWorld gameWorld = scene.GameWorldInternal;
                //int sizeX = gameWorld.BlockSize.X;
                int sizeY = gameWorld.BlockSize.Y;
                int sizeZ = gameWorld.BlockSize.Z;

                for (int x = startX; x < endX; x++)
                {
                    //int percentComplete = totalComplete * 100 / (sizeX - 1);
                    //if (percentComplete != lastPercentComplete)
                    //{
                    //    Console.WriteLine("Calculating lighting: {0}%", percentComplete);
                    //    lastPercentComplete = percentComplete;
                    //}

                    for (int z = 0; z < sizeZ; z++)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
                            LightComponent light = gameWorld[x, y, z].GetComponent<LightComponent>();
                            if (light != null)
                                FillChunksWithLight(light, lightInfos, x, y, z);
                        }
                    }
                    //totalComplete++;
                }
            });

            thread.Name = threadName;
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.Normal;
            thread.Start();
            return thread;
        }

        private Thread StartLightPreLoadThread(string threadName, List<ChunkComponent> allChunks, int startIndex, int endIndex)
        {
            Thread thread = new Thread(() =>
            {
                endIndex = Math.Min(endIndex, allChunks.Count);
                for (int i = startIndex; i < endIndex; i++)
                    CacheLightsInBlocksForChunk(allChunks[i].ChunkLoc.X, allChunks[i].ChunkLoc.Y, allChunks[i].ChunkLoc.Z);
            });

            thread.Name = threadName;
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.Normal;
            thread.Start();
            return thread;
        }

        private void FillChunksWithLight(LightComponent light, List<LightInfo> lightInfos, int blockX, int blockY, int blockZ)
        {
            int startX = Math.Max(0, (blockX - light.LightBlockDist) / ChunkComponent.BlockSize);
            int startY = Math.Max(0, (blockY - light.LightBlockDist) / ChunkComponent.BlockSize);
            int startZ = Math.Max(0, (blockZ - light.LightBlockDist) / ChunkComponent.BlockSize);
            int endX = Math.Min(chunkSize.X, (int)Math.Ceiling((blockX + light.LightBlockDist + 1) / (float)ChunkComponent.BlockSize));
            int endY = Math.Min(chunkSize.Y, (int)Math.Ceiling((blockY + light.LightBlockDist + 1) / (float)ChunkComponent.BlockSize));
            int endZ = Math.Min(chunkSize.Z, (int)Math.Ceiling((blockZ + light.LightBlockDist + 1) / (float)ChunkComponent.BlockSize));

            ushort lightIndex;
            lock (lightInfos)
            {
                if (lightInfos.Count == ushort.MaxValue - 1)
                    Messages.AddWarning("Too many lights in game world");
                if (lightInfos.Count == ushort.MaxValue)
                    return; // Too many lights in the world

                lightIndex = (ushort)lightInfos.Count;
                lightInfos.Add(new LightInfo(blockX, blockY, blockZ, light, lightingModel.GetCacheLightCalculation(light),
                    lightingModel.GetCacheLightCalculationForAmbient(light)));
            }

            for (int cz = startZ; cz < endZ; cz++)
            {
                for (int cx = startX; cx < endX; cx++)
                {
                    for (int cy = startY; cy < endY; cy++)
                    {
                        List<ushort> lightsInChunk = chunkLightInfo[chunkSize.GetArrayOffset(cx, cy, cz)].LightsAffectingChunks;
                        lock (lightsInChunk)
                            lightsInChunk.Add(lightIndex);
                    }
                }
            }
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

        /// <summary>
        /// Determines if the light located at (lbx, lby, lbz) should be considered to be unreachable to the block at (bx, by, bz)
        /// </summary>
        private bool CullLightAccurate(int lbx, int lby, int lbz, int bx, int by, int bz)
        {
            GameWorld gameWorld = scene.GameWorldInternal;
            int startX = Math.Max(0, bx - 1);
            int startY = Math.Max(0, by - 1);
            int startZ = Math.Max(0, bz - 1);
            int endX = Math.Min(bx + 1, gameWorld.BlockSize.X - 1);
            int endY = Math.Min(by + 1, gameWorld.BlockSize.Y - 1);
            int endZ = Math.Min(bz + 1, gameWorld.BlockSize.Z - 1);

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

        /// <summary>
        /// Gets the offset into the block lights array of a chunk for the block at the specified location
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetBlockLightOffset(int blockX, int blockY, int blockZ)
        {
            return (blockX * ChunkComponent.BlockSize + blockZ) * ChunkComponent.BlockSize + blockY;
        }
        #endregion

        #region ChunkLights structure
        /// <summary>
        /// Holds lighting information for a single chunk
        /// </summary>
        private struct ChunkLights
        {
            public readonly List<ushort> LightsAffectingChunks;

            public ushort[][] BlockLights;

            public ChunkLights(int maxLightsAffectingChunk)
            {
                LightsAffectingChunks = new List<ushort>(maxLightsAffectingChunk);
                BlockLights = null;
            }
        }
        #endregion
    }
}
