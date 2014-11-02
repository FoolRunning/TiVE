﻿using System;
using System.Collections.Generic;
using OpenTK.Graphics;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Lighting;
using ProdigalSoftware.TiVEPluginFramework.Particles;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.ProjectM.Controllers
{
    public class DefaultBlockLoader : IBlockGenerator
    {
        private static readonly Random random = new Random();

        public IEnumerable<BlockInformation> CreateBlocks()
        {
            for (int i = 0; i < 5; i++)
            {
                yield return CreateBlockInfo("dirt" + i, false, 4.3f, new Color4(129, 53, 2, 255), 1.0f);
                yield return CreateBlockInfo("back" + i, true, 0, new Color4(235, 235, 235, 255), 1.0f);
                yield return CreateBlockInfo("stone" + i, false, 0, new Color4(120, 120, 120, 255), 1.0f);
                yield return CreateBlockInfo("sand" + i, false, 0, new Color4(120, 100, 20, 255), 0.2f);
            }

            uint[,,] particleVoxels = new uint[1, 1, 1];
            particleVoxels[0, 0, 0] = 0xFFFFFFFF;
            yield return new BlockInformation("fire", 
                new ParticleSystemInformation(particleVoxels, new FireUpdater(), new Vector3b(5, 5, 1), 300, 310, true, false),
                new PointLight(new Vector3b(5, 5, 1), new Color3f(1.0f, 0.9f, 0.8f), 0.005f));

            yield return CreateBlockInfo("light0", false, 2, new Color4(255, 40, 40, 255), 1.0f, null,
                new PointLight(new Vector3b(5, 5, 5), new Color3f(1.0f, 0.05f, 0.05f), 0.005f));

            yield return CreateBlockInfo("light1", false, 2, new Color4(255, 255, 40, 255), 1.0f, null,
                new PointLight(new Vector3b(5, 5, 5), new Color3f(1.0f, 1.0f, 0.05f), 0.005f));

            yield return CreateBlockInfo("light2", false, 2, new Color4(40, 255, 40, 255), 1.0f, null,
                new PointLight(new Vector3b(5, 5, 5), new Color3f(0.05f, 1.0f, 0.05f), 0.005f));

            yield return CreateBlockInfo("light3", false, 2, new Color4(40, 255, 255, 255), 1.0f, null,
                new PointLight(new Vector3b(5, 5, 5), new Color3f(0.05f, 1.0f, 1.0f), 0.005f));

            yield return CreateBlockInfo("light4", false, 2, new Color4(40, 40, 255, 255), 1.0f, null,
                new PointLight(new Vector3b(5, 5, 5), new Color3f(0.05f, 0.05f, 1.0f), 0.005f));

            yield return CreateBlockInfo("light5", false, 2, new Color4(255, 40, 255, 255), 1.0f, null,
                new PointLight(new Vector3b(5, 5, 5), new Color3f(1.0f, 0.05f, 1.0f), 0.005f));

            yield return new BlockInformation("snow",
                new ParticleSystemInformation(particleVoxels, new SnowUpdater(), new Vector3b(0, 0, 0), 100, 1, false, true));

            particleVoxels = new uint[3, 3, 3];
            particleVoxels[1, 1, 1] = 0xFFFFFFFF;
            particleVoxels[0, 1, 1] = 0xFFFFFFFF;
            particleVoxels[2, 1, 1] = 0xFFFFFFFF;
            particleVoxels[1, 0, 1] = 0xFFFFFFFF;
            particleVoxels[1, 2, 1] = 0xFFFFFFFF;
            particleVoxels[1, 1, 0] = 0xFFFFFFFF;
            particleVoxels[1, 1, 2] = 0xFFFFFFFF;
            yield return CreateBlockInfo("fountain", false, 4, new Color4(20, 20, 150, 255), 1.0f,
                new ParticleSystemInformation(particleVoxels, new FountainUpdater(), new Vector3b(3, 3, 7), 1000, 2200, false, true));
        }

        public IEnumerable<BlockAnimationDefinition> CreateAnimations()
        {
            yield return new BlockAnimationDefinition(100, "sand0", "sand1", "sand2", "sand3", "sand0");
        }

        private static BlockInformation CreateBlockInfo(string name, bool frontOnly, float sphereSize, Color4 color, float voxelDensity,
            ParticleSystemInformation particleSystem = null, ILight light = null)
        {
            const int mid = BlockInformation.VoxelSize / 2;

            BlockInformation block = new BlockInformation(name, particleSystem, light);
            for (int x = 0; x < BlockInformation.VoxelSize; x++)
            {
                for (int y = 0; y < BlockInformation.VoxelSize; y++)
                {
                    for (int z = frontOnly ? BlockInformation.VoxelSize - 1 : 0; z < BlockInformation.VoxelSize; z++)
                    {
                        if (sphereSize > 0)
                        {
                            int dist = (x - mid) * (x - mid) + (y - mid) * (y - mid) + (z - mid) * (z - mid);
                            if (dist > sphereSize * sphereSize)
                                continue;
                        }

                        if (random.NextDouble() < voxelDensity)
                            block[x, y, z] = FromColor(CreateColorFromColor(color));
                    }
                }
            }

            return block;
        }

        private static uint FromColor(Color4 color)
        {
            return (uint)color.ToArgb();
        }

        private static Color4 CreateColorFromColor(Color4 seed)
        {
            float scale = (float)(random.NextDouble() * 0.16 + 0.92);
            return new Color4(Math.Min(seed.R * scale, 1.0f), Math.Min(seed.G * scale, 1.0f), 
                Math.Min(seed.B * scale, 1.0f), seed.A);
        }

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

                particle.Color = new Color4b(255, 255, 255, 100);
                particle.Time = (float)random.NextDouble() * AliveTime / 2.0f + AliveTime / 2.0f;
            }
        }


        private class FireUpdater : ParticleController
        {
            private const float FlameDeacceleration = 27.0f;
            private const float AliveTime = 1.0f;

            private readonly Random random = new Random();
            private static readonly Color4b[] colorList = new Color4b[256];

            static FireUpdater()
            {
                for (int i = 0; i < 256; i++)
                {
                    if (i < 150)
                        colorList[i] = new Color4b(255, (byte)(255 - (i * 1.7f)), (byte)(50 - i / 3), 200);
                    if (i >= 150)
                        colorList[i] = new Color4b((byte)(255 - (i - 150) * 2.4f), 0, 0, 200);
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
                float totalVel = (float)random.NextDouble() * 4.0f + 10.0f;
                particle.VelX = (float)Math.Cos(angle) * totalVel;
                particle.VelZ = (float)random.NextDouble() * 10.0f + 8.0f;
                particle.VelY = (float)Math.Sin(angle) * totalVel;

                particle.X = startX;
                particle.Y = startY;
                particle.Z = startZ;
                
                particle.Color = colorList[0];
                particle.Time = AliveTime;
            }
            #endregion
        }

        private class FountainUpdater : ParticleController
        {
            private readonly Random random = new Random();
            private static readonly Color4b[] colorList = new Color4b[256];

            static FountainUpdater()
            {
                for (int i = 0; i < 256; i++)
                    colorList[i] = new Color4b((byte)(55 - (int)((255 - i) / 5.0f)), (byte)(150 - (int)((255 - i) / 2.0f)), 255, 255);
            }

            #region Implementation of ParticleController
            public override bool BeginUpdate(IParticleSystem particleSystem, float timeSinceLastFrame)
            {
                return true;
            }

            private const float AliveTime = 2.0f;

            public override void Update(Particle particle, float timeSinceLastFrame, float systemX, float systemY, float systemZ)
            {
                particle.VelZ -= 100.0f * timeSinceLastFrame;
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
                float totalVel = (float)random.NextDouble() * 10.0f + 20.0f;
                particle.VelX = (float)Math.Cos(angle) * totalVel;
                particle.VelZ = (float)random.NextDouble() * 10.0f + 40.0f;
                particle.VelY = (float)Math.Sin(angle) * totalVel;

                particle.X = startX;
                particle.Y = startY;
                particle.Z = startZ;

                particle.Color = colorList[0];
                particle.Time = AliveTime;
            }
            #endregion
        }
    }
}
