using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.Voxels;
using ProdigalSoftware.TiVE.Resources;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Particles;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.Particles
{
    internal sealed class ParticleSystemCollection : IDisposable
    {
        private readonly List<ParticleSystem> particleSystems = new List<ParticleSystem>();
        /// <summary>Copy of the particle systems list used for updating without locking too long</summary>
        private readonly List<ParticleSystem> updateList = new List<ParticleSystem>();
        private readonly Dictionary<ParticleSystem, int> particleSystemIndex = new Dictionary<ParticleSystem, int>();
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

        public ParticleSystemCollection(ParticleSystemInformation systemInfo)
        {
            this.systemInfo = systemInfo;

            MeshBuilder voxelInstanceBuilder = new MeshBuilder(150, 0);
            VoxelMeshUtils.GenerateMesh(systemInfo.ParticleVoxels, voxelInstanceBuilder,
                out voxelsPerParticle, out renderedVoxelsPerParticle, out polysPerParticle);
            voxelInstanceLocationData = voxelInstanceBuilder.GetLocationData();
            if (!HasTransparency)
                voxelInstanceColorData = voxelInstanceBuilder.GetColorData();

            locations = new Vector3s[systemInfo.MaxParticles * 5];
            colors = new Color4b[systemInfo.MaxParticles * 5];

            for (int sysIndex = 0; sysIndex < 5; sysIndex++)
            {
                particleSystems.Add(null);
                particles.Add(new Particle[systemInfo.MaxParticles]);
                for (int i = 0; i < systemInfo.MaxParticles; i++)
                    particles[sysIndex][i] = new Particle();
            }
        }

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

        public bool HasTransparency
        {
            get { return systemInfo.TransparentParticles; }
        }

        public void Add(ParticleSystem system)
        {
            Debug.Assert(system.SystemInformation == systemInfo);

            using (new PerformanceLock(particleSystems))
            {
                int availableIndex = particleSystems.FindIndex(sys => sys == null);
                if (availableIndex >= 0)
                {
                    particleSystems[availableIndex] = system;
                    particleSystemIndex[system] = availableIndex;
                    ResetSystemParticles(availableIndex);
                }
                else
                {

                    int newIndex = particleSystems.Count;
                    for (int i = 0; i < 10; i++)
                        AddNewSystem();
                    particleSystems[newIndex] = system;
                    particleSystemIndex[system] = newIndex;

                    int origCount = locations.Length;
                    int newCount = origCount + systemInfo.MaxParticles * 10;
                    Array.Resize(ref locations, newCount);
                    Array.Resize(ref colors, newCount);
                }
            }
        }

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

        public RenderStatistics Render(ref Matrix4 matrixMVP)
        {
            if (instances == null)
            {
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

            locationData.UpdateData(locations, totalAliveParticles);
            colorData.UpdateData(colors, totalAliveParticles);

            IShaderProgram shader = ResourceManager.ShaderManager.GetShaderProgram(HasTransparency ? "TransparentParticles" : "SolidParticles");
            shader.Bind();
            shader.SetUniform("matrix_ModelViewProjection", ref matrixMVP);

            if (systemInfo.TransparentParticles)
            {
                TiVEController.Backend.DisableDepthWriting();
                TiVEController.Backend.SetBlendMode(BlendMode.Additive);
            }

            instances.Bind();
            TiVEController.Backend.Draw(PrimitiveType.Triangles, instances);

            if (systemInfo.TransparentParticles)
            {
                TiVEController.Backend.SetBlendMode(BlendMode.None);
                TiVEController.Backend.EnableDepthWriting();
            }

            return new RenderStatistics(1, totalAliveParticles * polysPerParticle, 
                totalAliveParticles * voxelsPerParticle, totalAliveParticles * renderedVoxelsPerParticle);
        }

        private void ResetSystemParticles(int index)
        {
            Particle[] particleList = particles[index];
            for (int i = 0; i < particleList.Length; i++)
                particleList[i].Time = 0.0f;
        }

        private void AddNewSystem()
        {
            particleSystems.Add(null);

            Particle[] newParticles = new Particle[systemInfo.MaxParticles];
            for (int i = 0; i < newParticles.Length; i++)
                newParticles[i] = new Particle();
            particles.Add(newParticles);
        }
    }
}
