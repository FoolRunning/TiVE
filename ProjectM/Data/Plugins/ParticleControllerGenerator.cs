using System;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Generators;

namespace ProdigalSoftware.ProjectM.Data.Plugins
{
    public class ParticleControllerGenerator : IParticleControllerGenerator
    {
        #region Implementation of IParticleControllerGenerator
        public ParticleController CreateController(string name)
        {
            switch (name)
            {
                case "Snow": return new SnowUpdater();
                case "Fire": return new FireUpdater();
                case "Lava": return new LavaUpdater();
                case "Fountain": return new FountainUpdater();
                case "LightBugs": return new LightBugsUpdater();
                default: return null;
            }
        }
        #endregion
        
        #region SnowUpdater class
        private class SnowUpdater : ParticleController
        {
            private const float SnowDeacceleration = 21.0f;
            private const float AliveTime = 30.0f;

            public SnowUpdater()
                : base(1, 100, TransparencyType.None, true)
            {
            }

            #region Implementation of ParticleController
            public override VoxelSprite ParticleSprite
            {
                get 
                {
                    VoxelSprite sprite = new VoxelSprite(1, 1, 1);
                    sprite[0, 0, 0] = Voxel.White;
                    return sprite;
                }
            }

            public override void Update(Particle particle, float timeSinceLastFrame, Vector3i systemLocation)
            {
                ApplyVelocity(particle, timeSinceLastFrame);

                if (particle.X > systemLocation.X + BlockLOD32.VoxelSize)
                    particle.VelX -= SnowDeacceleration * timeSinceLastFrame;
                if (particle.X < systemLocation.X)
                    particle.VelX += SnowDeacceleration * timeSinceLastFrame;
                if (particle.Y > systemLocation.Y + BlockLOD32.VoxelSize)
                    particle.VelY -= SnowDeacceleration * timeSinceLastFrame;
                if (particle.Y < systemLocation.Y)
                    particle.VelY += SnowDeacceleration * timeSinceLastFrame;

                if (particle.Z < 0)
                    InitNewInternal(particle, systemLocation, true);

                //particle.Time -= timeSinceLastFrame;
            }

            public override void InitializeNew(Particle particle, Vector3i systemLocation)
            {
                InitNewInternal(particle, systemLocation, false);
            }
            #endregion

            private void InitNewInternal(Particle particle, Vector3i systemLocation, bool startAtTop)
            {
                particle.VelX = Random.NextFloat() * 48.0f - 24.0f;
                particle.VelZ = Random.NextFloat() * -30.0f - 20.0f;
                particle.VelY = Random.NextFloat() * 48.0f - 24.0f;

                particle.X = systemLocation.X + Random.Next(BlockLOD32.VoxelSize);
                particle.Y = systemLocation.Y + Random.Next(BlockLOD32.VoxelSize);
                if (startAtTop)
                    particle.Z = 59 * BlockLOD32.VoxelSize;
                else
                    particle.Z = Random.Next(56 * BlockLOD32.VoxelSize) + 3 * BlockLOD32.VoxelSize;

                particle.Color = new Color4b(255, 255, 255, 255);
                particle.Time = Random.NextFloat() * AliveTime / 2.0f + AliveTime / 2.0f;
            }
        }
        #endregion

        #region FireUpdater class
        private class FireUpdater : ParticleController
        {
            private const float FlameDeacceleration = 35.0f;
            private const float AliveTime = 1.0f;

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

            public FireUpdater()
                : base(400, 300, TransparencyType.Additive, false)
            {
            }

            #region Implementation of ParticleController
            public override VoxelSprite ParticleSprite
            {
                get
                {
                    VoxelSprite sprite = new VoxelSprite(1, 1, 1);
                    sprite[0, 0, 0] = Voxel.White;
                    return sprite;
                }
            }

