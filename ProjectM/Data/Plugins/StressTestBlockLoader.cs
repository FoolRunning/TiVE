using System;
using System.Collections.Generic;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;
using ProdigalSoftware.TiVEPluginFramework.Generators;

namespace ProdigalSoftware.ProjectM.Data.Plugins
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
        
        public Block CreateBlock(string name)
        {
            return null;
        }
        
        public IEnumerable<Block> CreateBlocks()
        {
            const bool forFantasyLighting = true;

            const byte blockCenter = BlockLOD32.VoxelSize / 2;
            Vector3b blockCenterVector = new Vector3b(blockCenter, blockCenter, blockCenter);
            for (int i = 0; i < 64; i++)
            {
                yield return CreateBlockInfo("STlava" + i, new Color4f(200, 15, 8, 255), 1.0f, i,
                    new LightComponent(new Vector3b(blockCenter, blockCenter, blockCenter), new Color3f(0.3f, 0.02f, 0.01f), forFantasyLighting ? 4 : 15), true);
                yield return CreateBlockInfo("STston" + i, new Color4f(120, 120, 120, 255), 1.0f, i);
                yield return CreateBlockInfo("STsand" + i, new Color4f(120, 100, 20, 255), 0.1f, i, null, true);
            }

            yield return CreateBlockInfo("STback", true, 0, new Color4f(235, 235, 235, 255), 1.0f,
                new ParticleComponent("Snow", new Vector3i(0, 0, 0)), null, true);

            Block fireBlock = new Block("STfire");
            fireBlock.AddComponent(new ParticleComponent("Fire", new Vector3i(blockCenter, blockCenter, 1)));
            fireBlock.AddComponent(new LightComponent(new Vector3b(blockCenter, blockCenter, 4), new Color3f(1.0f, 0.8f, 0.6f), forFantasyLighting ? 7 : 24));
            fireBlock.AddComponent(new LightPassthroughComponent());
            yield return fireBlock;

            const int lightDist = forFantasyLighting ? 15 : 35;
            yield return CreateBlockInfo("STlight0", false, 2, new Color4f(255, 255, 255, 255), 1.0f, null,
                new LightComponent(blockCenterVector, new Color3f(1.0f, 1.0f, 1.0f), lightDist), true);

            yield return CreateBlockInfo("STlight1", false, 2, new Color4f(255, 255, 0, 255), 1.0f, null,
                new LightComponent(blockCenterVector, new Color3f(1.0f, 1.0f, 0.3f), lightDist), true);

            yield return CreateBlockInfo("STlight2", false, 2, new Color4f(0, 255, 0, 255), 1.0f, null,
                new LightComponent(blockCenterVector, new Color3f(0.3f, 1.0f, 0.3f), lightDist), true);

            yield return CreateBlockInfo("STlight3", false, 2, new Color4f(0, 255, 255, 255), 1.0f, null,
                new LightComponent(blockCenterVector, new Color3f(0.3f, 1.0f, 1.0f), lightDist), true);

            yield return CreateBlockInfo("STlight4", false, 2, new Color4f(0, 0, 255, 255), 1.0f, null,
                new LightComponent(blockCenterVector, new Color3f(0.3f, 0.3f, 1.0f), lightDist), true);

            yield return CreateBlockInfo("STlight5", false, 2, new Color4f(255, 0, 255, 255), 1.0f, null,
                new LightComponent(blockCenterVector, new Color3f(1.0f, 0.3f, 1.0f), lightDist), true);

            yield return CreateBlockInfo("STlight6", false, 2, new Color4f(255, 255, 255, 255), 1.0f, null,
                new LightComponent(blockCenterVector, new Color3f(1.0f, 1.0f, 1.0f), lightDist), true);

            yield return CreateBlockInfo("STfountain", false, BlockLOD32.VoxelSize / 2, new Color4f(20, 20, 150, 255), 1.0f,
                new ParticleComponent("Fountain", new Vector3i(blockCenter, blockCenter, 13)));
        }

        private static Block CreateBlockInfo(string name, Color4f color, float voxelDensity, int sides, 
            LightComponent light = null, bool allowLightPassthrough = false)
        {
            const float mid = BlockLOD32.VoxelSize / 2.0f - 0.5f;
            float sphereSize = BlockLOD32.VoxelSize / 2.0f;

            Block block = new Block(name);
            VoxelSettings settings = VoxelSettings.None;
            if (light == null)
                block.AddComponent(new VoxelNoiseComponent(0.2f));
            else
            {
                block.AddComponent(light);
                settings = VoxelSettings.AllowLightPassthrough | VoxelSettings.IgnoreLighting;
            }

            if (allowLightPassthrough)
                block.AddComponent(new LightPassthroughComponent());

            if (voxelDensity < 0.5f)
                settings |= VoxelSettings.SkipVoxelNormalCalc;

            for (int x = 0; x < BlockLOD32.VoxelSize; x++)
            {
                for (int y = 0; y < BlockLOD32.VoxelSize; y++)
                {
                    for (int z = 0; z < BlockLOD32.VoxelSize; z++)
                    {
                        if (((sides & Top) != 0 && (sides & Front) != 0 && y - (int)mid > BlockLOD32.VoxelSize - z) ||   // rounded YPlus-ZPlus
                            ((sides & Front) != 0 && (sides & Bottom) != 0 && y + (int)mid < z) ||                             // rounded ZPlus-YMinus
                            ((sides & Bottom) != 0 && (sides & Back) != 0 && y + (int)mid < BlockLOD32.VoxelSize - z) || // rounded YMinus-Back
                            ((sides & Back) != 0 && (sides & Top) != 0 && y - (int)mid > z))                                   // rounded Back-YPlus
                        {
                            // Cylinder around the x-axis
                            float dist = (y - mid) * (y - mid) + (z - mid) * (z - mid);
                            if (dist > sphereSize * sphereSize)
                                continue;
                        }

                        if (((sides & Right) != 0 && (sides & Front) != 0 && x - (int)mid > BlockLOD32.VoxelSize - z) || // rounded XPlus-ZPlus
                            ((sides & Front) != 0 && (sides & Left) != 0 && x + (int)mid < z) ||                               // rounded ZPlus-XMinus
                            ((sides & Left) != 0 && (sides & Back) != 0 && x + (int)mid < BlockLOD32.VoxelSize - z) ||   // rounded XMinus-Back
                            ((sides & Back) != 0 && (sides & Right) != 0 && x - (int)mid > z))                                 // rounded Back-XPlus
                        {
                            // Cylinder around the y-axis
                            float dist = (x - mid) * (x - mid) + (z - mid) * (z - mid);
                            if (dist > sphereSize * sphereSize)
                                continue;
                        }

                        if (((sides & Right) != 0 && (sides & Top) != 0 && x - (int)mid > BlockLOD32.VoxelSize - y) ||   // rounded XPlus-YPlus
                            ((sides & Top) != 0 && (sides & Left) != 0 && x + (int)mid < y) ||                                 // rounded YPlus-XMinus
                            ((sides & Left) != 0 && (sides & Bottom) != 0 && x + (int)mid < BlockLOD32.VoxelSize - y) || // rounded XMinus-YMinus
                            ((sides & Bottom) != 0 && (sides & Right) != 0 && x - (int)mid > y))                               // rounded YMinus-XPlus
                        {
                            // Cylinder around the z-axis
                            float dist = (x - mid) * (x - mid) + (y - mid) * (y - mid);
                            if (dist > sphereSize * sphereSize)
                                continue;
                        }

                        if ((((sides & Top) != 0 && (sides & Bottom) != 0 && (sides & Left) != 0 && x < mid) || // rounded XMinus
                            ((sides & Top) != 0 && (sides & Bottom) != 0 && (sides & Right) != 0 && x > mid) || // rounded XPlus
                            ((sides & Top) != 0 && (sides & Right) != 0 && (sides & Left) != 0 && y > mid) ||   // rounded YPlus
                            ((sides & Bottom) != 0 && (sides & Right) != 0 && (sides & Left) != 0 && y < mid))  // rounded YMinus
                            && (((sides & Front) != 0 && z > mid) || ((sides & Back) != 0 && z < mid)))         // on the front or back
                        {
                            // rounded front or back
                            float dist = (x - mid) * (x - mid) + (y - mid) * (y - mid) + (z - mid) * (z - mid);
                            if (dist > sphereSize * sphereSize)
                                continue;
                        }

                        if (random.NextDouble() < voxelDensity)
                            block.LOD32[x, y, z] = new Voxel(color.R, color.G, color.B, color.A, settings);
                    }
                }
            }

            return block;
        }

        private static Block CreateBlockInfo(string name, bool frontOnly, float sphereSize, Color4f color, float voxelDensity,
            ParticleComponent particleSystem = null, LightComponent light = null, bool allowLightPassthrough = false)
        {
            const int mid = BlockLOD32.VoxelSize / 2;

            Block block = new Block(name);
            if (particleSystem != null)
                block.AddComponent(particleSystem);

            VoxelSettings settings = VoxelSettings.None;
            if (light == null)
                block.AddComponent(new VoxelNoiseComponent(0.2f));
            else
            {
                block.AddComponent(light);
                settings = VoxelSettings.AllowLightPassthrough | VoxelSettings.IgnoreLighting;
            }

            if (allowLightPassthrough)
                block.AddComponent(new LightPassthroughComponent());

            for (int x = 0; x < BlockLOD32.VoxelSize; x++)
            {
                for (int y = 0; y < BlockLOD32.VoxelSize; y++)
                {
                    for (int z = frontOnly ? BlockLOD32.VoxelSize - 2 : 0; z < BlockLOD32.VoxelSize; z++)
                    {
                        if (sphereSize > 0)
                        {
                            int dist = (x - mid) * (x - mid) + (y - mid) * (y - mid) + (z - mid) * (z - mid);
                            if (dist > sphereSize * sphereSize)
                                continue;
                        }

                        if (random.NextDouble() < voxelDensity)
                            block.LOD32[x, y, z] = new Voxel(color.R, color.G, color.B, color.A, settings);
                    }
                }
            }

            return block;
        }
    }
}
