using System;
using System.Collections.Generic;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVE.RenderSystem;
using ProdigalSoftware.TiVE.RenderSystem.Lighting;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVE.VoxelMeshSystem;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.ParticleSystem
{
    /// <summary>
    /// Holds all the running particle systems for a system type (i.e. all "fountains" are in a collection, all "fires" are in a collection, etc.)
    /// </summary>
    internal sealed class ParticleEmitterCollection : IDisposable
    {
        #region Member variables
        private static readonly ParticleSystemSorter sorter = new ParticleSystemSorter();
        /// <summary>List of particles systems in this collection</summary>
        private readonly List<ParticleEmitter> particleSystems = new List<ParticleEmitter>();
        /// <summary>Copy of the particle systems list used for updating without locking too long</summary>
        private readonly List<ParticleEmitter> updateList = new List<ParticleEmitter>();
        /// <summary>Quick lookup for the index of a particle entity</summary>
        private readonly Dictionary<IEntity, int> particleSystemIndex = new Dictionary<IEntity, int>();

        private readonly ParticleController controller;

        private readonly IRendererData voxelInstanceLocationData;
        private readonly IRendererData voxelInstanceColorData;
        private readonly int polysPerParticle;
        private readonly int voxelsPerParticle;
        private readonly int renderedVoxelsPerParticle;

        private readonly object syncObj = new object();

        private Vector3us[] locations;
        private Color4b[] colors;
        
        private int totalAliveParticles;

        private IVertexDataCollection instances;
        private IRendererData locationData;
        private IRendererData colorData;
        #endregion

        #region Constructor/Dispose
        /// <summary>
        /// Creates a new ParticleEmitterCollection to hold particle systems of the specified type
        /// </summary>
        public ParticleEmitterCollection(ParticleController controller)
        {
            this.controller = controller;

            // Create particle voxel model to be used for each particle
            MeshBuilder voxelInstanceBuilder = new MeshBuilder(1000, 0);
            VoxelMeshUtils.GenerateMesh(controller.ParticleVoxels, voxelInstanceBuilder, true,
                out voxelsPerParticle, out renderedVoxelsPerParticle, out polysPerParticle);

            voxelInstanceLocationData = voxelInstanceBuilder.GetLocationData();
            if (TiVEController.UserSettings.Get(UserSettings.CubifyVoxelsKey))
                voxelInstanceColorData = voxelInstanceBuilder.GetColorData();

            locations = new Vector3us[controller.MaxParticles * 5];
            colors = new Color4b[controller.MaxParticles * 5];

            // Initialize room for 5 particle systems in the collection
            for (int i = 0; i < 5; i++)
                particleSystems.Add(new ParticleEmitter(controller));
        }

        /// <summary>
        /// Cleans up data used by all the particles systems in this collection
        /// </summary>
        public void Dispose()
        {
            voxelInstanceLocationData.Dispose();

            if (instances != null)
                instances.Dispose();

            particleSystems.Clear();
            particleSystemIndex.Clear();
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        public TransparencyType TransparencyType
        {
            get { return controller.TransparencyType; }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Adds the specified particle entity to this collection
        /// </summary>
        public void Add(IEntity entity, ParticleComponent particleData)
        {
            using (new PerformanceLock(particleSystems))
            {
                int availableIndex = particleSystems.FindIndex(sys => !sys.InUse);
                if (availableIndex < 0)
                {
                    // No free space, make room for 10 more particle systems and add the new system in the first new spot
                    availableIndex = particleSystems.Count;
                    for (int i = 0; i < 10; i++)
                        particleSystems.Add(new ParticleEmitter(controller));

                    int origCount = locations.Length;
                    int newCount = origCount + controller.MaxParticles * 10;
                    lock (syncObj)
                    {
                        Array.Resize(ref locations, newCount);
                        Array.Resize(ref colors, newCount);
                    }
                }

                particleSystems[availableIndex].Reset();
                particleSystems[availableIndex].Location = particleData.Location;
                particleSystems[availableIndex].InUse = true;
                particleSystemIndex[entity] = availableIndex;
            }
        }

        /// <summary>
        /// Removes the specified particle entity from this collection
        /// </summary>
        public void Remove(IEntity entity)
        {
            using (new PerformanceLock(particleSystems))
            {
                int systemIndex;
                if (particleSystemIndex.TryGetValue(entity, out systemIndex))
                    particleSystems[systemIndex].InUse = false;
                particleSystemIndex.Remove(entity);
            }
        }

        /// <summary>
        /// Updates all particle systems in this collection
        /// </summary>
        public void UpdateAll(Vector3i worldSize, Vector3i cameraLocation, LightProvider lightProvider, float timeSinceLastFrame)
        {
            updateList.Clear();
            using (new PerformanceLock(particleSystems))
                updateList.AddRange(particleSystems); // Make copy to not lock during the updating

            if (controller.TransparencyType == TransparencyType.Realistic)
            {
                sorter.CameraLocation = cameraLocation;
                updateList.Sort(sorter);
            }

            int dataIndex = 0;
            for (int i = 0; i < updateList.Count; i++)
            {
                ParticleEmitter system = updateList[i];
                if (system.InUse)
                {
                    system.UpdateInternal(cameraLocation, timeSinceLastFrame);
                    lock (syncObj)
                        system.AddToArrays(worldSize, lightProvider, locations, colors, ref dataIndex);
                }
            }

            totalAliveParticles = dataIndex;
        }

        /// <summary>
        /// Renders all particles in all systems in this collection
        /// </summary>
        public RenderStatistics Render()
        {
            if (instances == null)
                instances = CreateInstanceDataBuffer(out locationData, out colorData);

            if (controller.TransparencyType != TransparencyType.None)
            {
                TiVEController.Backend.DisableDepthWriting();
                if (controller.TransparencyType == TransparencyType.Additive)
                    TiVEController.Backend.SetBlendMode(BlendMode.Additive);
            }

            // Put the data for the current particles into the graphics memory and draw them
            int totalParticles = totalAliveParticles;
            lock (syncObj)
            {
                locationData.UpdateData(locations, totalParticles);
                colorData.UpdateData(colors, totalParticles);
            }
            instances.Bind();
            TiVEController.Backend.Draw(PrimitiveType.Triangles, instances);

            if (controller.TransparencyType != TransparencyType.None)
            {
                TiVEController.Backend.SetBlendMode(BlendMode.Realistic);
                TiVEController.Backend.EnableDepthWriting();
            }

            return new RenderStatistics(1, totalParticles * polysPerParticle,
                totalParticles * voxelsPerParticle, totalParticles * renderedVoxelsPerParticle);
        }
        #endregion

        private IVertexDataCollection CreateInstanceDataBuffer(out IRendererData locData, out IRendererData colData)
        {
            IVertexDataCollection instanceData = TiVEController.Backend.CreateVertexDataCollection();
            instanceData.AddBuffer(voxelInstanceLocationData);
            if (TiVEController.UserSettings.Get(UserSettings.CubifyVoxelsKey))
                instanceData.AddBuffer(voxelInstanceColorData);

            locData = TiVEController.Backend.CreateData(locations, 0, 3, DataType.Instance, DataValueType.UShort, false, true);
            instanceData.AddBuffer(locData);
            colData = TiVEController.Backend.CreateData(colors, 0, 4, DataType.Instance, DataValueType.Byte, true, true);
            instanceData.AddBuffer(colData);
            instanceData.Initialize();

            return instanceData;
        }

        #region ParticleSystemSorter class
        /// <summary>
        /// Helper class for sorting particles by their distance from the camera
        /// </summary>
        private sealed class ParticleSystemSorter : IComparer<ParticleEmitter>
        {
            public Vector3i CameraLocation;

            public int Compare(ParticleEmitter pe1, ParticleEmitter pe2)
            {
                if (pe1 == null && pe2 == null)
                    return 0;

                if (pe1 == null)
                    return 1;

                if (pe2 == null)
                    return -1;

                int p1DistX = pe1.Location.X - CameraLocation.X;
                int p1DistY = pe1.Location.Y - CameraLocation.Y;
                int p1DistZ = pe1.Location.Z - CameraLocation.Z;
                int p1DistSquared = p1DistX * p1DistX + p1DistY * p1DistY + p1DistZ * p1DistZ;

                int p2DistX = pe2.Location.X - CameraLocation.X;
                int p2DistY = pe2.Location.Y - CameraLocation.Y;
                int p2DistZ = pe2.Location.Z - CameraLocation.Z;
                int p2DistSquared = p2DistX * p2DistX + p2DistY * p2DistY + p2DistZ * p2DistZ;
                return p2DistSquared - p1DistSquared;
            }
        }
        #endregion
    }
}
