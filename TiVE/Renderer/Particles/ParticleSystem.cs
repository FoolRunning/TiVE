using System;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.Voxels;
using ProdigalSoftware.TiVE.Resources;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.Particles
{
    internal sealed class ParticleSystem : IParticleSystem, IDisposable
    {
        private readonly int polysPerVoxel;
        private readonly IRendererData voxelInstanceLocationData;

        private readonly ParticleController controller;
        private readonly Particle[] particles;
        private readonly Color4b[] colors;
        private readonly Vector3s[] locations;
        private int aliveParticleCount;
        private float numOfParticlesNeeded;
        private IVertexDataCollection instances;
        private IRendererData locationData;
        private IRendererData colorData;

        public ParticleSystem(uint[,,] particleVoxels, ParticleController controller, Vector3b location, int particlesPerSecond, int maxParticles)
        {
            this.controller = controller;
            Location = new Vector3(location.X, location.Y, location.Z);
            ParticlesPerSecond = particlesPerSecond;
            colors = new Color4b[maxParticles];
            locations = new Vector3s[maxParticles];
            particles = new Particle[maxParticles];
            for (int i = 0; i < maxParticles; i++)
                particles[i] = new Particle();

            MeshBuilder voxelInstanceBuilder = new MeshBuilder(150, 0);
            int dummy;
            VoxelMeshUtils.GenerateMesh(particleVoxels, voxelInstanceBuilder, out dummy, out dummy, out polysPerVoxel);
            voxelInstanceLocationData = voxelInstanceBuilder.GetLocationData();
        }

        public void Dispose()
        {
            if (instances != null)
                instances.Dispose();

            instances = null;
        }

        public int PolygonCount
        {
            get { return aliveParticleCount * polysPerVoxel; }
        }

        public int VoxelCount
        {
            get { return aliveParticleCount; }
        }

        public int RenderedVoxelCount
        {
            get { return aliveParticleCount; }
        }

        public Vector3 Location { get; set; }

        public int ParticlesPerSecond { get; set; }

        public void Update(float timeSinceLastFrame)
        {
            ParticleController upd = controller;
            upd.BeginUpdate(this, timeSinceLastFrame);

            Vector3s[] locationArray = locations;
            Color4b[] colorArray = colors;
            int aliveParticles = aliveParticleCount;
            Particle[] particleList = particles;
            float locX = Location.X;
            float locY = Location.Y;
            float locZ = Location.Z;
            for (int index = 0; index < aliveParticles; )
            {
                Particle part = particleList[index];
                upd.Update(part, timeSinceLastFrame, locX, locY, locZ);
                if (part.Time > 0.0f)
                {
                    locationArray[index] = new Vector3s((short)part.X, (short)part.Y, (short)part.Z);
                    colorArray[index] = part.Color;
                    index++;
                }
                else
                {
                    // Particle died replace with an alive particle
                    int lastAliveIndex = aliveParticles - 1;
                    Particle lastAlive = particleList[lastAliveIndex];
                    particleList[lastAliveIndex] = particleList[index];
                    particleList[index] = lastAlive;
                    aliveParticles--;
                }
            }

            numOfParticlesNeeded += ParticlesPerSecond * timeSinceLastFrame;
            int newParticleCount = Math.Min((int)numOfParticlesNeeded, particleList.Length - aliveParticles);
            numOfParticlesNeeded -= newParticleCount;

            for (int i = 0; i < newParticleCount; i++)
            {
                Particle part = particleList[aliveParticles];
                upd.InitializeNew(part, locX, locY, locZ);
                locationArray[aliveParticles] = new Vector3s((short)part.X, (short)part.Y, (short)part.Z);
                colorArray[aliveParticles] = part.Color;
                aliveParticles++;
            }

            aliveParticleCount = aliveParticles;
        }

        public void Render(ref Matrix4 matrixMVP)
        {
            IShaderProgram shader = ResourceManager.ShaderManager.GetShaderProgram("Particles");
            shader.Bind();
            shader.SetUniform("matrix_ModelViewProjection", ref matrixMVP);

            if (instances == null)
            {
                instances = TiVEController.Backend.CreateVertexDataCollection();
                instances.AddBuffer(voxelInstanceLocationData);

                locationData = TiVEController.Backend.CreateData(locations, locations.Length, 3, DataType.Instance, ValueType.Short, false, true);
                instances.AddBuffer(locationData);

                colorData = TiVEController.Backend.CreateData(colors, colors.Length, 4, DataType.Instance, ValueType.Byte, true, true);
                instances.AddBuffer(colorData);

                instances.Initialize();
            }
            else
            {
                locationData.UpdateData(locations, aliveParticleCount);
                colorData.UpdateData(colors, aliveParticleCount);
            }

            instances.Bind();

            TiVEController.Backend.Draw(PrimitiveType.Triangles, instances);
        }
    }
}
