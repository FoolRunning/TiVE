using System;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Generators;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.ProjectM.Plugins
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

            private static readonly Random random = new Random();

            public SnowUpdater()
                : base(1, 100, TransparencyType.None, true)
            {
            }

            #region Implementation of ParticleController
            public override uint[, ,] ParticleVoxels
            {
                get { return new[, ,] { { { 0xFFFFFFFF } } }; }
            }

            public override void Update(Particle particle, float timeSinceLastFrame, Vector3i systemLocation)
            {
                ApplyVelocity(particle, timeSinceLastFrame);

                if (particle.X > systemLocation.X + Block.VoxelSize)
                    particle.VelX -= SnowDeacceleration * timeSinceLastFrame;
                if (particle.X < systemLocation.X)
                    particle.VelX += SnowDeacceleration * timeSinceLastFrame;
                if (particle.Y > systemLocation.Y + Block.VoxelSize)
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

            private static void InitNewInternal(Particle particle, Vector3i systemLocation, bool startAtTop)
            {
                particle.VelX = (float)random.NextDouble() * 48.0f - 24.0f;
                particle.VelZ = (float)random.NextDouble() * -30.0f - 20.0f;
                particle.VelY = (float)random.NextDouble() * 48.0f - 24.0f;

                particle.X = systemLocation.X + random.Next(Block.VoxelSize);
                particle.Y = systemLocation.Y + random.Next(Block.VoxelSize);
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

            public FireUpdater()
                : base(400, 300, TransparencyType.Additive, false)
            {
            }

            #region Implementation of ParticleController
            public override uint[, ,] ParticleVoxels
            {
                get { return new[, ,] { { { 0xFFFFFFFF } } }; }
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
                float angle = (float)random.NextDouble() * 2.0f * 3.141592f;
                float totalVel = (float)random.NextDouble() * 6.0f + 10.0f;
                particle.VelX = (float)Math.Cos(angle) * totalVel;
                particle.VelZ = (float)random.NextDouble() * 11.0f + 8.0f;
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

            private readonly Random random = new Random();
            private static readonly Color4b[] colorList = new Color4b[256];

            static FountainUpdater()
            {
                for (int i = 0; i < 256; i++)
                    colorList[i] = new Color4b((byte)(55 - (int)((255 - i) / 5.0f)), (byte)(150 - (int)((255 - i) / 2.0f)), 255, (byte)(100 - i / 3));
            }

            public FountainUpdater()
                : base(300, 100, TransparencyType.Realistic, true)
            {
            }

            #region Implementation of ParticleController
            public override uint[, ,] ParticleVoxels
            {
                get
                {
                    uint[, ,] particleVoxels = new uint[3, 3, 3];
                    particleVoxels[1, 1, 1] = 0xFFFFFFFF;
                    particleVoxels[0, 1, 1] = 0xFFFFFFFF;
                    particleVoxels[2, 1, 1] = 0xFFFFFFFF;
                    particleVoxels[1, 0, 1] = 0xFFFFFFFF;
                    particleVoxels[1, 2, 1] = 0xFFFFFFFF;
                    particleVoxels[1, 1, 0] = 0xFFFFFFFF;
                    particleVoxels[1, 1, 2] = 0xFFFFFFFF;
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
                float angle = (float)random.NextDouble() * 2.0f * 3.141592f;
                float totalVel = (float)random.NextDouble() * 5.0f + 3.0f;
                particle.VelX = (float)Math.Cos(angle) * totalVel;
                particle.VelZ = (float)random.NextDouble() * 30.0f + 110.0f;
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

        #region LightBugsUpdater class
        private class LightBugsUpdater : ParticleController
        {
            private const float BugDeacceleration = 15.0f;
            private static readonly Random random = new Random();

            public LightBugsUpdater() : base(10, 15, TransparencyType.Realistic, true)
            {
            }

            #region Implementation of ParticleController
            public override uint[,,] ParticleVoxels
            {
                get { return new[, ,] { { { 0xFFFFFFFF } } }; }
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
                particle.VelX = (float)random.NextDouble() * 20.0f - 10.0f;
                particle.VelZ = (float)random.NextDouble() * 20.0f - 10.0f;
                particle.VelY = (float)random.NextDouble() * 20.0f - 10.0f;

                particle.X = (float)random.NextDouble() * 20.0f + systemLocation.X - 10.0f;
                particle.Y = (float)random.NextDouble() * 20.0f + systemLocation.Y - 10.0f;
                particle.Z = (float)random.NextDouble() * 20.0f + systemLocation.Z - 10.0f;

                particle.Color = CreateColor();
                particle.Time = 1.0f;
            }
            #endregion

            private static Color4b CreateColor()
            {
                byte intensity = (byte)(150 + random.Next(100));
                return new Color4b(intensity, intensity, intensity, 200);
            }
        }
        #endregion
    }
}
