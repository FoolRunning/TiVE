using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework.Lighting;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Resources
{
    internal sealed class LightManager
    {
        private const int MaxLightsPerBlock = 3;

        private readonly ILight ambientLight;
        private GameWorld gameWorld;

        public LightManager()
        {
            ambientLight = new AmbientLight(new Color4f(0.05f, 0.05f, 0.05f, 1.0f));
        }

        public void CalcualteLightsForGameWorld(GameWorld newGameWorld)
        {
            Messages.Print("Calculating static lighting...");
            
            gameWorld = newGameWorld;

            int sizeX = newGameWorld.BlockSize.X;
            int sizeY = newGameWorld.BlockSize.Y;
            int sizeZ = newGameWorld.BlockSize.Z;

            int lastPercentComplete = 0;
            int totalComplete = 0;
            Parallel.For(0, sizeX, x =>
                //for (int x = 0; x < sizeX; x++)
            {
                totalComplete++;
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
                        BlockInformation block = newGameWorld[x, y, z];
                        ILight light = block.Light;
                        if (light == null)
                            continue;

                        LightInfo lightInfo = new LightInfo(x, y, z, light);
                        int maxLightBlockDist = (int)Math.Ceiling(light.MaxVoxelDist / BlockInformation.BlockSize) + 1;
                        for (int lz = z - maxLightBlockDist; lz < z + maxLightBlockDist; lz++)
                        {
                            for (int lx = x - maxLightBlockDist; lx < x + maxLightBlockDist; lx++)
                            {
                                for (int ly = y - maxLightBlockDist; ly < y + maxLightBlockDist; ly++)
                                {
                                    if (lx >= 0 && lx < sizeX && ly >= 0 && ly < sizeY && lz >= 0 && lz < sizeZ)
                                    {
                                        List<LightInfo> blockLights = newGameWorld.GetLights(lx, ly, lz);
                                        using (new PerformanceLock(blockLights))
                                        {
                                            if (blockLights.Count < MaxLightsPerBlock)
                                                blockLights.Add(lightInfo);
                                            else
                                            {
                                                // Too many lights on this block. Remove the light that affects the block the least.
                                                int vx = lx * BlockInformation.BlockSize + 5;
                                                int vy = ly * BlockInformation.BlockSize + 5;
                                                int vz = lz * BlockInformation.BlockSize + 5;
                                                LightInfo leastLight = null;
                                                int leastLightIndex = -1;
                                                float leastMaxComponent = 0.0f;
                                                for (int i = 0; i < blockLights.Count; i++)
                                                {
                                                    float lightMaxComponent = blockLights[i].GetLightAtVoxel(vx, vy, vz).MaxComponent;
                                                    if (leastLight == null || lightMaxComponent < leastLight.GetLightAtVoxel(vx, vy, vz).MaxComponent)
                                                    {
                                                        leastLight = blockLights[i];
                                                        leastLightIndex = i;
                                                        leastMaxComponent = lightMaxComponent;
                                                    }
                                                }

                                                if (leastMaxComponent < lightInfo.GetLightAtVoxel(vx, vy, vz).MaxComponent)
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
            });

            Messages.AddDoneText();
        }

        public void GetLightAt(int voxelX, int voxelY, int voxelZ, out float percentR, out float percentG, out float percentB)
        {
            percentR = percentG = percentB = 0.0f;

            if (ambientLight != null)
            {
                Color4f color = ambientLight.GetColorAtDistSquared(0.0f);
                percentR += color.R;
                percentG += color.G;
                percentB += color.B;
            }

            Vector3i worldSize = gameWorld.VoxelSize;
            if (voxelX < 0 || voxelX >= worldSize.X || voxelY < 0 || voxelY >= worldSize.Y || voxelZ < 0 || voxelZ >= worldSize.Z)
                return; // voxel is outside the game world (probably a particle) so just return the ambient light

            int blockX = voxelX / BlockInformation.BlockSize;
            int blockY = voxelY / BlockInformation.BlockSize;
            int blockZ = voxelZ / BlockInformation.BlockSize;

            List<LightInfo> blockLights = gameWorld.GetLights(blockX, blockY, blockZ);
            for (int i = 0; i < blockLights.Count; i++)
            {
                Color4f color = blockLights[i].GetLightAtVoxel(voxelX, voxelY, voxelZ);
                percentR += color.R;
                percentG += color.G;
                percentB += color.B;
            }
        }
    }
}
