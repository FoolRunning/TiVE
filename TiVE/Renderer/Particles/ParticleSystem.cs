using System;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.Voxels;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.Particles
{
    internal sealed class ParticleSystem : IParticleSystem, IDisposable
    {
        #region Constants
        private const string VertexShaderSource = @"
            #version 150 core 
 
            // premultiplied model to projection transformation
            uniform mat4 matrix_ModelViewProjection;
 
            // incoming vertex information
            in vec3 in_Position;
            in vec4 in_Color;

            // incoming vertex information for each instance
            in vec3 in_InstancePos;
            in vec4 in_InstanceColor;

            flat out vec4 fragment_color;
 
            void main(void)
            {
                fragment_color = in_Color * in_InstanceColor;

                // transforming the incoming vertex position
                gl_Position = matrix_ModelViewProjection * vec4(in_Position + in_InstancePos, 1);
            }";

        private const string FragmentShaderSource = @"
                #version 150 core

                flat in vec4 fragment_color;

                out vec4 color;

				void main(void)
				{
					color = fragment_color;
				}
			";
        #endregion

        private static readonly int polysPerVoxel;
        private static IShaderProgram shader;
        private static IRendererData voxelInstanceLocationData;
        private static IRendererData voxelInstanceColorData;

        private readonly ParticleController controller;
        private readonly Particle[] particles;
        private readonly Color4b[] colors;
        private readonly Vector3s[] locations;
        private int aliveParticleCount;
        private float numOfParticlesNeeded;
        private IVertexDataCollection instances;
        private IRendererData locationData;
        private IRendererData colorData;

        static ParticleSystem()
        {
            MeshBuilder voxelInstanceBuilder = new MeshBuilder(10, 0);
            polysPerVoxel = SimpleVoxelGroup.CreateVoxel(voxelInstanceBuilder, VoxelSides.All, 0, 0, 0, 235, 235, 235, 255);

            voxelInstanceLocationData = voxelInstanceBuilder.GetLocationData();
            voxelInstanceLocationData.Lock();
            voxelInstanceColorData = voxelInstanceBuilder.GetColorData();
            voxelInstanceColorData.Lock();
        }

        public ParticleSystem(ParticleController controller, Vector3b location, int particlesPerSecond, int maxParticles)
        {
            this.controller = controller;
            Location = new Vector3(location.X, location.Y, location.Z);
            ParticlesPerSecond = particlesPerSecond;
            colors = new Color4b[maxParticles];
            locations = new Vector3s[maxParticles];
            particles = new Particle[maxParticles];
            for (int i = 0; i < maxParticles; i++)
                particles[i] = new Particle();
        }

        public void Dispose()
        {
            //if (shader != null)
            //    shader.Delete();

            if (instances != null)
                instances.Dispose();

            //shader = null;
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
            if (shader == null)
                shader = CreateShader();

            shader.Bind();
            shader.SetUniform("matrix_ModelViewProjection", ref matrixMVP);

            if (instances == null)
            {
                instances = TiVEController.Backend.CreateVertexDataCollection();
                instances.AddBuffer(voxelInstanceLocationData);
                instances.AddBuffer(voxelInstanceColorData);

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
            
            //int aliveParticles = aliveParticleCount;
            //Vector3s[] locationArray = locations;
            //Color4b[] colorArray = colors;
            //if (aliveParticles > 0)
            //{
            //    //unsafe
            //    //{
            //    //    IntPtr locationMemoryBlock = locationData.MapData(aliveParticleCount);
            //    //    IntPtr colorMemoryBlock = colorData.MapData(aliveParticleCount);
            //    //    //Vector3s* locationDataBlock = (Vector3s*)memoryBlock.ToPointer();
            //    //    //for (int i = 0; i < aliveParticles; i++)
            //    //    //{
            //    //    //    Particle part = particles[i];
            //    //    //    locationDataBlock[i] = new Vector3s((short)part.X, (short)part.Y, (short)part.Z);
            //    //    //}

            //    //    //Color4b* colorDataBlock = (Color4b*)memoryBlock.ToPointer();
            //    //    //for (int i = 0; i < aliveParticles; i++)
            //    //    //    colorDataBlock[i] = particles[i].Color;

            //    //    Vector3s* locationDataBlock = (Vector3s*)locationMemoryBlock.ToPointer();
            //    //    Color4b* colorDataBlock = (Color4b*)colorMemoryBlock.ToPointer();
            //    //    for (int i = 0; i < aliveParticles; i++)
            //    //    {
            //    //        Particle part = particles[i];
            //    //        *locationDataBlock++ = new Vector3s((short)part.X, (short)part.Y, (short)part.Z);
            //    //        *colorDataBlock++ = part.Color;
            //    //    }
            //    //    locationData.UnmapData();
            //    //    colorData.UnmapData();
            //        //fixed (Vector3s* locationPtr = &locations[0])
            //        //fixed (Color4b* colorPtr = &colors[0])
            //        //{
            //        //    for (int i = 0; i < aliveParticles; i++)
            //        //    {
            //        //        Particle part = particles[i];
            //        //        locationPtr[i] = new Vector3s((short)part.X, (short)part.Y, (short)part.Z);
            //        //        colorPtr[i] = part.Color;
            //        //    }
            //        //}

            //    //}
            //}

            TiVEController.Backend.Draw(PrimitiveType.Triangles, instances);
        }

        private static IShaderProgram CreateShader()
        {
            IShaderProgram program = TiVEController.Backend.CreateShaderProgram();
            program.AddShader(VertexShaderSource, ShaderType.Vertex);
            program.AddShader(FragmentShaderSource, ShaderType.Fragment);
            program.AddAttribute("in_Position");
            program.AddAttribute("in_Color");
            program.AddAttribute("in_InstancePos");
            program.AddAttribute("in_InstanceColor");
            program.AddKnownUniform("matrix_ModelViewProjection");

            if (!program.Initialize())
                Messages.AddWarning("Failed to initialize shader program for particle system");
            return program;
        }
    }
}
