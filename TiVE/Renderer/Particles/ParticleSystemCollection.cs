using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.Meshes;
using ProdigalSoftware.TiVE.Renderer.Voxels;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.Particles
{
    /// <summary>
    /// Holds all the running particle systems for a system type (i.e. all "fountains" are in a collection, all "fires" are in a collection, etc.)
    /// </summary>
    internal sealed class ParticleSystemCollection : IDisposable
    {
        #region Member variables
        private static readonly ParticleSystemSorter sorter = new ParticleSystemSorter();
        /// <summary>List of particles systems in this collection</summary>
        private readonly List<ParticleSystem> particleSystems = new List<ParticleSystem>();
        /// <summary>Copy of the particle systems list used for updating without locking too long</summary>
        private readonly List<ParticleSystem> updateList = new List<ParticleSystem>();
        /// <summary>Quick lookup for the index of a system</summary>
        private readonly Dictionary<RunningParticleSystem, int> particleSystemIndex = new Dictionary<RunningParticleSystem, int>();

        private readonly ParticleSystemComponent systemInfo;

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
        /// Creates a new ParticleSystemCollection to hold particle systems of the specified type
        /// </summary>
        public ParticleSystemCollection(ParticleSystemComponent systemInfo)
        {
            this.systemInfo = systemInfo;

            // Create particle voxel model to be used for each particle
            MeshBuilder voxelInstanceBuilder = new MeshBuilder(1000, 0);
            VoxelMeshUtils.GenerateMesh(systemInfo.ParticleVoxels, voxelInstanceBuilder, true,
                out voxelsPerParticle, out renderedVoxelsPerParticle, out polysPerParticle);

            voxelInstanceLocationData = voxelInstanceBuilder.GetLocationData();
            if (TiVEController.UserSettings.Get(UserSettings.ShadedVoxelsKey))
                voxelInstanceColorData = voxelInstanceBuilder.GetColorData();

            locations = new Vector3us[systemInfo.MaxParticles * 5];
            colors = new Color4b[systemInfo.MaxParticles * 5];

            // Initialize room for 5 particle systems in the collection
            for (int i = 0; i < 5; i++)
                particleSystems.Add(new ParticleSystem(systemInfo));
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
            get { return systemInfo.TransparencyType; }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Adds the specified particle system to this collection
        /// </summary>
        public void Add(RunningParticleSystem system)
        {
            Debug.Assert(system.SystemInfo == systemInfo);

            using (new PerformanceLock(particleSystems))
            {
                int availableIndex = particleSystems.FindIndex(sys => !sys.IsAlive);
                if (availableIndex < 0)
                {
                    // No free space, make room for 10 more particle systems and add the new system in the first new spot
                    availableIndex = particleSystems.Count;
                    for (int i = 0; i < 10; i++)
                        particleSystems.Add(new ParticleSystem(systemInfo));

                    int origCount = locations.Length;
                    int newCount = origCount + systemInfo.MaxParticles * 10;
                    lock (syncObj)
                    {
                        Array.Resize(ref locations, newCount);
                        Array.Resize(ref colors, newCount);
                    }
                }

                particleSystems[availableIndex].Reset(system.WorldLocation);
                particleSystems[availableIndex].IsAlive = true;
                particleSystemIndex[system] = availableIndex;
            }
        }

        /// <summary>
        /// Removes the specified particle system from this collection
        /// </summary>
        public void Remove(RunningParticleSystem system)
        {
            Debug.Assert(system.SystemInfo == systemInfo);

            using (new PerformanceLock(particleSystems))
            {
                int systemIndex;
                if (particleSystemIndex.TryGetValue(system, out systemIndex))
                    particleSystems[systemIndex].IsAlive = false;
                particleSystemIndex.Remove(system);
            }
        }

        /// <summary>
        /// Updates all particle systems in this collection
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="timeSinceLastFrame">The time (in seconds) since the last call to update</param>
        public void UpdateAll(IGameWorldRenderer renderer, float timeSinceLastFrame)
        {
            updateList.Clear();
            using (new PerformanceLock(particleSystems))
                updateList.AddRange(particleSystems); // Make copy to not lock during the updating

            if (systemInfo.TransparencyType == TransparencyType.Realistic)
            {
                sorter.CameraLocation = new Vector3i((int)renderer.Camera.Location.X, (int)renderer.Camera.Location.Y, (int)renderer.Camera.Location.Z);
                updateList.Sort(sorter);
            }

            int dataIndex = 0;
            for (int i = 0; i < updateList.Count; i++)
            {
                ParticleSystem system = updateList[i];
                if (system.IsAlive)
                {
                    //lock (syncObj)
                        system.Update(timeSinceLastFrame, locations, colors, renderer, ref dataIndex);
                }
            }

            totalAliveParticles = dataIndex;
        }

        /// <summary>
        /// Renders all particles in all systems in this collection
        /// </summary>
        public RenderStatistics Render(ShaderManager shaderManager, ref Matrix4 matrixMVP)
        {
            if (instances == null)
            {
                // Initialize the data for use in the renderer
                instances = TiVEController.Backend.CreateVertexDataCollection();
                instances.AddBuffer(voxelInstanceLocationData);
                if (TiVEController.UserSettings.Get(UserSettings.ShadedVoxelsKey))
                    instances.AddBuffer(voxelInstanceColorData);

                locationData = TiVEController.Backend.CreateData(locations, 0, 3, DataType.Instance, ValueType.UShort, false, true);
                instances.AddBuffer(locationData);
                colorData = TiVEController.Backend.CreateData(colors, 0, 4, DataType.Instance, ValueType.Byte, true, true);
                instances.AddBuffer(colorData);
                instances.Initialize();
            }

            IShaderProgram shader = shaderManager.GetShaderProgram(VoxelMeshHelper.Get(true).ShaderName);
            shader.Bind();
            shader.SetUniform("matrix_ModelViewProjection", ref matrixMVP);

            if (systemInfo.TransparencyType != TransparencyType.None)
            {
                TiVEController.Backend.DisableDepthWriting();
                if (systemInfo.TransparencyType == TransparencyType.Additive)
                    TiVEController.Backend.SetBlendMode(BlendMode.Additive);
            }

            // Put the data for the current particles into the graphics memory and draw them
            int totalParticles = totalAliveParticles;
            //lock (syncObj)
            {
                locationData.UpdateData(locations, totalParticles);
                colorData.UpdateData(colors, totalParticles);
            }
            instances.Bind();
            TiVEController.Backend.Draw(PrimitiveType.Triangles, instances);

            if (systemInfo.TransparencyType != TransparencyType.None)
            {
                TiVEController.Backend.SetBlendMode(BlendMode.Realistic);
                TiVEController.Backend.EnableDepthWriting();
            }

            return new RenderStatistics(1, totalParticles * polysPerParticle,
                totalParticles * voxelsPerParticle, totalParticles * renderedVoxelsPerParticle);
        }
        #endregion

        #region ParticleSystemSorter class
        /// <summary>
        /// Helper class for sorting particles by their distance from the camera
        /// </summary>
        private sealed class ParticleSystemSorter : IComparer<ParticleSystem>
        {
            public Vector3i CameraLocation;

            public int Compare(ParticleSystem ps1, ParticleSystem ps2)
            {
                if (ps1 == null && ps2 == null)
                    return 0;

                if (ps1 == null)
                    return 1;

                if (ps2 == null)
                    return -1;

                int p1DistX = ps1.Location.X - CameraLocation.X;
                int p1DistY = ps1.Location.Y - CameraLocation.Y;
                int p1DistZ = ps1.Location.Z - CameraLocation.Z;
                int p1DistSquared = p1DistX * p1DistX + p1DistY * p1DistY + p1DistZ * p1DistZ;

                int p2DistX = ps2.Location.X - CameraLocation.X;
                int p2DistY = ps2.Location.Y - CameraLocation.Y;
                int p2DistZ = ps2.Location.Z - CameraLocation.Z;
                int p2DistSquared = p2DistX * p2DistX + p2DistY * p2DistY + p2DistZ * p2DistZ;
                return p2DistSquared - p1DistSquared;
            }
        }
        #endregion
    }
}
