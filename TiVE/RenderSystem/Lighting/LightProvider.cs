using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using ProdigalSoftware.TiVE.RenderSystem.Voxels;
using ProdigalSoftware.TiVE.RenderSystem.World;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.RenderSystem.Lighting
{
    #region LightComplexity enum
    internal enum LightComplexity
    {
        Simple,
        Realistic
    }
    #endregion

    internal abstract class LightProvider : IDisposable
    {
        #region Constants
        private const float ReflectiveLightingFactor = 0.3f;

        private const int HalfBlockVoxelSize = Block.VoxelSize / 2;
        #endregion

        #region Member variables
        private readonly GameWorld gameWorld;
        private readonly Vector3i blockSize;
        private readonly ushort[][] lightsInBlocks;
        private readonly LightingModel lightingModel;
        private LightInfo[] lightInfoList;
        private volatile int totalComplete;
        private volatile int lastPercentComplete;
        #endregion

        #region Constructor/singleton getter
        private LightProvider(GameWorld gameWorld)
        {
            this.gameWorld = gameWorld;
            blockSize = new Vector3i(gameWorld.BlockSize.X, gameWorld.BlockSize.Y, gameWorld.BlockSize.Z);

            AmbientLight = Color3f.Empty;
            lightingModel = LightingModel.Get(gameWorld.LightingModelType);

            lightsInBlocks = new ushort[gameWorld.BlockSize.X * gameWorld.BlockSize.Y * gameWorld.BlockSize.Z][];
        }

        public void Dispose()
        {
            lightInfoList = null;
            for (int i = 0; i < lightsInBlocks.Length; i++)
            {
                if (lightsInBlocks[i] != null)
                    Array.Clear(lightsInBlocks[i], 0, lightsInBlocks[i].Length);
            }
        }

        /// <summary>
        /// Gets a light provider for the specified game world using the current user settings to determine the complexity
        /// </summary>
        public static LightProvider Get(GameWorld gameWorld)
        {
            switch ((LightComplexity)(int)TiVEController.UserSettings.Get(UserSettings.LightingComplexityKey))
            {
                case LightComplexity.Realistic: return new RealisticLightProvider(gameWorld);
                default: return new SimpleLightProvider(gameWorld);
            }
        }
        #endregion

        #region Properties
        public Color3f AmbientLight { get; set; }
        #endregion

        #region Public methods
        /// <summary>
        /// Gets the light value at the specified voxel
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        public abstract Color3f GetLightAtFast(int voxelX, int voxelY, int voxelZ);

        /// <summary>
        /// Gets the light value at the specified voxel. This version is faster if the caller already has the other parameters
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        public abstract Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, int voxelSize, int blockX, int blockY, int blockZ, VoxelSides visibleSides);

        public void Calculate(BlockList blockList, bool clearLightInfo)
        {
            Messages.Print("Initializing lighting...");
            Stopwatch sw = Stopwatch.StartNew();
            int maxLightsPerBlock = TiVEController.UserSettings.Get(UserSettings.LightsPerBlockKey);
            for (int i = 0; i < lightsInBlocks.Length; i++)
            {
                if (lightsInBlocks[i] == null)
                    lightsInBlocks[i] = new ushort[maxLightsPerBlock];
            }
            Messages.AddDoneText();
            Messages.AddDebug(string.Format("Light initialization took {0}ms", sw.ElapsedMilliseconds));

            Messages.Print("Calculating static lighting...");

            sw.Restart();
            if (clearLightInfo)
                Dispose();

            int numThreads = Environment.ProcessorCount > 2 ? Environment.ProcessorCount - 1 : 1;
            List<LightInfo> lightInfos = new List<LightInfo>(2000);
            lightInfos.Add(new LightInfo());
            Thread[] threads = new Thread[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                threads[i] = StartLightCalculationThread("Light " + (i + 1), blockList, lightInfos,
                    i * blockSize.X / numThreads, (i + 1) * blockSize.X / numThreads);
            }

            foreach (Thread thread in threads)
                thread.Join();

            sw.Stop();

            lightInfoList = lightInfos.ToArray();

            Messages.AddDoneText();

            Messages.AddDebug(string.Format("Lighting for {0} lights took {1}ms", lightInfoList.Length - 1, sw.ElapsedMilliseconds));
        }
        
        public void FillWorldWithLight(LightComponent light, List<LightInfo> lightInfos, int blockX, int blockY, int blockZ)
        {
            int sizeX = blockSize.X;
            int sizeY = blockSize.Y;
            int sizeZ = blockSize.Z;
            int maxLightBlockDist = light.LightBlockDist;

            int startX = Math.Max(0, blockX - maxLightBlockDist);
            int startY = Math.Max(0, blockY - maxLightBlockDist);
            int startZ = Math.Max(0, blockZ - maxLightBlockDist);
            int endX = Math.Min(sizeX, blockX + maxLightBlockDist);
            int endY = Math.Min(sizeY, blockY + maxLightBlockDist);
            int endZ = Math.Min(sizeZ, blockZ + maxLightBlockDist);
            LightingModel lm = lightingModel;
            LightInfo lightInfo = new LightInfo(blockX, blockY, blockZ, light, lm.GetCacheLightCalculation(light));
            ushort lightIndex;
            lock (lightInfos)
            {
                int lightCount = lightInfos.Count;
                if (lightCount >= ushort.MaxValue)
                    return; // Too many lights in the world
                lightIndex = (ushort)lightCount;
                lightInfos.Add(lightInfo);
            }

            for (int bz = startZ; bz < endZ; bz++)
            {
                for (int bx = startX; bx < endX; bx++)
                {
                    for (int by = startY; by < endY; by++)
                    {
                        if (!gameWorld.LessThanBlockCountInLine(blockX, blockY, blockZ, bx, by, bz, 2) &&
                            !gameWorld.LessThanBlockCountInLine(bx, by, bz, blockX, blockY, blockZ, 2)/* &&
                            (bx == 0 || !gameWorld.LessThanBlockCountInLine(blockX, blockY, blockZ, bx - 1, by, bz, 2)) &&
                            (bx == sizeX - 1 || !gameWorld.LessThanBlockCountInLine(blockX, blockY, blockZ, bx + 1, by, bz, 2)) &&
                            (by == 0 || !gameWorld.LessThanBlockCountInLine(blockX, blockY, blockZ, bx, by - 1, bz, 2)) &&
                            (by == sizeY - 1 || !gameWorld.LessThanBlockCountInLine(blockX, blockY, blockZ, bx, by + 1, bz, 2)) &&
                            (bz == 0 || !gameWorld.LessThanBlockCountInLine(blockX, blockY, blockZ, bx, by, bz - 1, 2)) &&
                            (bz == sizeZ - 1 || !gameWorld.LessThanBlockCountInLine(blockX, blockY, blockZ, bx, by, bz + 1, 2))*/)
                        {
                            continue; // Unlikely the light will actually hit the block at all
                        }

                        int vx = bx * Block.VoxelSize + HalfBlockVoxelSize;
                        int vy = by * Block.VoxelSize + HalfBlockVoxelSize;
                        int vz = bz * Block.VoxelSize + HalfBlockVoxelSize;
                        float newLightPercentage = lm.GetLightPercentage(lightInfo, vx, vy, vz);
                        if (newLightPercentage < LightingModel.MinRealisticLightPercent)
                            continue; // doesn't affect the block enough to talk about

                        int arrayOffset = GetBlockLightOffset(bx, by, bz);

                        ushort[] lightsInBlock = lightsInBlocks[arrayOffset];
                        lock (lightsInBlock)
                        {
                            // Calculate lighting information
                            // Sort lights by highest percentage to lowest
                            int leastLightIndex = lightsInBlock.Length;
                            for (int i = 0; i < lightsInBlock.Length; i++)
                            {
                                ushort otherInfo = lightsInBlock[i];
                                if (otherInfo == 0 || lm.GetLightPercentage(lightInfos[otherInfo], vx, vy, vz) < newLightPercentage)
                                {
                                    leastLightIndex = i;
                                    break;
                                }
                            }

                            if (leastLightIndex < lightsInBlock.Length)
                            {
                                for (int i = lightsInBlock.Length - 1; i > leastLightIndex; i--)
                                    lightsInBlock[i] = lightsInBlock[i - 1];

                                lightsInBlock[leastLightIndex] = lightIndex;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Private helper methods
        private Thread StartLightCalculationThread(string threadName, BlockList blockList, List<LightInfo> lightInfos, int startX, int endX)
        {
            Thread thread = new Thread(() =>
            {
                int sizeX = blockSize.X;
                int sizeY = blockSize.Y;
                int sizeZ = blockSize.Z;

                for (int x = startX; x < endX; x++)
                {
                    int percentComplete = totalComplete * 100 / (sizeX - 1);
                    if (percentComplete != lastPercentComplete)
                    {
                        //Console.WriteLine("Calculating lighting: {0}%", percentComplete);
                        lastPercentComplete = percentComplete;
                    }

                    for (int z = 0; z < sizeZ; z++)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
                            LightComponent light = blockList[gameWorld[x, y, z]].GetComponent<LightComponent>();
                            if (light != null)
                                FillWorldWithLight(light, lightInfos, x, y, z);
                        }
                    }
                    totalComplete++;
                }
            });

            thread.Name = threadName;
            thread.Start();
            return thread;
        }

        /// <summary>
        /// Gets the offset into the block lights array for the block at the specified location
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlockLightOffset(int blockX, int blockY, int blockZ)
        {
            Vector3i size = blockSize;
            MiscUtils.CheckConstraints(blockX, blockY, blockZ, size);
            return (blockX * size.Z + blockZ) * size.Y + blockY;
        }
        #endregion

        #region RealisticLightProvider class
        private sealed class RealisticLightProvider : LightProvider
        {
            public RealisticLightProvider(GameWorld gameWorld) : base(gameWorld)
            {
            }

            public override Color3f GetLightAtFast(int voxelX, int voxelY, int voxelZ)
            {
                int blockX = voxelX / Block.VoxelSize;
                int blockY = voxelY / Block.VoxelSize;
                int blockZ = voxelZ / Block.VoxelSize;
                ushort[] lightsInBlock = lightsInBlocks[GetBlockLightOffset(blockX, blockY, blockZ)];
                LightingModel lightModel = lightingModel;
                GameWorld world = gameWorld;
                Color3f color = AmbientLight;
                LightInfo[] lights = lightInfoList;
                for (int i = 0; i < lightsInBlock.Length; i++)
                {
                    ushort lightIndex = lightsInBlock[i];
                    if (lightIndex == 0)
                        break;
                    LightInfo lightInfo = lights[lightIndex];

                    if (world.NoVoxelInLine(lightInfo.VoxelLocX, lightInfo.VoxelLocY, lightInfo.VoxelLocZ, voxelX, voxelY, voxelZ))
                        color += lightInfo.LightColor * lightModel.GetLightPercentage(lightInfo, voxelX, voxelY, voxelZ);
                    else
                    {
                        // Simulate a very crude and simple reflective ambient lighting model by using the light reduced by a small 
                        // amount for voxels in a "shadow".
                        color += lightInfo.LightColor * (lightModel.GetLightPercentage(lightInfo, voxelX, voxelY, voxelZ) * ReflectiveLightingFactor);
                    }
                }
                return color;
            }

            public override Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, int voxelSize, int blockX, int blockY, int blockZ, VoxelSides visibleSides)
            {
                bool availableMinusX = (visibleSides & VoxelSides.Left) != 0;
                bool availableMinusY = (visibleSides & VoxelSides.Bottom) != 0;
                bool availableMinusZ = (visibleSides & VoxelSides.Back) != 0;
                bool availablePlusX = (visibleSides & VoxelSides.Right) != 0;
                bool availablePlusY = (visibleSides & VoxelSides.Top) != 0;
                bool availablePlusZ = (visibleSides & VoxelSides.Front) != 0;

                LightingModel lightModel = lightingModel;
                GameWorld world = gameWorld;
                Color3f color = AmbientLight;
                LightInfo[] lights = lightInfoList;
                ushort[] lightsInBlock = lightsInBlocks[GetBlockLightOffset(blockX, blockY, blockZ)];
                for (int i = 0; i < lightsInBlock.Length; i++)
                {
                    ushort lightIndex = lightsInBlock[i];
                    if (lightIndex == 0)
                        break;

                    LightInfo lightInfo = lights[lightIndex];
                    int lx = lightInfo.VoxelLocX;
                    int ly = lightInfo.VoxelLocY;
                    int lz = lightInfo.VoxelLocZ;
                    if ((availableMinusX && lx <= voxelX && world.NoVoxelInLine(lx, ly, lz, voxelX - 1, voxelY, voxelZ)) ||
                        (availableMinusY && ly <= voxelY && world.NoVoxelInLine(lx, ly, lz, voxelX, voxelY - 1, voxelZ)) ||
                        (availableMinusZ && lz <= voxelZ && world.NoVoxelInLine(lx, ly, lz, voxelX, voxelY, voxelZ - 1)) ||
                        (availablePlusX && lx >= voxelX && world.NoVoxelInLine(lx, ly, lz, voxelX + voxelSize, voxelY, voxelZ)) ||
                        (availablePlusY && ly >= voxelY && world.NoVoxelInLine(lx, ly, lz, voxelX, voxelY + voxelSize, voxelZ)) ||
                        (availablePlusZ && lz >= voxelZ && world.NoVoxelInLine(lx, ly, lz, voxelX, voxelY, voxelZ + voxelSize)))
                    {
                        color += lightInfo.LightColor * lightModel.GetLightPercentage(lightInfo, voxelX, voxelY, voxelZ);
                    }
                    else if ((availableMinusX && lx <= voxelX) || (availableMinusY && ly <= voxelY) || (availableMinusZ && lz <= voxelZ) ||
                        (availablePlusX && lx >= voxelX) || (availablePlusY && ly >= voxelY) || (availablePlusZ && lz >= voxelZ))
                    {
                        // Simulate a very crude and simple reflective ambient lighting model by using the light reduced by a small 
                        // amount for voxels in a "shadow".
                        color += lightInfo.LightColor * (lightModel.GetLightPercentage(lightInfo, voxelX, voxelY, voxelZ) * ReflectiveLightingFactor);
                    }
                }
                return color;
            }
        }
        #endregion

        #region SimpleLightProvider class
        private sealed class SimpleLightProvider : LightProvider
        {
            public SimpleLightProvider(GameWorld gameWorld) : base(gameWorld)
            {
            }

            public override Color3f GetLightAtFast(int voxelX, int voxelY, int voxelZ)
            {
                ushort[] lightsInBlock = lightsInBlocks[GetBlockLightOffset(voxelX / Block.VoxelSize, voxelY / Block.VoxelSize, voxelZ / Block.VoxelSize)];
                LightingModel lightModel = lightingModel;
                Color3f color = AmbientLight;
                LightInfo[] lights = lightInfoList;
                for (int i = 0; i < lightsInBlock.Length; i++)
                {
                    ushort lightIndex = lightsInBlock[i];
                    if (lightIndex == 0)
                        break;

                    LightInfo lightInfo = lights[lightIndex];
                    color += lightInfo.LightColor * lightModel.GetLightPercentage(lightInfo, voxelX, voxelY, voxelZ);
                }
                return color;
            }

            public override Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, int voxelSize, int blockX, int blockY, int blockZ, VoxelSides visibleSides)
            {
                bool availableMinusX = (visibleSides & VoxelSides.Left) != 0;
                bool availableMinusY = (visibleSides & VoxelSides.Bottom) != 0;
                bool availableMinusZ = (visibleSides & VoxelSides.Back) != 0;
                bool availablePlusX = (visibleSides & VoxelSides.Right) != 0;
                bool availablePlusY = (visibleSides & VoxelSides.Top) != 0;
                bool availablePlusZ = (visibleSides & VoxelSides.Front) != 0;

                ushort[] lightsInBlock = lightsInBlocks[GetBlockLightOffset(blockX, blockY, blockZ)];
                LightingModel lightModel = lightingModel;
                Color3f color = AmbientLight;
                LightInfo[] lights = lightInfoList;
                for (int i = 0; i < lightsInBlock.Length; i++)
                {
                    ushort lightIndex = lightsInBlock[i];
                    if (lightIndex == 0)
                        break;

                    LightInfo lightInfo = lights[lightIndex];

                    int lx = lightInfo.VoxelLocX;
                    int ly = lightInfo.VoxelLocY;
                    int lz = lightInfo.VoxelLocZ;
                    if ((availableMinusX && lx <= voxelX) || (availableMinusY && ly <= voxelY) || (availableMinusZ && lz <= voxelZ) ||
                        (availablePlusX && lx >= voxelX) || (availablePlusY && ly >= voxelY) || (availablePlusZ && lz >= voxelZ))
                    {
                        color += lightInfo.LightColor * lightModel.GetLightPercentage(lightInfo, voxelX, voxelY, voxelZ);
                    }
                }
                return color;
            }
        }
        #endregion
    }
}
