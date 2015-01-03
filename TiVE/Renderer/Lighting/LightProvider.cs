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
        ClearSimpleLights = 1,
        ClearRealisticLights = 2,
        CalculateSimpleLights = 4,
        CalculateRealisticLights = 8,
        ClearAllLights = ClearSimpleLights | ClearRealisticLights,
        CalculateAllLights = CalculateSimpleLights | CalculateRealisticLights
    }

    internal sealed class LightProvider
    {
        private const int NumLightCalculationThreads = 5;
        private const int MaxLightsPerBlock = 10;
        private const int HalfBlockVoxelSize = BlockInformation.VoxelSize / 2;

        private readonly Vector3i blockSize;
        private readonly BlockLightInfo[] blockLightInfo;
        private readonly GameWorld gameWorld;
        private readonly bool useSimpleLighting;
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
            useSimpleLighting = TiVEController.UserSettings.Get(UserSettings.SimpleLightingKey);
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
            int maxLightBlockDist = (int)light.LightBlockDist; // LightUtils.GetLightBlockDist(light);

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
                        int vx = bx * BlockInformation.VoxelSize + HalfBlockVoxelSize;
                        int vy = by * BlockInformation.VoxelSize + HalfBlockVoxelSize;
                        int vz = bz * BlockInformation.VoxelSize + HalfBlockVoxelSize;
                        float newLightPercentage = LightUtils.GetLightPercentage(lightInfo, vx, vy, vz);
                        List<LightInfo> blockLights = GetLights(bx, by, bz);
                        using (new PerformanceLock(blockLights))
                        {
                            if (calcSimpleLights)
                            {
                                Color3f currentBlockLight = GetBlockLight(bx, by, bz);
                                SetBlockLight(bx, by, bz, currentBlockLight + (light.Color * newLightPercentage));
                            }

                            if (!calcRealisticLights)
                                continue;

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

            if (useSimpleLighting) // TODO: make this much more efficient
                return ambientLight + blockLightInfo[GetBlockOffset(blockX, blockY, blockZ)].BlockLight;

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

            Thread[] threads = new Thread[NumLightCalculationThreads];
            for (int i = 0; i < NumLightCalculationThreads; i++)
            {
                threads[i] = StartLightCalculationThread("Light " + (i + 1), 
                    i * blockSize.X / NumLightCalculationThreads, (i + 1) * blockSize.X / NumLightCalculationThreads, options);
            }

            foreach (Thread thread in threads)
                thread.Join();

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
