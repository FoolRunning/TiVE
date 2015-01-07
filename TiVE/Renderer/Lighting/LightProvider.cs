using System;
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
        private readonly BlockLightsInfo[] blockLightsInfo;
        private readonly GameWorld gameWorld;
        private readonly LightingModel lightingModel;
        private volatile int totalComplete;
        private volatile int lastPercentComplete;

        private Color3f ambientLight;

        private LightProvider(GameWorld gameWorld)
        {
            this.gameWorld = gameWorld;
            blockSize = new Vector3i(gameWorld.BlockSize.X, gameWorld.BlockSize.Y, gameWorld.BlockSize.Z);

            blockLightsInfo = new BlockLightsInfo[gameWorld.BlockSize.X * gameWorld.BlockSize.Y * gameWorld.BlockSize.Z];
            for (int i = 0; i < blockLightsInfo.Length; i++)
                blockLightsInfo[i] = new BlockLightsInfo(MaxLightsPerBlock);

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
                        LightInfo[] blockLights = blockLightsInfo[GetBlockOffset(bx, by, bz)].Lights;
                        using (new PerformanceLock(blockLights))
                        {
                            // Calculate simple lighting information
                            SetBlockLight(bx, by, bz, GetBlockLight(bx, by, bz) + (light.Color * newLightPercentage));

                            // Calculate smooth lighting information
                            // Sort lights by highest percentage to lowest
                            int leastLightIndex = MaxLightsPerBlock;
                            for (int i = 0; i < blockLights.Length; i++)
                            {
                                LightInfo otherInfo = blockLights[i];
                                if (otherInfo == null)
                                {
                                    leastLightIndex = i;
                                    break;
                                }

                                float otherLightPercentage = lightingModel.GetLightPercentage(otherInfo, vx, vy, vz);
                                if (otherLightPercentage < newLightPercentage)
                                {
                                    leastLightIndex = i;
                                    break;
                                }
                            }

                            if (leastLightIndex < MaxLightsPerBlock)
                            {
                                for (int i = MaxLightsPerBlock - 1; i > leastLightIndex; i--)
                                    blockLights[i] = blockLights[i - 1];

                                blockLights[leastLightIndex] = lightInfo;
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
                            Array.Clear(blockLightsInfo[GetBlockOffset(x, y, z)].Lights, 0, MaxLightsPerBlock);
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
            Messages.AddDebug(string.Format("Lighting took {0}ms", sw.ElapsedMilliseconds));
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

        private Color3f GetBlockLight(int blockX, int blockY, int blockZ)
        {
            return blockLightsInfo[GetBlockOffset(blockX, blockY, blockZ)].BlockLight;
        }

        private void SetBlockLight(int blockX, int blockY, int blockZ, Color3f light)
        {
            blockLightsInfo[GetBlockOffset(blockX, blockY, blockZ)].BlockLight = light;
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
                LightInfo[] blockLights = blockLightsInfo[GetBlockOffset(blockX, blockY, blockZ)].Lights;
                for (int i = 0; i < blockLights.Length; i++)
                {
                    LightInfo lightInfo = blockLights[i];
                    if (lightInfo == null)
                        break;
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
                return ambientLight + blockLightsInfo[GetBlockOffset(blockX, blockY, blockZ)].BlockLight;
            }
        }
        #endregion

        #region Block class
        /// <summary>
        /// Represents the light information for one block in the game world
        /// </summary>
        private struct BlockLightsInfo
        {
            /// <summary>
            /// List of lights that affect this block
            /// </summary>
            public readonly LightInfo[] Lights;

            public Color3f BlockLight;

            public BlockLightsInfo(int lightListSize)
            {
                Lights = new LightInfo[lightListSize];
                BlockLight = new Color3f();
            }
        }
        #endregion
    }
}
