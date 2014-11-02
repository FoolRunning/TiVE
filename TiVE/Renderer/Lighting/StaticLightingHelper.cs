using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Lighting;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.Lighting
{
    internal sealed class StaticLightingHelper
    {
        private readonly GameWorld gameWorld;
        private readonly int maxLightsPerBlock;
        private readonly float minLightValue;
        private volatile int totalComplete;
        private volatile int lastPercentComplete;

        public StaticLightingHelper(GameWorld gameWorld, int maxLightsPerBlock, float minLightValue)
        {
            this.gameWorld = gameWorld;
            this.maxLightsPerBlock = maxLightsPerBlock;
            this.minLightValue = minLightValue;
        }

        public void Calculate()
        {
            Messages.Print("Calculating static lighting...");

            Stopwatch sw = Stopwatch.StartNew();
            int mid1 = gameWorld.BlockSize.X / 3;
            int mid2 = gameWorld.BlockSize.X * 2 / 3;
            Thread thread1 = StartLightCalculationThread("Light 1", 0, mid1);
            Thread thread2 = StartLightCalculationThread("Light 2", mid1, mid2);
            Thread thread3 = StartLightCalculationThread("Light 3", mid2, gameWorld.BlockSize.X);

            thread1.Join();
            thread2.Join();
            thread3.Join();

            sw.Stop();
            Messages.AddDoneText();
            Messages.Println(string.Format("Lighting took {0}ms", sw.ElapsedTicks * 1000.0f / Stopwatch.Frequency));
        }

        private Thread StartLightCalculationThread(string threadName, int startX, int endX)
        {
            Thread thread = new Thread(() =>
            {
                int sizeX = gameWorld.BlockSize.X;
                int sizeY = gameWorld.BlockSize.Y;
                int sizeZ = gameWorld.BlockSize.Z;

                for (int x = startX; x < endX; x++)
                {
                    int percentComplete = totalComplete * 100 / (sizeX - 1);
                    if (percentComplete != lastPercentComplete)
                    {
                        Console.WriteLine("Calculating lighting: {0}%", percentComplete);
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

        private void FillWorldWithLight(ILight light, int blockX, int blockY, int blockZ)
        {
            int sizeX = gameWorld.BlockSize.X;
            int sizeY = gameWorld.BlockSize.Y;
            int sizeZ = gameWorld.BlockSize.Z;
            float maxVoxelDist = (float)Math.Sqrt(1.0 / (light.Attenuation * minLightValue));
            int maxLightBlockDist = (int)Math.Ceiling(maxVoxelDist / BlockInformation.VoxelSize);

            int startX = Math.Max(0, blockX - maxLightBlockDist);
            int startY = Math.Max(0, blockY - maxLightBlockDist);
            int startZ = Math.Max(0, blockZ - maxLightBlockDist);
            int endX = Math.Min(sizeX, blockX + maxLightBlockDist);
            int endY = Math.Min(sizeY, blockY + maxLightBlockDist);
            int endZ = Math.Min(sizeZ, blockZ + maxLightBlockDist);
            LightInfo lightInfo = new LightInfo(blockX, blockY, blockZ, light);

            for (int lz = startZ; lz < endZ; lz++)
            {
                for (int lx = startX; lx < endX; lx++)
                {
                    for (int ly = startY; ly < endY; ly++)
                    {
                        int vx = lx * BlockInformation.VoxelSize + 5;
                        int vy = ly * BlockInformation.VoxelSize + 5;
                        int vz = lz * BlockInformation.VoxelSize + 5;
                        float blockPercentage = LightUtils.GetLightPercentage(lightInfo, vx, vy, vz);
                        Color3f color = gameWorld.GetBlockLight(lx, ly, lz);
                        gameWorld.SetBlockLight(lx, ly, lz, color + (light.Color * blockPercentage));

                        List<LightInfo> blockLights = gameWorld.GetLights(lx, ly, lz);
                        using (new PerformanceLock(blockLights))
                        {
                            if (blockLights.Count < maxLightsPerBlock)
                                blockLights.Add(lightInfo);
                            else
                            {
                                // Too many lights on this block. Remove the light that affects the block the least.
                                int leastLightIndex = 0;
                                float leastPercentage = LightUtils.GetLightPercentage(blockLights[0], vx, vy, vz);
                                for (int i = 1; i < blockLights.Count; i++)
                                {
                                    float lightPercentage = LightUtils.GetLightPercentage(blockLights[i], vx, vy, vz);
                                    if (lightPercentage < leastPercentage)
                                    {
                                        leastLightIndex = i;
                                        leastPercentage = lightPercentage;
                                    }
                                }

                                if (leastPercentage < blockPercentage)
                                {
                                    // Found an existing light that is less intense then the new light
                                    blockLights[leastLightIndex] = lightInfo;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
