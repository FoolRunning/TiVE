using System;
using System.Collections.Generic;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVE.RenderSystem;
using ProdigalSoftware.TiVE.VoxelMeshSystem;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.ParticleSystem
{
    /// <summary>
    /// Represents a single particle emitter. Responsible for updating all particles owned by itself.
    /// </summary>
    internal sealed class ParticleEmitter : IDisposable
    {
        private static readonly ParticleSorter particleSorter = new ParticleSorter();

        private readonly object syncObj = new object();
        private readonly Particle[] particles;
        private readonly IRendererData voxelInstanceLocationData;
        private readonly IRendererData voxelInstanceNormalData;
        private readonly IRendererData voxelInstanceVoxelStateData;
        private readonly Vector3us[] locations;
        private readonly Color4b[] colors;
        private readonly int voxelsPerParticle;
        private readonly int renderedVoxelsPerParticle;

        private IVertexDataCollection instances;
        private IRendererData locationData;
        private IRendererData colorData;
        private float numOfParticlesNeeded;
        private int aliveParticles;

        public ParticleEmitter(ParticleController controller)
        {
            this.Controller = controller;
            particles = new Particle[controller.MaxParticles];
            for (int i = 0; i < particles.Length; i++)
                particles[i] = new Particle();

            // Create particle voxel model to be used for each particle
            MeshBuilder voxelInstanceBuilder = new MeshBuilder(controller.ParticleSprite.VoxelCount);
            VoxelMeshUtils.GenerateMesh(controller.ParticleSprite, voxelInstanceBuilder, out voxelsPerParticle, out renderedVoxelsPerParticle);
            voxelInstanceLocationData = voxelInstanceBuilder.GetLocationData();
            voxelInstanceNormalData = voxelInstanceBuilder.GetNormalData();
            voxelInstanceVoxelStateData = voxelInstanceBuilder.GetVoxelStateData();

            locations = new Vector3us[controller.MaxParticles];
            colors = new Color4b[controller.MaxParticles];
        }

        /// <summary>
        /// Cleans up data used by all the particles systems in this collection
        /// </summary>
        public void Dispose()
        {
            voxelInstanceLocationData.Dispose();
            voxelInstanceNormalData.Dispose();
            voxelInstanceVoxelStateData.Dispose();
            instances?.Dispose();
        }

        #region Properties
        public ParticleController Controller { get; }

        public TransparencyType TransparencyType => Controller.TransparencyType;

        public Vector3i Location { get; private set; }
        #endregion

        public void Reset(Vector3i newLocation)
        {
            for (int i = 0; i < particles.Length; i++)
                particles[i].Time = 0.0f;

            Location = newLocation;
            numOfParticlesNeeded = 0;
            aliveParticles = 0;
        }

        /// <summary>
        /// Renders all particles in all systems in this collection
        /// </summary>
        public RenderStatistics Render()
        {
            if (instances == null)
                instances = CreateInstanceDataBuffer(out locationData, out colorData);

            // Put the data for the current particles into the graphics memory and draw them
            lock (syncObj)
            {
                locationData.UpdateData(locations, aliveParticles);
                colorData.UpdateData(colors, aliveParticles);
            }
            instances.Bind();
            TiVEController.Backend.Draw(PrimitiveType.Points, instances);

            return new RenderStatistics(1, aliveParticles * voxelsPerParticle, aliveParticles * renderedVoxelsPerParticle);
        }
        
        /// <summary>
        /// Updates all particle systems in this collection
        /// </summary>
        public void UpdateAll(ref Vector3i cameraLocation, float timeSinceLastFrame)
        {
            UpdateInternal(ref cameraLocation, timeSinceLastFrame);
            lock (syncObj)
            {
                bool isLit = Controller.IsLit;
                for (int i = 0; i < aliveParticles; i++)
                {
                    Particle part = particles[i];
                    if (part.X < 0.0f || part.Y < 0.0f || part.Z < 0.0f)
                        continue; // Can't be cast to a ushort

                    ushort partX = (ushort)part.X;
                    ushort partY = (ushort)part.Y;
                    ushort partZ = (ushort)part.Z;
                    locations[i] = new Vector3us(partX, partY, partZ);
                    colors[i] = part.Color;
                }
            }
        }
        
        private void UpdateInternal(ref Vector3i cameraLocation, float timeSinceLastUpdate)
        {
            numOfParticlesNeeded += Controller.ParticlesPerSecond * timeSinceLastUpdate;
            int newParticleCount = Math.Min((int)numOfParticlesNeeded, particles.Length - aliveParticles);
            numOfParticlesNeeded -= newParticleCount;

            Vector3i loc = Location;
            for (int i = 0; i < aliveParticles; i++)
            {
                Particle part = particles[i];
                if (part.Time > 0.0f)
                {
                    // Normal case - particle is still alive, so just update it
                    Controller.Update(part, timeSinceLastUpdate, loc);
                }
                else if (newParticleCount > 0)
                {
                    // Particle died, but we need new particles so just re-initialize this one
                    Controller.InitializeNew(part, loc);
                    newParticleCount--;
                }
                else
                {
                    // Particle died - replace with an existing alive particle
                    int lastAliveIndex = aliveParticles - 1;
                    Particle lastAlive = particles[lastAliveIndex];
                    particles[lastAliveIndex] = part;
                    particles[i] = lastAlive;
                    part = lastAlive;
                    aliveParticles--;
                    // Just replaced current dead particle with an alive one. Need to update it.
                    Controller.Update(part, timeSinceLastUpdate, loc);
                }
            }

            // Intialize any new particles that are still needed
            for (int i = 0; i < newParticleCount; i++)
            {
                Particle part = particles[aliveParticles];
                Controller.InitializeNew(part, loc);
                aliveParticles++;
            }

            if (Controller.TransparencyType == TransparencyType.Realistic)
            {
                particleSorter.CameraLocation = cameraLocation;
                Array.Sort(particles, particleSorter);
            }
        }

        private IVertexDataCollection CreateInstanceDataBuffer(out IRendererData locData, out IRendererData colData)
        {
            IVertexDataCollection instanceData = TiVEController.Backend.CreateVertexDataCollection();
            instanceData.AddBuffer(voxelInstanceLocationData);
            instanceData.AddBuffer(voxelInstanceNormalData);
            instanceData.AddBuffer(voxelInstanceVoxelStateData);

            locData = TiVEController.Backend.CreateData(locations, 0, 3, DataType.Instance, DataValueType.UShort, false, true);
            instanceData.AddBuffer(locData);
            colData = TiVEController.Backend.CreateData(colors, 0, 4, DataType.Instance, DataValueType.Byte, true, true);
            instanceData.AddBuffer(colData);
            instanceData.Initialize();

            return instanceData;
        }

        #region ParticleSorter class
        private sealed class ParticleSorter : IComparer<Particle>
        {
            public Vector3i CameraLocation;

            public int Compare(Particle p1, Particle p2)
            {
                if (p1.Time <= 0.0f && p2.Time <= 0.0f)
                    return 0;

                if (p1.Time <= 0.0f)
                    return 1;

                if (p2.Time <= 0.0f)
                    return -1;

                int p1DistX = (int)p1.X - CameraLocation.X;
                int p1DistY = (int)p1.Y - CameraLocation.Y;
                int p1DistZ = (int)p1.Z - CameraLocation.Z;
                int p1DistSquared = p1DistX * p1DistX + p1DistY * p1DistY + p1DistZ * p1DistZ;

                int p2DistX = (int)p2.X - CameraLocation.X;
                int p2DistY = (int)p2.Y - CameraLocation.Y;
                int p2DistZ = (int)p2.Z - CameraLocation.Z;
                int p2DistSquared = p2DistX * p2DistX + p2DistY * p2DistY + p2DistZ * p2DistZ;
                return p2DistSquared - p1DistSquared;
            }
        }
        #endregion
    }
}
