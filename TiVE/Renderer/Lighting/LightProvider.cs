using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Lighting;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.Lighting
{
    [Flags]
    public enum CalcOptions
    {
        None = 0,
        ClearLights = 1,
        CalculateLights = 2
    }

    internal abstract class LightProvider
    {
        private const int NumLightCalculationThreads = 4;
        private const int MaxLightsPerBlock = 10;
        private const int HalfBlockVoxelSize = BlockInformation.VoxelSize / 2;

        private readonly Vector3i blockSize;
        private readonly BlockLightInfo[] blockLightInfo;
        private readonly GameWorld gameWorld;
        private readonly LightingModel lightingModel;
        private volatile int totalComplete;
        private volatile int lastPercentComplete;

        private Color3f ambientLight;

        private LightProvider(GameWorld gameWorld)
        {
            this.gameWorld = gameWorld;
            blockSize = new Vector3i(gameWorld.BlockSize.X, gameWorld.BlockSize.Y, gameWorld.BlockSize.Z);

            blockLightInfo = new BlockLightInfo[gameWorld.BlockSize.X * gameWorld.BlockSize.Y * gameWorld.BlockSize.Z];
            for (int i = 0; i < blockLightInfo.Length; i++)
                blockLightInfo[i] = new BlockLightInfo(MaxLightsPerBlock);

            ambientLight = Color3f.Empty;
            lightingModel = LightingModel.Get(gameWorld.LightingModelType);
        }

        public static LightProvider Get(GameWorld gameWorld)
        {
            if (TiVEController.UserSettings.Get(UserSettings.SimpleLightingKey))
                return new SimpleLightProvider(gameWorld);
            return new SmoothLightProvider(gameWorld);
        }

        public Color3f AmbientLight
        {
            get { return ambientLight; }
            set { ambientLight = value; }
        }

        /// <summary>
        /// Gets the light value at the specified voxel
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // Probably can't be inlined because it's an abstract method, but try anyways
        public abstract Color3f GetLightAt(int voxelX, int voxelY, int voxelZ);

        public void FillWorldWithLight(ILight light, int blockX, int blockY, int blockZ)
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
            LightInfo lightInfo = new LightInfo(blockX, blockY, blockZ, light, lightingModel.GetCacheLightCalculation(light));

            for (int bz = startZ; bz < endZ; bz++)
            {
                for (int bx = startX; bx < endX; bx++)
                {
                    for (int by = startY; by < endY; by++)
                    {
                        int vx = bx * BlockInformation.VoxelSize + HalfBlockVoxelSize;
                        int vy = by * BlockInformation.VoxelSize + HalfBlockVoxelSize;
                        int vz = bz * BlockInformation.VoxelSize + HalfBlockVoxelSize;
                        float newLightPercentage = lightingModel.GetLightPercentage(lightInfo, vx, vy, vz);
                        List<LightInfo> blockLights = GetLights(bx, by, bz);
                        using (new PerformanceLock(blockLights))
                        {
                            // Calculate simple lighting information
                            Color3f currentBlockLight = GetBlockLight(bx, by, bz);
                            SetBlockLight(bx, by, bz, currentBlockLight + (light.Color * newLightPercentage));

                            // Calculate smooth lighting information
                            if (blockLights.Count == 0)
                                blockLights.Add(lightInfo);
                            else
                            {
                                // Sort lights by highest percentage to lowest
                                int leastLightIndex = blockLights.Count;
                                for (int i = 0; i < blockLights.Count; i++)
                                {
                                    float lightPercentage = lightingModel.GetLightPercentage(blockLights[i], vx, vy, vz);
                                    if (lightPercentage < newLightPercentage)
                                    {
                                        leastLightIndex = i;
                                        break;
                                    }
                                }

                                if (leastLightIndex < blockLights.Count || blockLights.Count < MaxLightsPerBlock)
                                {
                                    if (blockLights.Count >= MaxLightsPerBlock)
                                        blockLights.RemoveAt(blockLights.Count - 1);
                                    blockLights.Insert(leastLightIndex, lightInfo);

                                    Debug.Assert(blockLights.Count <= MaxLightsPerBlock, "Should never add more then max lights");
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Calculate(CalcOptions options)
        {
            if (options == CalcOptions.None)
                return;

            Messages.Print("Calculating static lighting...");

            if ((options & CalcOptions.ClearLights) != 0)
            {
                for (int z = 0; z < blockSize.Z; z++)
                {
                    for (int x = 0; x < blockSize.X; x++)
                    {
                        for (int y = 0; y < blockSize.Y; y++)
                        {
                            SetBlockLight(x, y, z, Color3f.Empty);
                            GetLights(x, y, z).Clear();
                        }
                    }
                }
            }

            Stopwatch sw = Stopwatch.StartNew();
            if ((options & CalcOptions.CalculateLights) != 0)
            {
                Thread[] threads = new Thread[NumLightCalculationThreads];
                for (int i = 0; i < NumLightCalculationThreads; i++)
                {
                    threads[i] = StartLightCalculationThread("Light " + (i + 1),
                        i * blockSize.X / NumLightCalculationThreads, (i + 1) * blockSize.X / NumLightCalculationThreads);
                }

                foreach (Thread thread in threads)
                    thread.Join();
            }
            Messages.AddDoneText();

            sw.Stop();
            Console.WriteLine("Lighting took {0}ms", sw.ElapsedTicks * 1000.0f / Stopwatch.Frequency);
        }

        private Thread StartLightCalculationThread(string threadName, int startX, int endX)
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
                            ILight light = gameWorld[x, y, z].Light;
                            if (light != null)
                                FillWorldWithLight(light, x, y, z);
                        }
                    }
                    totalComplete++;
                }
            });

            thread.Name = threadName;
            thread.Start();
            return thread;
        }

        private List<LightInfo> GetLights(int blockX, int blockY, int blockZ)
        {
            return blockLightInfo[GetBlockOffset(blockX, blockY, blockZ)].Lights;
        }

        private Color3f GetBlockLight(int blockX, int blockY, int blockZ)
        {
            return blockLightInfo[GetBlockOffset(blockX, blockY, blockZ)].BlockLight;
        }

        private void SetBlockLight(int blockX, int blockY, int blockZ, Color3f light)
        {
            blockLightInfo[GetBlockOffset(blockX, blockY, blockZ)].BlockLight = light;
        }

        #region Private helper methods
        /// <summary>
        /// Gets the offset into the game world blocks array for the block at the specified location
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlockOffset(int x, int y, int z)
        {
            MiscUtils.CheckConstraints(x, y, z, blockSize);
            return (x * blockSize.Z + z) * blockSize.Y + y; // y-axis major for speed
        }
        #endregion

        #region SmoothLightProvider class
        private sealed class SmoothLightProvider : LightProvider
        {
            public SmoothLightProvider(GameWorld gameWorld) : base(gameWorld)
            {
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)] // Probably can't be inlined because it's an abstract method, but try anyways
            public override Color3f GetLightAt(int voxelX, int voxelY, int voxelZ)
            {
                int blockX = voxelX / BlockInformation.VoxelSize;
                int blockY = voxelY / BlockInformation.VoxelSize;
                int blockZ = voxelZ / BlockInformation.VoxelSize;

                Color3f color = ambientLight;
                List<LightInfo> blockLights = blockLightInfo[GetBlockOffset(blockX, blockY, blockZ)].Lights;
                for (int i = 0; i < blockLights.Count; i++)
                {
                    LightInfo lightInfo = blockLights[i];
                    color += lightInfo.Light.Color * lightingModel.GetLightPercentage(lightInfo, voxelX, voxelY, voxelZ);
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)] // Probably can't be inlined because it's an abstract method, but try anyways
            public override Color3f GetLightAt(int voxelX, int voxelY, int voxelZ)
            {
                int blockX = voxelX / BlockInformation.VoxelSize;
                int blockY = voxelY / BlockInformation.VoxelSize;
                int blockZ = voxelZ / BlockInformation.VoxelSize;
                return ambientLight + blockLightInfo[GetBlockOffset(blockX, blockY, blockZ)].BlockLight;
            }
        }
        #endregion

        #region Block class
        /// <summary>
        /// Represents the light information for one block in the game world
        /// </summary>
        private struct BlockLightInfo
        {
            /// <summary>
            /// List of lights that affect this block
            /// </summary>
            public readonly List<LightInfo> Lights;

            public Color3f BlockLight;

            public BlockLightInfo(int lightListSize)
            {
                Lights = new List<LightInfo>(lightListSize);
                BlockLight = new Color3f();
            }
        }
        #endregion
    }
}
