using System;
using System.Collections.Generic;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Particles;
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
            uint[, ,] particleVoxels = new uint[1, 1, 1];
            particleVoxels[0, 0, 0] = 0xFFFFFFFF;
            for (int i = 0; i < 64; i++)
            {
                yield return CreateBlockInfo("lava" + i, new Color4f(200, 15, 8, 255), 1.0f, i,
                    new LightComponent(new Vector3b(blockCenter, blockCenter, blockCenter), new Color3f(0.2f, 0.01f, 0.001f), forFantasyLighting ? 4 : 6), true);
                yield return CreateBlockInfo("ston" + i, new Color4f(120, 120, 120, 255), 1.0f, i);
                yield return CreateBlockInfo("sand" + i, new Color4f(120, 100, 20, 255), 0.1f, i, null, true);
            }

            for (int i = 0; i < 5; i++)
            {
                yield return CreateBlockInfo("back" + i, true, 0, new Color4f(235, 235, 235, 255), 1.0f,
                    new ParticleSystemComponent(particleVoxels, new SnowUpdater(), new Vector3b(0, 0, 0), 100, 1, TransparencyType.None, true), null, true);
            }

            Block fireBlock = new Block("fire");
            fireBlock.AddComponent(new ParticleSystemComponent(particleVoxels, new FireUpdater(), new Vector3b(blockCenter, blockCenter, 1), 300, 400, TransparencyType.Additive, false));
            fireBlock.AddComponent(new LightComponent(new Vector3b(blockCenter, blockCenter, 4), new Color3f(1.0f, 0.8f, 0.6f), forFantasyLighting ? 5 : 10));
            yield return fireBlock;

            yield return CreateBlockInfo("light0", false, 2, new Color4f(255, 255, 255, 255), 1.0f, null,
                new LightComponent(blockCenterVector, new Color3f(1.0f, 1.0f, 1.0f), forFantasyLighting ? 10 : 20));

            yield return CreateBlockInfo("light1", false, 2, new Color4f(255, 255, 0, 255), 1.0f, null,
                new LightComponent(blockCenterVector, new Color3f(1.0f, 1.0f, 0.0f), forFantasyLighting ? 10 : 20));

            yield return CreateBlockInfo("light2", false, 2, new Color4f(0, 255, 0, 255), 1.0f, null,
                new LightComponent(blockCenterVector, new Color3f(0.0f, 1.0f, 0.0f), forFantasyLighting ? 10 : 20));

            yield return CreateBlockInfo("light3", false, 2, new Color4f(0, 255, 255, 255), 1.0f, null,
                new LightComponent(blockCenterVector, new Color3f(0.0f, 1.0f, 1.0f), forFantasyLighting ? 10 : 20));

            yield return CreateBlockInfo("light4", false, 2, new Color4f(0, 0, 255, 255), 1.0f, null,
                new LightComponent(blockCenterVector, new Color3f(0.0f, 0.0f, 1.0f), forFantasyLighting ? 10 : 20));

            yield return CreateBlockInfo("light5", false, 2, new Color4f(255, 0, 255, 255), 1.0f, null,
                new LightComponent(blockCenterVector, new Color3f(1.0f, 0.0f, 1.0f), forFantasyLighting ? 10 : 20));

            yield return CreateBlockInfo("light6", false, 2, new Color4f(255, 255, 255, 255), 1.0f, null,
                new LightComponent(blockCenterVector, new Color3f(1.0f, 1.0f, 1.0f), forFantasyLighting ? 10 : 20));

            particleVoxels = new uint[3, 3, 3];
            particleVoxels[1, 1, 1] = 0xFFFFFFFF;
            particleVoxels[0, 1, 1] = 0xFFFFFFFF;
            particleVoxels[2, 1, 1] = 0xFFFFFFFF;
            particleVoxels[1, 0, 1] = 0xFFFFFFFF;
            particleVoxels[1, 2, 1] = 0xFFFFFFFF;
            particleVoxels[1, 1, 0] = 0xFFFFFFFF;
            particleVoxels[1, 1, 2] = 0xFFFFFFFF;
            yield return CreateBlockInfo("fountain", false, Block.VoxelSize / 2, new Color4f(20, 20, 150, 255), 1.0f,
                new ParticleSystemComponent(particleVoxels, new FountainUpdater(), new Vector3b(blockCenter, blockCenter, 13), 100, 300, TransparencyType.Realistic, true));
        }

        private static Block CreateBlockInfo(string name, Color4f color, float voxelDensity, int sides, 
            LightComponent light = null, bool allowLightPassthrough = false)
        {
            const float mid = Block.VoxelSize / 2.0f - 0.5f;
            float sphereSize = Block.VoxelSize / 2.0f;

            Block block = new Block(name);
            if (light != null)
            {
                block.AddComponent(light);
                block.AddComponent(new UnlitComponent());
            }
            if (allowLightPassthrough || light != null)
                block.AddComponent(new TransparentComponent());

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
                            block[x, y, z] = CreateColorFromColor(color).ToArgb();
                    }
                }
            }

            return block;
        }

        private static Block CreateBlockInfo(string name, bool frontOnly, float sphereSize, Color4f color, float voxelDensity,
            ParticleSystemComponent particleSystem = null, LightComponent light = null, bool allowLightPassthrough = false)
        {
            const int mid = Block.VoxelSize / 2;

            Block block = new Block(name);
            if (particleSystem != null)
                block.AddComponent(particleSystem);
            if (light != null)
            {
                block.AddComponent(light);
                block.AddComponent(new UnlitComponent());
            }
            if (allowLightPassthrough || light != null)
                block.AddComponent(new TransparentComponent());

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
                            block[x, y, z] = (light == null) ? CreateColorFromColor(color).ToArgb() : ((Color4b)color).ToArgb();
                    }
                }
            }

            return block;
        }

        private static Color4b CreateColorFromColor(Color4f seed)
        {
            float scale = (float)(random.NextDouble() * 0.1 + 0.95);
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

                if (particle.X > systemX + Block.VoxelSize)
                    particle.VelX -= SnowDeacceleration * timeSinceLastFrame;
                if (particle.X < systemX)
                    particle.VelX += SnowDeacceleration * timeSinceLastFrame;
                if (particle.Y > systemY + Block.VoxelSize)
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

                particle.X = startX + random.Next(Block.VoxelSize);
                particle.Y = startY + random.Next(Block.VoxelSize);
                if (startAtTop)
                    particle.Z = 59 * Block.VoxelSize;
                else
                    particle.Z = random.Next(56 * Block.VoxelSize) + 3 * Block.VoxelSize;

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

            private readonly Random random = new Random();
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
            private readonly Random random = new Random();
            private static readonly Color4b[] colorList = new Color4b[256];

            static FountainUpdater()
            {
                for (int i = 0; i < 256; i++)
                    colorList[i] = new Color4b((byte)(55 - (int)((255 - i) / 5.0f)), (byte)(150 - (int)((255 - i) / 2.0f)), 255, (byte)(100 - i / 3));
            }

            #region Implementation of ParticleController
            public override bool BeginUpdate(IParticleSystem particleSystem, float timeSinceLastFrame)
            {
                return true;
            }

            private const float AliveTime = 2.0f;

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
            #endregion
        }
        #endregion
    }
}
