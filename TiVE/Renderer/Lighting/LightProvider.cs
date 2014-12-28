using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
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
        ClearSimpleLights = 1,
        ClearRealisticLights = 2,
        CalculateSimpleLights = 4,
        CalculateRealisticLights = 8,
        ClearAllLights = ClearSimpleLights | ClearRealisticLights,
        CalculateAllLights = CalculateSimpleLights | CalculateRealisticLights
    }

    internal sealed class LightProvider
    {
        private const int MaxLightsPerBlock = 10;
        private const float MinLightValue = 0.002f; // 0.002f (0.2%) produces the best result as that is less then a single light value's worth

        private readonly Vector3i blockSize;
        private readonly BlockLightInfo[] blockLightInfo;
        private readonly GameWorld gameWorld;
        private volatile int totalComplete;
        private volatile int lastPercentComplete;

        private Color3f ambientLight;

        public LightProvider(GameWorld gameWorld)
        {
            this.gameWorld = gameWorld;
            blockSize = new Vector3i(gameWorld.BlockSize.X, gameWorld.BlockSize.Y, gameWorld.BlockSize.Z);

            blockLightInfo = new BlockLightInfo[gameWorld.BlockSize.X * gameWorld.BlockSize.Y * gameWorld.BlockSize.Z];
            for (int i = 0; i < blockLightInfo.Length; i++)
                blockLightInfo[i] = new BlockLightInfo(MaxLightsPerBlock);

            ambientLight = new Color3f(0, 0, 0);
        }

        public Color3f AmbientLight
        {
            get { return ambientLight; }
            set { ambientLight = value; }
        }

        public void FillWorldWithLight(ILight light, int blockX, int blockY, int blockZ, CalcOptions options)
        {
            if ((options & CalcOptions.CalculateAllLights) == 0)
                return;

            bool calcSimpleLights = (options & CalcOptions.CalculateSimpleLights) != 0;
            bool calcRealisticLights = (options & CalcOptions.CalculateRealisticLights) != 0;

            int sizeX = blockSize.X;
            int sizeY = blockSize.Y;
            int sizeZ = blockSize.Z;
            float maxVoxelDist = (float)Math.Sqrt(1.0 / (light.Attenuation * MinLightValue));
            int maxLightBlockDist = (int)Math.Ceiling(maxVoxelDist / BlockInformation.VoxelSize);

            int startX = Math.Max(0, blockX - maxLightBlockDist);
            int startY = Math.Max(0, blockY - maxLightBlockDist);
            int startZ = Math.Max(0, blockZ - maxLightBlockDist);
            int endX = Math.Min(sizeX, blockX + maxLightBlockDist);
            int endY = Math.Min(sizeY, blockY + maxLightBlockDist);
            int endZ = Math.Min(sizeZ, blockZ + maxLightBlockDist);
            LightInfo lightInfo = new LightInfo(blockX, blockY, blockZ, light);

            for (int bz = startZ; bz < endZ; bz++)
            {
                for (int bx = startX; bx < endX; bx++)
                {
                    for (int by = startY; by < endY; by++)
                    {
                        int vx = bx * BlockInformation.VoxelSize + 5;
                        int vy = by * BlockInformation.VoxelSize + 5;
                        int vz = bz * BlockInformation.VoxelSize + 5;
                        float newLightPercentage = LightUtils.GetLightPercentage(lightInfo, vx, vy, vz);
                        if (calcSimpleLights)
                        {
                            Color3f currentBlockLight = GetBlockLight(bx, by, bz);
                            SetBlockLight(bx, by, bz, currentBlockLight + (light.Color * newLightPercentage));
                        }

                        if (!calcRealisticLights)
                            continue;

                        List<LightInfo> blockLights = GetLights(bx, by, bz);
                        using (new PerformanceLock(blockLights))
                        {
                            if (blockLights.Count == 0)
                                blockLights.Add(lightInfo);
                            else
                            {
                                // Sort lights by highest percentage to lowest
                                int leastLightIndex = blockLights.Count;
                                for (int i = 0; i < blockLights.Count; i++)
                                {
                                    float lightPercentage = LightUtils.GetLightPercentage(blockLights[i], vx, vy, vz);
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

        /// <summary>
        /// Gets the light value at the specified voxel
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color3f GetLightAt(int voxelX, int voxelY, int voxelZ)
        {
            int blockX = voxelX / BlockInformation.VoxelSize;
            int blockY = voxelY / BlockInformation.VoxelSize;
            int blockZ = voxelZ / BlockInformation.VoxelSize;

            //return ambientLight + blockLightInfo[GetBlockOffset(blockX, blockY, blockZ)].BlockLight;

            Color3f color = ambientLight;

            List<LightInfo> blockLights = blockLightInfo[GetBlockOffset(blockX, blockY, blockZ)].Lights;
            for (int i = 0; i < blockLights.Count && i < 10; i++)
            {
                LightInfo lightInfo = blockLights[i];
                color += lightInfo.Light.Color * LightUtils.GetLightPercentage(lightInfo, voxelX, voxelY, voxelZ);
            }
            return color;
        }

        public void Calculate(CalcOptions options)
        {
            if (options == CalcOptions.None)
                return;

            Messages.Print("Calculating static lighting...");

            bool clearSimpleLights = (options & CalcOptions.ClearSimpleLights) != 0;
            bool clearRealisticLights = (options & CalcOptions.ClearRealisticLights) != 0;

            if (clearSimpleLights || clearRealisticLights)
            {
                for (int z = 0; z < blockSize.Z; z++)
                {
                    for (int x = 0; x < blockSize.X; x++)
                    {
                        for (int y = 0; y < blockSize.Y; y++)
                        {
                            if (clearSimpleLights)
                                SetBlockLight(x, y, z, Color3f.Empty);
                            if (clearRealisticLights)
                                GetLights(x, y, z).Clear();
                        }
                    }
                }
            }

            Stopwatch sw = Stopwatch.StartNew();
            int mid1 = blockSize.X / 3;
            int mid2 = blockSize.X * 2 / 3;
            Thread thread1 = StartLightCalculationThread("Light 1", 0, mid1, options);
            Thread thread2 = StartLightCalculationThread("Light 2", mid1, mid2, options);
            Thread thread3 = StartLightCalculationThread("Light 3", mid2, blockSize.X, options);

            thread1.Join();
            thread2.Join();
            thread3.Join();

            sw.Stop();
            Messages.AddDoneText();
            Messages.Println(string.Format("Lighting took {0}ms", sw.ElapsedTicks * 1000.0f / Stopwatch.Frequency));
        }

        private Thread StartLightCalculationThread(string threadName, int startX, int endX, CalcOptions options)
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
                                FillWorldWithLight(light, x, y, z, options);
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

        #region Block class
        /// <summary>
        /// Represents one block in the game world
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
