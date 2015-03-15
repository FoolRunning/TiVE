using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using ProdigalSoftware.TiVE.Renderer.Voxels;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Lighting;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.Lighting
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
        private const int HalfBlockVoxelSize = BlockInformation.VoxelSize / 2;
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
        
        public void FillWorldWithLight(ILight light, List<LightInfo> lightInfos, int blockX, int blockY, int blockZ)
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
            ushort[][] lib = lightsInBlocks;

            for (int bz = startZ; bz < endZ; bz++)
            {
                for (int bx = startX; bx < endX; bx++)
                {
                    for (int by = startY; by < endY; by++)
                    {
                        int vx = bx * BlockInformation.VoxelSize + HalfBlockVoxelSize;
                        int vy = by * BlockInformation.VoxelSize + HalfBlockVoxelSize;
                        int vz = bz * BlockInformation.VoxelSize + HalfBlockVoxelSize;
                        float newLightPercentage = lm.GetLightPercentage(lightInfo, vx, vy, vz);
                        if (newLightPercentage < 0.001f)
                            continue; // doesn't affect the block enough to talk about

                        int arrayOffset = GetBlockLightOffset(bx, by, bz);

                        ushort[] lightsInBlock = lib[arrayOffset];
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
                            ILight light = blockList[gameWorld[x, y, z]].Light;
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
                int blockX = voxelX / BlockInformation.VoxelSize;
                int blockY = voxelY / BlockInformation.VoxelSize;
                int blockZ = voxelZ / BlockInformation.VoxelSize;
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

                    if (NoVoxelInLine(lightInfo.VoxelLocX, lightInfo.VoxelLocY, lightInfo.VoxelLocZ, world, voxelX, voxelY, voxelZ))
                        color += lightInfo.LightColor * lightModel.GetLightPercentage(lightInfo, voxelX, voxelY, voxelZ);
                    else
                    {
                        // Simulate a very crude and simple reflective ambient lighting model by using the light reduced by 70% for
                        // voxels in a "shadow".
                        color += lightInfo.LightColor * (lightModel.GetLightPercentage(lightInfo, voxelX, voxelY, voxelZ) * 0.3f);
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
                    int x = lightInfo.VoxelLocX;
                    int y = lightInfo.VoxelLocY;
                    int z = lightInfo.VoxelLocZ;
                    if ((availableMinusX && lightInfo.VoxelLocX <= voxelX && NoVoxelInLine(x, y, z, world, voxelX - 1, voxelY, voxelZ)) ||
                        (availableMinusY && lightInfo.VoxelLocY <= voxelY && NoVoxelInLine(x, y, z, world, voxelX, voxelY - 1, voxelZ)) ||
                        (availableMinusZ && lightInfo.VoxelLocZ <= voxelZ && NoVoxelInLine(x, y, z, world, voxelX, voxelY, voxelZ - 1)) ||
                        (availablePlusX && lightInfo.VoxelLocX >= voxelX && NoVoxelInLine(x, y, z, world, voxelX + voxelSize, voxelY, voxelZ)) ||
                        (availablePlusY && lightInfo.VoxelLocY >= voxelY && NoVoxelInLine(x, y, z, world, voxelX, voxelY + voxelSize, voxelZ)) ||
                        (availablePlusZ && lightInfo.VoxelLocZ >= voxelZ && NoVoxelInLine(x, y, z, world, voxelX, voxelY, voxelZ + voxelSize)))
                    {
                        color += lightInfo.LightColor * lightModel.GetLightPercentage(lightInfo, voxelX, voxelY, voxelZ);
                    }
                    else
                    {
                        // Simulate a very crude and simple reflective ambient lighting model by using the light reduced by 70% for
                        // voxels in a "shadow".
                        color += lightInfo.LightColor * (lightModel.GetLightPercentage(lightInfo, voxelX, voxelY, voxelZ) * 0.3f);
                    }
                }
                return color;
            }

            /// <summary>
            /// Voxel transversal algorithm taken from: http://www.cse.chalmers.se/edu/year/2011/course/TDA361_Computer_Graphics/grid.pdf
            /// Modified with small optimizations for TiVE.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool NoVoxelInLine(int x, int y, int z, GameWorld gameWorld, int voxelX, int voxelY, int voxelZ)
            {
                if (x == voxelX && y == voxelY && z == voxelZ)
                    return true;

                int stepX = x < voxelX ? 1 : -1;
                int stepY = y < voxelY ? 1 : -1;
                int stepZ = z < voxelZ ? 1 : -1;

                // Because all voxels in TiVE have a size of 1.0, this simplifies the calculation of the delta considerably.
                // We also don't have to worry about specifically handling a divide-by-zero as .Net makes the result Infinity
                // which works just fine for this algorithm.
                float tDeltaX = 1.0f / Math.Abs(voxelX - x);
                float tDeltaY = 1.0f / Math.Abs(voxelY - y);
                float tDeltaZ = 1.0f / Math.Abs(voxelZ - z);
                float tMaxX = tDeltaX;
                float tMaxY = tDeltaY;
                float tMaxZ = tDeltaZ;

                do
                {
                    if (tMaxX < tMaxY)
                    {
                        if (tMaxX < tMaxZ)
                        {
                            x = x + stepX;
                            tMaxX = tMaxX + tDeltaX;
                        }
                        else
                        {
                            z = z + stepZ;
                            tMaxZ = tMaxZ + tDeltaZ;
                        }
                    }
                    else if (tMaxY < tMaxZ)
                    {
                        y = y + stepY;
                        tMaxY = tMaxY + tDeltaY;
                    }
                    else
                    {
                        z = z + stepZ;
                        tMaxZ = tMaxZ + tDeltaZ;
                    }

                    if (x == voxelX && y == voxelY && z == voxelZ)
                        return true;
                }
                while (gameWorld.VoxelEmptyForLighting(x, y, z));

                return false;
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
                int blockX = voxelX / BlockInformation.VoxelSize;
                int blockY = voxelY / BlockInformation.VoxelSize;
                int blockZ = voxelZ / BlockInformation.VoxelSize;
                return GetLightAt(voxelX, voxelY, voxelZ, 1, blockX, blockY, blockZ, VoxelSides.None);
            }

            public override Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, int voxelSize, int blockX, int blockY, int blockZ, VoxelSides visibleSides)
            {
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
                    color += lightInfo.LightColor * lightModel.GetLightPercentage(lightInfo, voxelX, voxelY, voxelZ);
                }
                return color;
            }
        }
        #endregion
    }
}
