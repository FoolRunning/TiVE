﻿using System;
using System.Collections.Generic;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Lighting;
using ProdigalSoftware.TiVEPluginFramework.Particles;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.ProjectM.Plugins
{
    public class MazeBlockLoader : IBlockGenerator
    {
        private static readonly Random random = new Random();

        private const int Front = 1;
        private const int Back = 2;
        private const int Left = 4;
        private const int Right = 8;
        private const int Top = 16;
        private const int Bottom = 32;

        public IEnumerable<BlockInformation> CreateBlocks(string blockListName)
        {
            if (blockListName != "maze")
                yield break;

            const byte bc = BlockInformation.VoxelSize / 2;
            const int mv = BlockInformation.VoxelSize - 1;
            const uint mortarColor = 0xFFAFAFAF;

            Vector3b blockCenterVector = new Vector3b(bc, bc, bc);
            uint[, ,] particleVoxels = new uint[1, 1, 1];
            particleVoxels[0, 0, 0] = 0xFFFFFFFF;
            for (int i = 0; i < 64; i++)
            {
                yield return CreateBlockInfo("lava" + i, new Color4f(200, 15, 8, 255), 1.0f, i,
                    new PointLight(new Vector3b(bc, bc, bc), new Color3f(0.2f, 0.01f, 0.005f), 4));
                
                BlockInformation stone = CreateBlockInfo("ston" + i, new Color4f(220, 220, 220, 255), 1.0f, i);
                for (int x = 0; x <= mv; x++)
                {
                    for (int y = 0; y <= mv; y++)
                    {
                        if ((i & Bottom) != 0)
                        {
                            stone[x, 0, 0] = 0;
                            stone[x, 0, 1] = 0;
                            stone[x, 0, mv] = 0;

                            stone[x, 0, bc] = 0;
                            stone[x, 0, bc - 1] = 0;
                            stone[x, 0, bc + 1] = 0;
                        }

                        if ((i & Top) != 0)
                        {
                            stone[x, mv, 0] = 0;
                            stone[x, mv, 1] = 0;
                            stone[x, mv, mv] = 0;

                            stone[x, mv, bc] = 0;
                            stone[x, mv, bc - 1] = 0;
                            stone[x, mv, bc + 1] = 0;
                        }

                        if ((i & Left) != 0)
                        {
                            stone[0, y, 0] = 0;
                            stone[0, y, 1] = 0;
                            stone[0, y, mv] = 0;

                            stone[0, y, bc] = 0;
                            stone[0, y, bc - 1] = 0;
                            stone[0, y, bc + 1] = 0;
                        }

                        if ((i & Right) != 0)
                        {
                            stone[mv, y, 0] = 0;
                            stone[mv, y, 1] = 0;
                            stone[mv, y, mv] = 0;

                            stone[mv, y, bc] = 0;
                            stone[mv, y, bc - 1] = 0;
                            stone[mv, y, bc + 1] = 0;
                        }

                        ReplaceVoxel(stone, x, y, 0, mortarColor);
                        ReplaceVoxel(stone, x, y, bc, mortarColor);
                    }
                }
                for (int n = 0; n <= mv; n++)
                {
                    for (int z = 0; z < bc; z++)
                    {
                        if ((i & Bottom) != 0)
                        {
                            stone[bc - 5, 0, z] = 0;
                            stone[bc - 4, 0, z] = 0;
                            stone[bc - 3, 0, z] = 0;

                            stone[bc + 3, 0, z + bc] = 0;
                            stone[bc + 4, 0, z + bc] = 0;
                            stone[bc + 5, 0, z + bc] = 0;
                        }

                        if ((i & Top) != 0)
                        {
                            stone[bc - 5, mv, z] = 0;
                            stone[bc - 4, mv, z] = 0;
                            stone[bc - 3, mv, z] = 0;

                            stone[bc + 3, mv, z + bc] = 0;
                            stone[bc + 4, mv, z + bc] = 0;
                            stone[bc + 5, mv, z + bc] = 0;
                        }

                        if ((i & Left) != 0)
                        {
                            stone[0, bc - 5, z] = 0;
                            stone[0, bc - 4, z] = 0;
                            stone[0, bc - 3, z] = 0;

                            stone[0, bc + 3, z + bc] = 0;
                            stone[0, bc + 4, z + bc] = 0;
                            stone[0, bc + 5, z + bc] = 0;
                        }

                        if ((i & Right) != 0)
                        {
                            stone[mv, bc - 5, z] = 0;
                            stone[mv, bc - 4, z] = 0;
                            stone[mv, bc - 3, z] = 0;

                            stone[mv, bc + 3, z + bc] = 0;
                            stone[mv, bc + 4, z + bc] = 0;
                            stone[mv, bc + 5, z + bc] = 0;
                        }

                        ReplaceVoxel(stone, bc - 4, n, z, mortarColor);
                        if (z < bc - 1 || (i & Front) == 0)
                            ReplaceVoxel(stone, bc + 4, n, z + bc, mortarColor);

                        ReplaceVoxel(stone, n, bc - 4, z, mortarColor);
                        if (z < bc - 1 || (i & Front) == 0)
                            ReplaceVoxel(stone, n, bc + 4, z + bc, mortarColor);
                    }
                }
                yield return stone;
            }

            for (int i = 0; i < 50; i++)
            {
                BlockInformation grass = new BlockInformation("grass" + i, null, null, null, true, true);
                Color4f grassColor = new Color4f(50, 230, 50, 255);
                for (int z = 0; z < BlockInformation.VoxelSize; z++)
                {
                    for (int x = 0; x < BlockInformation.VoxelSize; x++)
                    {
                        for (int y = 0; y < BlockInformation.VoxelSize; y++)
                        {
                            if (random.NextDouble() < 0.5 - z / 50.0 && (z == 0 || GrassVoxelUnder(grass, x, y, z)))
                            {
                                grass[x, y, z] = CreateColorFromColor(grassColor).ToArgb();
                                if (z == BlockInformation.VoxelSize - 1 && random.NextDouble() < 0.2 &&
                                    x > 0 && x < BlockInformation.VoxelSize - 1 && y > 0 && y < BlockInformation.VoxelSize - 1)
                                {
                                    // Make a flower
                                    Color4f flowerColor = CreateRandomFlowerColor(random);
                                    grass[x, y, z] = CreateColorFromColor(flowerColor).ToArgb();
                                    grass[x - 1, y, z - 1] = CreateColorFromColor(flowerColor).ToArgb();
                                    grass[x + 1, y, z - 1] = CreateColorFromColor(flowerColor).ToArgb();
                                    grass[x, y - 1, z - 1] = CreateColorFromColor(flowerColor).ToArgb();
                                    grass[x, y + 1, z - 1] = CreateColorFromColor(flowerColor).ToArgb();
                                }
                            }
                        }
                    }
                }

                yield return grass;
            }

            for (int i = 0; i < 6; i++)
            {
                yield return CreateBlockInfo("backStone" + i, 0, new Color4f(240, 240, 240, 255), 1.0f, allowLightPassthrough: true);
                yield return CreateBlockInfo("back" + i, 0, new Color4f(0.6f, 0.45f, 0.25f, 1.0f), 1.0f, allowLightPassthrough: true);
            }

            BlockInformation fireBlock = new BlockInformation("fire", 
                new ParticleSystemInformation(particleVoxels, new FireUpdater(), new Vector3b(bc, bc, 1), 300, 400, TransparencyType.Additive, false),
                new PointLight(new Vector3b(bc, bc, 4), new Color3f(1.0f, 0.8f, 0.6f), 15));
            yield return fireBlock;

            yield return CreateBlockInfo("roomLight", 5, new Color4f(1.0f, 1.0f, 1.0f, 1.0f), 1.0f, null,
                new PointLight(blockCenterVector, new Color3f(1.0f, 1.0f, 1.0f), 30));

            const int lightDist = 20;
            ParticleSystemInformation bugInformation = new ParticleSystemInformation(particleVoxels,
                new LightBugsUpdater(), new Vector3b(bc, bc, bc), 15, 10, TransparencyType.Realistic, true);

            yield return CreateBlockInfo("light0", 2, new Color4f(0.5f, 0.8f, 1.0f, 1.0f), 1.0f, bugInformation,
                new PointLight(blockCenterVector, new Color3f(0.5f, 0.8f, 1.0f), lightDist));

            yield return CreateBlockInfo("light1", 2, new Color4f(0.8f, 0.5f, 1.0f, 1.0f), 1.0f, bugInformation,
                new PointLight(blockCenterVector, new Color3f(0.8f, 0.5f, 1.0f), lightDist));

            yield return CreateBlockInfo("light2", 2, new Color4f(1.0f, 0.5f, 0.8f, 1.0f), 1.0f, bugInformation,
                new PointLight(blockCenterVector, new Color3f(1.0f, 0.5f, 0.8f), lightDist));

            yield return CreateBlockInfo("light3", 2, new Color4f(1.0f, 0.8f, 0.5f, 1.0f), 1.0f, bugInformation,
                new PointLight(blockCenterVector, new Color3f(1.0f, 0.8f, 0.5f), lightDist));

            yield return CreateBlockInfo("light4", 2, new Color4f(0.5f, 1.0f, 0.8f, 1.0f), 1.0f, bugInformation,
                new PointLight(blockCenterVector, new Color3f(0.5f, 1.0f, 0.8f), lightDist));

            yield return CreateBlockInfo("light5", 2, new Color4f(0.8f, 1.0f, 0.5f, 1.0f), 1.0f, bugInformation,
                new PointLight(blockCenterVector, new Color3f(0.8f, 1.0f, 0.5f), lightDist));

            particleVoxels = new uint[3, 3, 3];
            particleVoxels[1, 1, 1] = 0xFFFFFFFF;
            particleVoxels[0, 1, 1] = 0xFFFFFFFF;
            particleVoxels[2, 1, 1] = 0xFFFFFFFF;
            particleVoxels[1, 0, 1] = 0xFFFFFFFF;
            particleVoxels[1, 2, 1] = 0xFFFFFFFF;
            particleVoxels[1, 1, 0] = 0xFFFFFFFF;
            particleVoxels[1, 1, 2] = 0xFFFFFFFF;
            yield return CreateBlockInfo("fountain", bc, new Color4f(20, 20, 150, 255), 1.0f,
                new ParticleSystemInformation(particleVoxels, new FountainUpdater(), new Vector3b(bc, bc, 13), 100, 300, TransparencyType.Realistic, true));
        }

        public IEnumerable<BlockAnimationDefinition> CreateAnimations(string blockListName)
        {
            yield break;
        }

        private static void ReplaceVoxel(BlockInformation block, int x, int y, int z, uint newVoxel)
        {
            if (block[x, y, z] != 0)
                block[x, y, z] = newVoxel;
        }

        private static Color4f CreateRandomFlowerColor(Random random)
        {
            switch (random.Next(6))
            {
                case 1: return new Color4f(1.0f, 0.0f, 0.0f, 1.0f);
                case 2: return new Color4f(0.0f, 0.0f, 1.0f, 1.0f);
                case 3: return new Color4f(1.0f, 0.0f, 1.0f, 1.0f);
                case 4: return new Color4f(1.0f, 1.0f, 0.0f, 1.0f);
                case 5: return new Color4f(0.0f, 1.0f, 1.0f, 1.0f);
                default: return new Color4f(1.0f, 1.0f, 1.0f, 1.0f);
            }
        }

        private static bool GrassVoxelUnder(BlockInformation block, int x, int y, int z)
        {
            int countUnder = 0;
            if (block[x, y, z - 1] != 0)
                countUnder++;
            if (x > 0 && block[x - 1, y, z - 1] != 0)
                countUnder++;
            if (x < BlockInformation.VoxelSize - 1 && block[x + 1, y, z - 1] != 0)
                countUnder++;
            if (y > 0 && block[x, y - 1, z - 1] != 0)
                countUnder++;
            if (y < BlockInformation.VoxelSize - 1 && block[x, y + 1, z - 1] != 0)
                countUnder++;
            return countUnder > 0 && countUnder <= 1;
        }

        private static BlockInformation CreateBlockInfo(string name, Color4f color, float voxelDensity, int sides, ILight light = null)
        {
            const float mid = BlockInformation.VoxelSize / 2.0f - 0.5f;
            float sphereSize = BlockInformation.VoxelSize / 2.0f;

            BlockInformation block = new BlockInformation(name, null, light, null, light == null);
            for (int x = 0; x < BlockInformation.VoxelSize; x++)
            {
                for (int y = 0; y < BlockInformation.VoxelSize; y++)
                {
                    for (int z = 0; z < BlockInformation.VoxelSize; z++)
                    {
                        if (((sides & Top) != 0 && (sides & Front) != 0 && y - (int)mid > BlockInformation.VoxelSize - z) ||   // rounded Top-Front
                            ((sides & Front) != 0 && (sides & Bottom) != 0 && y + (int)mid < z) ||                             // rounded Front-Bottom
                            ((sides & Bottom) != 0 && (sides & Back) != 0 && y + (int)mid < BlockInformation.VoxelSize - z) || // rounded Bottom-Back
                            ((sides & Back) != 0 && (sides & Top) != 0 && y - (int)mid > z))                                   // rounded Back-Top
                        {
                            // Cylinder around the x-axis
                            float dist = (y - mid) * (y - mid) + (z - mid) * (z - mid);
                            if (dist > sphereSize * sphereSize)
                                continue;
                        }

                        if (((sides & Right) != 0 && (sides & Front) != 0 && x - (int)mid > BlockInformation.VoxelSize - z) || // rounded Right-Front
                            ((sides & Front) != 0 && (sides & Left) != 0 && x + (int)mid < z) ||                               // rounded Front-Left
                            ((sides & Left) != 0 && (sides & Back) != 0 && x + (int)mid < BlockInformation.VoxelSize - z) ||   // rounded Left-Back
                            ((sides & Back) != 0 && (sides & Right) != 0 && x - (int)mid > z))                                 // rounded Back-Right
                        {
                            // Cylinder around the y-axis
                            float dist = (x - mid) * (x - mid) + (z - mid) * (z - mid);
                            if (dist > sphereSize * sphereSize)
                                continue;
                        }

                        if (((sides & Right) != 0 && (sides & Top) != 0 && x - (int)mid > BlockInformation.VoxelSize - y) ||   // rounded Right-Top
                            ((sides & Top) != 0 && (sides & Left) != 0 && x + (int)mid < y) ||                                 // rounded Top-Left
                            ((sides & Left) != 0 && (sides & Bottom) != 0 && x + (int)mid < BlockInformation.VoxelSize - y) || // rounded Left-Bottom
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
                            block[x, y, z] = CreateColorFromColor(color).ToArgb();
                    }
                }
            }

            return block;
        }

        private static BlockInformation CreateBlockInfo(string name, float sphereSize, Color4f color, float voxelDensity,
            ParticleSystemInformation particleSystem = null, ILight light = null, int zStart = 0, int zLimit = BlockInformation.VoxelSize,
            bool allowLightPassthrough = false)
        {
            const int mid = BlockInformation.VoxelSize / 2;

            BlockInformation block = new BlockInformation(name, particleSystem, light, null, light == null, allowLightPassthrough || light != null);
            for (int x = 0; x < BlockInformation.VoxelSize; x++)
            {
                for (int y = 0; y < BlockInformation.VoxelSize; y++)
                {
                    for (int z = zStart; z < zLimit; z++)
                    {
                        if (sphereSize > 0)
                        {
                            int dist = (x - mid) * (x - mid) + (y - mid) * (y - mid) + (z - mid) * (z - mid);
                            if (dist > sphereSize * sphereSize)
                                continue;
                        }

                        if (random.NextDouble() < voxelDensity)
                            block[x, y, z] = (light == null) ? CreateColorFromColor(color).ToArgb() : ((Color4b)color).ToArgb();
                    }
                }
            }

            return block;
        }

        private static Color4b CreateColorFromColor(Color4f seed)
        {
            float scale = (float)(random.NextDouble() * 0.2 + 0.9);
            return new Color4b(Math.Min(seed.R * scale, 1.0f), Math.Min(seed.G * scale, 1.0f), 
                Math.Min(seed.B * scale, 1.0f), seed.A);
        }

        #region SnowUpdater class
        private class SnowUpdater : ParticleController
        {
            private const float SnowDeacceleration = 21.0f;
            private const float AliveTime = 30.0f;

            private static readonly Random random = new Random();

            #region Implementation of ParticleController
            public override bool BeginUpdate(IParticleSystem particleSystem, float timeSinceLastFrame)
            {
                return true;
            }

            public override void Update(Particle particle, float timeSinceLastFrame, float systemX, float systemY, float systemZ)
            {
                ApplyVelocity(particle, timeSinceLastFrame);

                if (particle.X > systemX + BlockInformation.VoxelSize)
                    particle.VelX -= SnowDeacceleration * timeSinceLastFrame;
                if (particle.X < systemX)
                    particle.VelX += SnowDeacceleration * timeSinceLastFrame;
                if (particle.Y > systemY + BlockInformation.VoxelSize)
                    particle.VelY -= SnowDeacceleration * timeSinceLastFrame;
                if (particle.Y < systemY)
                    particle.VelY += SnowDeacceleration * timeSinceLastFrame;

                if (particle.Z < 0)
                    InitNewInternal(particle, systemX, systemY, true);

                //particle.Time -= timeSinceLastFrame;
            }

            public override void InitializeNew(Particle particle, float startX, float startY, float startZ)
            {
                InitNewInternal(particle, startX, startY, false);
            }
            #endregion

            private static void InitNewInternal(Particle particle, float startX, float startY, bool startAtTop)
            {
                particle.VelX = (float)random.NextDouble() * 48.0f - 24.0f;
                particle.VelZ = (float)random.NextDouble() * -30.0f - 20.0f;
                particle.VelY = (float)random.NextDouble() * 48.0f - 24.0f;

                particle.X = startX + random.Next(BlockInformation.VoxelSize);
                particle.Y = startY + random.Next(BlockInformation.VoxelSize);
                if (startAtTop)
                    particle.Z = 59 * BlockInformation.VoxelSize;
                else
                    particle.Z = random.Next(56 * BlockInformation.VoxelSize) + 3 * BlockInformation.VoxelSize;

                particle.Color = new Color4b(255, 255, 255, 255);
                particle.Time = (float)random.NextDouble() * AliveTime / 2.0f + AliveTime / 2.0f;
            }
        }
        #endregion

        #region FireUpdater class
        private class FireUpdater : ParticleController
        {
            private const float FlameDeacceleration = 35.0f;
            private const float AliveTime = 1.0f;

            private static readonly Random random = new Random();
            private static readonly Color4b[] colorList = new Color4b[256];

            static FireUpdater()
            {
                for (int i = 0; i < 256; i++)
                {
                    if (i < 150)
                        colorList[i] = new Color4b(255, (byte)(255 - (i * 1.7f)), (byte)(50 - i / 3), 250);
                    if (i >= 150)
                        colorList[i] = new Color4b((byte)(255 - (i - 150) * 2.4f), 0, 0, 250);
                }
            }

            #region Implementation of ParticleController
            public override bool BeginUpdate(IParticleSystem particleSystem, float timeSinceLastFrame)
            {
                return true;
            }

            public override void Update(Particle particle, float timeSinceLastFrame, float systemX, float systemY, float systemZ)
            {
                ApplyVelocity(particle, timeSinceLastFrame);

                if (particle.X > systemX)
                    particle.VelX -= FlameDeacceleration * timeSinceLastFrame;
                if (particle.X < systemX)
                    particle.VelX += FlameDeacceleration * timeSinceLastFrame;
                if (particle.Y > systemY)
                    particle.VelY -= FlameDeacceleration * timeSinceLastFrame;
                if (particle.Y < systemY)
                    particle.VelY += FlameDeacceleration * timeSinceLastFrame;
                //if (particle.Z > systemZ)
                //    particle.VelZ -= FlameDeacceleration * timeSinceLastFrame;
                //if (particle.Z < systemZ)
                //    particle.VelZ += FlameDeacceleration * timeSinceLastFrame;
                
                //float totalTime = (float)Math.Pow(particleAliveTime, 5);
                //part.size = 1.0f - (float)Math.pow(part.aliveTime, 5) / totalTime;

                particle.Time -= timeSinceLastFrame;

                // set color
                if (particle.Time > 0.0f)
                {
                    int colorIndex = (int)(((AliveTime - particle.Time) / AliveTime) * (colorList.Length - 1));
                    particle.Color = colorList[Math.Min(colorIndex, colorList.Length - 1)];
                }
            }

            public override void InitializeNew(Particle particle, float startX, float startY, float startZ)
            {
                float angle = (float)random.NextDouble() * 2.0f * 3.141592f;
                float totalVel = (float)random.NextDouble() * 6.0f + 10.0f;
                particle.VelX = (float)Math.Cos(angle) * totalVel;
                particle.VelZ = (float)random.NextDouble() * 11.0f + 8.0f;
                particle.VelY = (float)Math.Sin(angle) * totalVel;

                particle.X = startX;
                particle.Y = startY;
                particle.Z = startZ;
                
                particle.Color = colorList[0];
                particle.Time = AliveTime;
            }
            #endregion
        }
        #endregion

        #region FountainUpdater class
        private class FountainUpdater : ParticleController
        {
            private const float AliveTime = 2.0f;
            private static readonly Random random = new Random();
            private static readonly Color4b[] colorList = new Color4b[256];

            static FountainUpdater()
            {
                for (int i = 0; i < 256; i++)
                    colorList[i] = new Color4b((byte)(55 - (int)((255 - i) / 5.0f)), (byte)(150 - (int)((255 - i) / 2.0f)), 255, (byte)(100 - i / 3));
            }

            public override bool BeginUpdate(IParticleSystem particleSystem, float timeSinceLastFrame)
            {
                return true;
            }

            public override void Update(Particle particle, float timeSinceLastFrame, float systemX, float systemY, float systemZ)
            {
                particle.VelZ -= 200.0f * timeSinceLastFrame;
                ApplyVelocity(particle, timeSinceLastFrame);
                particle.Time -= timeSinceLastFrame;

                // set color
                if (particle.Time > 0.0f)
                {
                    int colorIndex = (int)(((AliveTime - particle.Time) / AliveTime) * (colorList.Length - 1));
                    particle.Color = colorList[Math.Min(colorIndex, colorList.Length - 1)];
                }
            }

            public override void InitializeNew(Particle particle, float startX, float startY, float startZ)
            {
                float angle = (float)random.NextDouble() * 2.0f * 3.141592f;
                float totalVel = (float)random.NextDouble() * 5.0f + 3.0f;
                particle.VelX = (float)Math.Cos(angle) * totalVel;
                particle.VelZ = (float)random.NextDouble() * 30.0f + 110.0f;
                particle.VelY = (float)Math.Sin(angle) * totalVel;

                particle.X = startX;
                particle.Y = startY;
                particle.Z = startZ;

                particle.Color = colorList[0];
                particle.Time = AliveTime;
            }
        }
        #endregion

        #region LightBugsUpdater class
        private class LightBugsUpdater : ParticleController
        {
            private const float BugDeacceleration = 15.0f;
            private static readonly Random random = new Random();

            public override bool BeginUpdate(IParticleSystem particleSystem, float timeSinceLastFrame)
            {
                return true;
            }

            public override void Update(Particle particle, float timeSinceLastFrame, float systemX, float systemY, float systemZ)
            {
                if (particle.X > systemX)
                    particle.VelX -= BugDeacceleration * timeSinceLastFrame;
                if (particle.X < systemX)
                    particle.VelX += BugDeacceleration * timeSinceLastFrame;
                if (particle.Y > systemY)
                    particle.VelY -= BugDeacceleration * timeSinceLastFrame;
                if (particle.Y < systemY)
                    particle.VelY += BugDeacceleration * timeSinceLastFrame;
                if (particle.Z > systemZ)
                    particle.VelZ -= BugDeacceleration * timeSinceLastFrame;
                if (particle.Z < systemZ)
                    particle.VelZ += BugDeacceleration * timeSinceLastFrame;

                ApplyVelocity(particle, timeSinceLastFrame);
                particle.Color = CreateColor();
            }

            public override void InitializeNew(Particle particle, float startX, float startY, float startZ)
            {
                particle.VelX = (float)random.NextDouble() * 20.0f - 10.0f;
                particle.VelZ = (float)random.NextDouble() * 20.0f - 10.0f;
                particle.VelY = (float)random.NextDouble() * 20.0f - 10.0f;

                particle.X = (float)random.NextDouble() * 20.0f + startX - 10.0f;
                particle.Y = (float)random.NextDouble() * 20.0f + startY - 10.0f;
                particle.Z = (float)random.NextDouble() * 20.0f + startZ - 10.0f;

                particle.Color = CreateColor();
                particle.Time = 1.0f;
            }

            private static Color4b CreateColor()
            {
                byte intensity = (byte)(150 + random.Next(100));
                return new Color4b(intensity, intensity, intensity, 155);
            }
        }
        #endregion
    }
}
