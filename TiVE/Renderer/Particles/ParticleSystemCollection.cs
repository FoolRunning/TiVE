using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.Voxels;
using ProdigalSoftware.TiVE.Resources;
using ProdigalSoftware.TiVEPluginFramework.Particles;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.Particles
{
    /// <summary>
    /// Holds all the running particle systems for a system type (i.e. all "fountains" are in a collection, all "fires" are in a collection, etc.)
    /// </summary>
    internal sealed class ParticleSystemCollection : IDisposable
    {
        #region Member variables
        /// <summary>List of particles systems in this collection</summary>
        private readonly List<ParticleSystem> particleSystems = new List<ParticleSystem>();
        /// <summary>Copy of the particle systems list used for updating without locking too long</summary>
        private readonly List<ParticleSystem> updateList = new List<ParticleSystem>();
        /// <summary>Quick lookup for the index of a system</summary>
        private readonly Dictionary<ParticleSystem, int> particleSystemIndex = new Dictionary<ParticleSystem, int>();
        /// <summary>Holds the particles for each system</summary>
        private readonly List<Particle[]> particles = new List<Particle[]>();

        private readonly ParticleSystemInformation systemInfo;

        private readonly IRendererData voxelInstanceLocationData;
        private readonly IRendererData voxelInstanceColorData;
        private readonly int polysPerParticle;
        private readonly int voxelsPerParticle;
        private readonly int renderedVoxelsPerParticle;

        private Vector3s[] locations;
        private Color4b[] colors;
        
        private int totalAliveParticles;

        private IVertexDataCollection instances;
        private IRendererData locationData;
        private IRendererData colorData;
        #endregion

        #region Constructor/Dispose
        /// <summary>
        /// Creates a new ParticleSystemCollection to hold particle systems of the specified type
        /// </summary>
        public ParticleSystemCollection(ParticleSystemInformation systemInfo)
        {
            this.systemInfo = systemInfo;

            // Create particle voxel model to be used for each particle
            MeshBuilder voxelInstanceBuilder = new MeshBuilder(150, 0);
            VoxelMeshUtils.GenerateMesh(systemInfo.ParticleVoxels, voxelInstanceBuilder,
                out voxelsPerParticle, out renderedVoxelsPerParticle, out polysPerParticle);
            voxelInstanceLocationData = voxelInstanceBuilder.GetLocationData();
            if (!HasTransparency)
                voxelInstanceColorData = voxelInstanceBuilder.GetColorData();

            locations = new Vector3s[systemInfo.MaxParticles * 5];
            colors = new Color4b[systemInfo.MaxParticles * 5];

            // Initialize room for 5 particle systems in the collection
            for (int i = 0; i < 5; i++)
                AddNewBlankSystem();
        }

        /// <summary>
        /// Cleans up data used by all the particles systems in this collection
        /// </summary>
        public void Dispose()
        {
            voxelInstanceLocationData.Dispose();

            if (voxelInstanceColorData != null)
                voxelInstanceColorData.Dispose();

            if (instances != null)
                instances.Dispose();

            particleSystems.Clear();
            particleSystemIndex.Clear();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets whether the particles in this collection contain transparency
        /// </summary>
        public bool HasTransparency
        {
            get { return systemInfo.TransparentParticles; }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Adds the specified particle system to this collection
        /// </summary>
        public void Add(ParticleSystem system)
        {
            Debug.Assert(system.SystemInformation == systemInfo);

            using (new PerformanceLock(particleSystems))
            {
                int availableIndex = particleSystems.FindIndex(sys => sys == null);
                if (availableIndex >= 0)
                {
                    // Found a free space to use
                    particleSystems[availableIndex] = system;
                    particleSystemIndex[system] = availableIndex;
                    ResetSystemParticles(availableIndex);
                }
                else
                {
                    // No free space, make room for 10 more particle systems and add the new system in the first new spot
                    int newIndex = particleSystems.Count;
                    for (int i = 0; i < 10; i++)
                        AddNewBlankSystem();
                    particleSystems[newIndex] = system;
                    particleSystemIndex[system] = newIndex;

                    int origCount = locations.Length;
                    int newCount = origCount + systemInfo.MaxParticles * 10;
                    Array.Resize(ref locations, newCount);
                    Array.Resize(ref colors, newCount);
                }
            }
        }

        /// <summary>
        /// Removes the specified particle system from this collection
        /// </summary>
        public void Remove(ParticleSystem system)
        {
            Debug.Assert(system.SystemInformation == systemInfo);

            using (new PerformanceLock(particleSystems))
            {
                int systemIndex;
                if (particleSystemIndex.TryGetValue(system, out systemIndex))
                {
                    particleSystems[systemIndex] = null;
                    particleSystemIndex.Remove(system);
                }
            }
        }

        /// <summary>
        /// Updates all particle systems in this collection
        /// </summary>
        /// <param name="timeSinceLastFrame">The time (in seconds) since the last call to update</param>
        public void UpdateAll(float timeSinceLastFrame)
        {
            updateList.Clear();
            using (new PerformanceLock(particleSystems))
                updateList.AddRange(particleSystems); // Make copy to not lock during the updating

            int dataIndex = 0;
            for (int i = 0; i < updateList.Count; i++)
            {
                ParticleSystem system = updateList[i];
                if (system != null)
                    system.Update(timeSinceLastFrame, particles[i], locations, colors, ref dataIndex);
            }

            totalAliveParticles = dataIndex;
        }

        /// <summary>
        /// Renders all particles in all systems in this collection
        /// </summary>
        public RenderStatistics Render(ref Matrix4 matrixMVP)
        {
            if (instances == null)
            {
                // Initialize the data for use in the renderer
                instances = TiVEController.Backend.CreateVertexDataCollection();
                instances.AddBuffer(voxelInstanceLocationData);
                if (!HasTransparency)
                    instances.AddBuffer(voxelInstanceColorData);
                locationData = TiVEController.Backend.CreateData(locations, 0, 3, DataType.Instance, ValueType.Short, false, true);
                instances.AddBuffer(locationData);
                colorData = TiVEController.Backend.CreateData(colors, 0, 4, DataType.Instance, ValueType.Byte, true, true);
                instances.AddBuffer(colorData);
                instances.Initialize();
            }

            IShaderProgram shader = ResourceManager.ShaderManager.GetShaderProgram(HasTransparency ? "TransparentParticles" : "SolidParticles");
            shader.Bind();
            shader.SetUniform("matrix_ModelViewProjection", ref matrixMVP);

            if (systemInfo.TransparentParticles)
            {
                TiVEController.Backend.DisableDepthWriting();
                TiVEController.Backend.SetBlendMode(BlendMode.Additive);
            }

            // Put the data for the current particles into the graphics memory and draw them
            int totalParticles = totalAliveParticles;
            locationData.UpdateData(locations, totalParticles);
            colorData.UpdateData(colors, totalParticles);
            instances.Bind();
            TiVEController.Backend.Draw(PrimitiveType.Triangles, instances);

            if (systemInfo.TransparentParticles)
            {
                TiVEController.Backend.SetBlendMode(BlendMode.None);
                TiVEController.Backend.EnableDepthWriting();
            }

            return new RenderStatistics(1, totalParticles * polysPerParticle,
                totalParticles * voxelsPerParticle, totalParticles * renderedVoxelsPerParticle);
        }
        #endregion

        #region Private helper methods
        /// <summary>
        /// Resets the time for the particles in the particle system at the specified index
        /// </summary>
        private void ResetSystemParticles(int index)
        {
            Particle[] particleList = particles[index];
            for (int i = 0; i < particleList.Length; i++)
                particleList[i].Time = 0.0f;
        }

        /// <summary>
        /// Creates room for another particle system in this collection
        /// </summary>
        private void AddNewBlankSystem()
        {
            particleSystems.Add(null);

            Particle[] newParticles = new Particle[systemInfo.MaxParticles];
            for (int i = 0; i < newParticles.Length; i++)
                newParticles[i] = new Particle();
            particles.Add(newParticles);
        }
        #endregion
    }
}
