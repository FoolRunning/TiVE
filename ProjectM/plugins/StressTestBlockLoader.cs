using System;
using System.Collections.Generic;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;
using ProdigalSoftware.TiVEPluginFramework.Generators;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.ProjectM.Plugins
{
    public class StressTestBlockLoader : IBlockGenerator
    {
        private static readonly Random random = new Random();

        private const int Front = 1;
        private const int Back = 2;
        private const int Left = 4;
        private const int Right = 8;
        private const int Top = 16;
        private const int Bottom = 32;

        public IEnumerable<Block> CreateBlocks(string blockListName)
        {
            if (blockListName != "stress")
                yield break;

            const bool forFantasyLighting = true;

            byte blockCenter = Block.VoxelSize / 2;
            Vector3b blockCenterVector = new Vector3b(blockCenter, blockCenter, blockCenter);
            for (int i = 0; i < 64; i++)
            {
                yield return CreateBlockInfo("lava" + i, new Color4f(200, 15, 8, 255), 1.0f, i,
                    new LightComponent(new Vector3b(blockCenter, blockCenter, blockCenter), new Color3f(0.2f, 0.01f, 0.001f), forFantasyLighting ? 5 : 8, true), true);
                yield return CreateBlockInfo("ston" + i, new Color4f(120, 120, 120, 255), 1.0f, i);
                yield return CreateBlockInfo("sand" + i, new Color4f(120, 100, 20, 255), 0.1f, i, null, true);
            }

            for (int i = 0; i < 5; i++)
            {
                yield return CreateBlockInfo("back" + i, true, 0, new Color4f(235, 235, 235, 255), 1.0f,
                    new ParticleComponent("Snow", new Vector3i(0, 0, 0)), null, true);
            }

            Block fireBlock = Factory.CreateBlock("fire");
            fireBlock.AddComponent(new ParticleComponent("Fire", new Vector3i(blockCenter, blockCenter, 1)));
            fireBlock.AddComponent(new LightComponent(new Vector3b(blockCenter, blockCenter, 4), new Color3f(1.0f, 0.8f, 0.6f), forFantasyLighting ? 5 : 10, true));
            yield return fireBlock;

            yield return CreateBlockInfo("light0", false, 2, new Color4f(255, 255, 255, 255), 1.0f, null,
                new LightComponent(blockCenterVector, new Color3f(1.0f, 1.0f, 1.0f), forFantasyLighting ? 10 : 20, true));

            yield return CreateBlockInfo("light1", false, 2, new Color4f(255, 255, 0, 255), 1.0f, null,
                new LightComponent(blockCenterVector, new Color3f(1.0f, 1.0f, 0.0f), forFantasyLighting ? 10 : 20, true));

            yield return CreateBlockInfo("light2", false, 2, new Color4f(0, 255, 0, 255), 1.0f, null,
                new LightComponent(blockCenterVector, new Color3f(0.0f, 1.0f, 0.0f), forFantasyLighting ? 10 : 20, true));

            yield return CreateBlockInfo("light3", false, 2, new Color4f(0, 255, 255, 255), 1.0f, null,
                new LightComponent(blockCenterVector, new Color3f(0.0f, 1.0f, 1.0f), forFantasyLighting ? 10 : 20, true));

            yield return CreateBlockInfo("light4", false, 2, new Color4f(0, 0, 255, 255), 1.0f, null,
                new LightComponent(blockCenterVector, new Color3f(0.0f, 0.0f, 1.0f), forFantasyLighting ? 10 : 20, true));

            yield return CreateBlockInfo("light5", false, 2, new Color4f(255, 0, 255, 255), 1.0f, null,
                new LightComponent(blockCenterVector, new Color3f(1.0f, 0.0f, 1.0f), forFantasyLighting ? 10 : 20, true));

            yield return CreateBlockInfo("light6", false, 2, new Color4f(255, 255, 255, 255), 1.0f, null,
                new LightComponent(blockCenterVector, new Color3f(1.0f, 1.0f, 1.0f), forFantasyLighting ? 10 : 20, true));

            yield return CreateBlockInfo("fountain", false, Block.VoxelSize / 2, new Color4f(20, 20, 150, 255), 1.0f,
                new ParticleComponent("Fountain", new Vector3i(blockCenter, blockCenter, 13)));
        }

        private static Block CreateBlockInfo(string name, Color4f color, float voxelDensity, int sides, 
            LightComponent light = null, bool allowLightPassthrough = false)
        {
            const float mid = Block.VoxelSize / 2.0f - 0.5f;
            float sphereSize = Block.VoxelSize / 2.0f;

            Block block = Factory.CreateBlock(name);
            if (light != null)
            {
                block.AddComponent(light);
                block.AddComponent(UnlitComponent.Instance);
            }
            else
                block.AddComponent(ReflectiveLightComponent.Instance);

            if (allowLightPassthrough || light != null)
                block.AddComponent(TransparentComponent.Instance);

            for (int x = 0; x < Block.VoxelSize; x++)
            {
                for (int y = 0; y < Block.VoxelSize; y++)
                {
                    for (int z = 0; z < Block.VoxelSize; z++)
                    {
                        if (((sides & Top) != 0 && (sides & Front) != 0 && y - (int)mid > Block.VoxelSize - z) ||   // rounded Top-Front
                            ((sides & Front) != 0 && (sides & Bottom) != 0 && y + (int)mid < z) ||                             // rounded Front-Bottom
                            ((sides & Bottom) != 0 && (sides & Back) != 0 && y + (int)mid < Block.VoxelSize - z) || // rounded Bottom-Back
                            ((sides & Back) != 0 && (sides & Top) != 0 && y - (int)mid > z))                                   // rounded Back-Top
                        {
                            // Cylinder around the x-axis
                            float dist = (y - mid) * (y - mid) + (z - mid) * (z - mid);
                            if (dist > sphereSize * sphereSize)
                                continue;
                        }

                        if (((sides & Right) != 0 && (sides & Front) != 0 && x - (int)mid > Block.VoxelSize - z) || // rounded Right-Front
                            ((sides & Front) != 0 && (sides & Left) != 0 && x + (int)mid < z) ||                               // rounded Front-Left
                            ((sides & Left) != 0 && (sides & Back) != 0 && x + (int)mid < Block.VoxelSize - z) ||   // rounded Left-Back
                            ((sides & Back) != 0 && (sides & Right) != 0 && x - (int)mid > z))                                 // rounded Back-Right
                        {
                            // Cylinder around the y-axis
                            float dist = (x - mid) * (x - mid) + (z - mid) * (z - mid);
                            if (dist > sphereSize * sphereSize)
                                continue;
                        }

                        if (((sides & Right) != 0 && (sides & Top) != 0 && x - (int)mid > Block.VoxelSize - y) ||   // rounded Right-Top
                            ((sides & Top) != 0 && (sides & Left) != 0 && x + (int)mid < y) ||                                 // rounded Top-Left
                            ((sides & Left) != 0 && (sides & Bottom) != 0 && x + (int)mid < Block.VoxelSize - y) || // rounded Left-Bottom
                            ((sides & Bottom) != 0 && (sides & Right) != 0 && x - (int)mid > y))                               // rounded Bottom-Right
                        {
                            // Cylinder around the z-axis
                            float dist = (x - mid) * (x - mid) + (y - mid) * (y - mid);
                            if (dist > sphereSize * sphereSize)
                                continue;
                        }

                        if ((((sides & Top) != 0 && (sides & Bottom) != 0 && (sides & Left) != 0 && x < mid) || // rounded Left
                            ((sides & Top) != 0 && (sides & Bottom) != 0 && (sides & Right) != 0 && x > mid) || // rounded Right
                            ((sides & Top) != 0 && (sides & Right) != 0 && (sides & Left) != 0 && y > mid) ||   // rounded Top
                            ((sides & Bottom) != 0 && (sides & Right) != 0 && (sides & Left) != 0 && y < mid))  // rounded Bottom
                            && (((sides & Front) != 0 && z > mid) || ((sides & Back) != 0 && z < mid)))         // on the front or back
                        {
                            // rounded front or back
                            float dist = (x - mid) * (x - mid) + (y - mid) * (y - mid) + (z - mid) * (z - mid);
                            if (dist > sphereSize * sphereSize)
                                continue;
                        }

                        if (random.NextDouble() < voxelDensity)
                            block[x, y, z] = CreateVoxelFromColor(color);
                    }
                }
            }

            return block;
        }

        private static Block CreateBlockInfo(string name, bool frontOnly, float sphereSize, Color4f color, float voxelDensity,
            ParticleComponent particleSystem = null, LightComponent light = null, bool allowLightPassthrough = false)
        {
            const int mid = Block.VoxelSize / 2;

            Block block = Factory.CreateBlock(name);
            if (particleSystem != null)
                block.AddComponent(particleSystem);
            if (light != null)
            {
                block.AddComponent(light);
                block.AddComponent(UnlitComponent.Instance);
            }
            if (allowLightPassthrough || light != null)
                block.AddComponent(TransparentComponent.Instance);

            for (int x = 0; x < Block.VoxelSize; x++)
            {
                for (int y = 0; y < Block.VoxelSize; y++)
                {
                    for (int z = frontOnly ? Block.VoxelSize - 2 : 0; z < Block.VoxelSize; z++)
                    {
                        if (sphereSize > 0)
                        {
                            int dist = (x - mid) * (x - mid) + (y - mid) * (y - mid) + (z - mid) * (z - mid);
                            if (dist > sphereSize * sphereSize)
                                continue;
                        }

                        if (random.NextDouble() < voxelDensity)
                            block[x, y, z] = (light == null) ? CreateVoxelFromColor(color) : (Voxel)(Color4b)color;
                    }
                }
            }

            return block;
        }

        private static Voxel CreateVoxelFromColor(Color4f seed)
        {
            float scale = (float)(random.NextDouble() * 0.1 + 0.95);
            return (Voxel)new Color4b(Math.Min(seed.R * scale, 1.0f), Math.Min(seed.G * scale, 1.0f), 
                Math.Min(seed.B * scale, 1.0f), seed.A);
        }
    }
}