            public override void Update(Particle particle, float timeSinceLastFrame, Vector3i systemLocation)
            {
                ApplyVelocity(particle, timeSinceLastFrame);

                if (particle.X > systemLocation.X)
                    particle.VelX -= FlameDeacceleration * timeSinceLastFrame;
                if (particle.X < systemLocation.X)
                    particle.VelX += FlameDeacceleration * timeSinceLastFrame;
                if (particle.Y > systemLocation.Y)
                    particle.VelY -= FlameDeacceleration * timeSinceLastFrame;
                if (particle.Y < systemLocation.Y)
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

            public override void InitializeNew(Particle particle, Vector3i systemLocation)
            {
                float angle = Random.NextFloat() * 2.0f * 3.141592f;
                float totalVel = Random.NextFloat() * 6.0f + 10.0f;
                particle.VelX = (float)Math.Cos(angle) * totalVel;
                particle.VelZ = Random.NextFloat() * 11.0f + 8.0f;
                particle.VelY = (float)Math.Sin(angle) * totalVel;

                particle.X = systemLocation.X;
                particle.Y = systemLocation.Y;
                particle.Z = systemLocation.Z;

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

            private static readonly Color4b[] colorList = new Color4b[256];

            static FountainUpdater()
            {
                for (int i = 0; i < 256; i++)
                    colorList[i] = new Color4b((byte)(55 - (int)((255 - i) / 5.0f)), (byte)(150 - (int)((255 - i) / 2.0f)), 255, (byte)(100 - i / 4));
            }

            public FountainUpdater() : base(300, 100, TransparencyType.Realistic, true)
            {
            }

            #region Implementation of ParticleController
            public override VoxelSprite ParticleSprite
            {
                get
                {
                    VoxelSprite particleVoxels = new VoxelSprite(5, 5, 5);
                    const int mid = 5 / 2;

                    for (int z = 0; z < 5; z++)
                    {
                        for (int x = 0; x < 5; x++)
                        {
                            for (int y = 0; y < 5; y++)
                            {
                                int dist = (x - mid) * (x - mid) + (y - mid) * (y - mid) + (z - mid) * (z - mid);
                                if (dist <= 4)
                                    particleVoxels[x, y, z] = Voxel.White;
                            }
                        }
                    }
                    return particleVoxels;
                }
            }

            public override void Update(Particle particle, float timeSinceLastFrame, Vector3i systemLocation)
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

            public override void InitializeNew(Particle particle, Vector3i systemLocation)
            {
                float angle = Random.NextFloat() * 2.0f * 3.141592f;
                float totalVel = Random.NextFloat() * 10.0f + 3.0f;
                particle.VelX = (float)Math.Cos(angle) * totalVel;
                particle.VelZ = Random.NextFloat() * 30.0f + 180.0f;
                particle.VelY = (float)Math.Sin(angle) * totalVel;

                particle.X = systemLocation.X - 2;
                particle.Y = systemLocation.Y - 2;
                particle.Z = systemLocation.Z - 2;

                particle.Color = colorList[0];
                particle.Time = AliveTime;
            }
            #endregion
        }
        #endregion

        #region LavaUpdater class
        private class LavaUpdater : ParticleController
        {
            private const int SpriteSize = (int)(BlockLOD32.VoxelSize * 0.333f);
            private const int SpriteMid = SpriteSize / 2;
            private const float AliveTime = 5.0f;

            private static readonly Color4b[] colorList = new Color4b[256];

            static LavaUpdater()
            {
                for (int i = 0; i < 256; i++)
                    colorList[i] = new Color4b((byte)(255 - i), (byte)(10 - i / 30), (byte)(5 - i / 70), (byte)(255 - i));
            }

            public LavaUpdater() : base(25, 4, TransparencyType.Realistic, false)
            {
            }

            #region Implementation of ParticleController
            public override VoxelSprite ParticleSprite
            {
                get
                {
                    const int sphereRadius = SpriteMid - 1;

                    VoxelSprite particleVoxels = new VoxelSprite(SpriteSize, SpriteSize, SpriteSize);
                    for (int z = 0; z < SpriteSize; z++)
                    {
                        for (int x = 0; x < SpriteSize; x++)
                        {
                            for (int y = 0; y < SpriteSize; y++)
                            {
                                int dist = (x - SpriteMid) * (x - SpriteMid) + (y - SpriteMid) * (y - SpriteMid) + (z - SpriteMid) * (z - SpriteMid);
                                if (dist > sphereRadius * sphereRadius)
                                    continue;

                                particleVoxels[x, y, z] = Voxel.White;
                            }
                        }
                    }
                    return particleVoxels;
                }
            }

            public override void Update(Particle particle, float timeSinceLastFrame, Vector3i systemLocation)
            {
                ApplyVelocity(particle, timeSinceLastFrame);
                if (particle.Z > systemLocation.Z - SpriteMid + 1)
                    particle.VelZ = 0.0f;

                particle.Time -= timeSinceLastFrame;

                // set color
                if (particle.Time > 0.0f)
                {
                    int colorIndex = (int)(((AliveTime - particle.Time) / AliveTime) * (colorList.Length - 1));
                    particle.Color = colorList[Math.Min(colorIndex, colorList.Length - 1)];
                }
            }

            public override void InitializeNew(Particle particle, Vector3i systemLocation)
            {
                particle.VelZ = SpriteSize * 0.8f;

                particle.X = systemLocation.X + Random.Next(BlockLOD32.VoxelSize + SpriteMid) - SpriteMid + 1;
                particle.Y = systemLocation.Y + Random.Next(BlockLOD32.VoxelSize + SpriteMid) - SpriteMid + 1;
                particle.Z = systemLocation.Z - SpriteSize - 2;

                particle.Color = colorList[0];
                particle.Time = Random.NextFloat() * AliveTime / 2.0f + AliveTime / 2.0f;
            }
            #endregion
        }
        #endregion

        #region LightBugsUpdater class
        private class LightBugsUpdater : ParticleController
        {
            private const float BugDeacceleration = 30.0f;

            public LightBugsUpdater() : base(10, 15, TransparencyType.Realistic, true)
            {
            }

            #region Implementation of ParticleController
            public override VoxelSprite ParticleSprite
            {
                get
                {
                    VoxelSprite sprite = new VoxelSprite(1, 1, 1);
                    sprite[0, 0, 0] = Voxel.White;
                    return sprite;
                }
            }

            public override void Update(Particle particle, float timeSinceLastFrame, Vector3i systemLocation)
            {
                if (particle.X > systemLocation.X)
                    particle.VelX -= BugDeacceleration * timeSinceLastFrame;
                if (particle.X < systemLocation.X)
                    particle.VelX += BugDeacceleration * timeSinceLastFrame;
                if (particle.Y > systemLocation.Y)
                    particle.VelY -= BugDeacceleration * timeSinceLastFrame;
                if (particle.Y < systemLocation.Y)
                    particle.VelY += BugDeacceleration * timeSinceLastFrame;
                if (particle.Z > systemLocation.Z)
                    particle.VelZ -= BugDeacceleration * timeSinceLastFrame;
                if (particle.Z < systemLocation.Z)
                    particle.VelZ += BugDeacceleration * timeSinceLastFrame;

                ApplyVelocity(particle, timeSinceLastFrame);
                particle.Color = CreateColor();
            }

            public override void InitializeNew(Particle particle, Vector3i systemLocation)
            {
                particle.VelX = Random.NextFloat() * 40.0f - 20.0f;
                particle.VelZ = Random.NextFloat() * 40.0f - 20.0f;
                particle.VelY = Random.NextFloat() * 40.0f - 20.0f;

                particle.X = Random.NextFloat() * 20.0f + systemLocation.X - 10.0f;
                particle.Y = Random.NextFloat() * 20.0f + systemLocation.Y - 10.0f;
                particle.Z = Random.NextFloat() * 20.0f + systemLocation.Z - 10.0f;

                particle.Color = CreateColor();
                particle.Time = 1.0f;
            }
            #endregion

            private static Color4b CreateColor()
            {
                byte intensity = (byte)(50 + Random.Next(100));
                return new Color4b(intensity, intensity, intensity, 200);
            }
        }
        #endregion
    }
}
