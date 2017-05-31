using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVE.RenderSystem;
using ProdigalSoftware.TiVE.RenderSystem.World;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;
using ProdigalSoftware.TiVEPluginFramework.Generators;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.ParticleSystem
{
    internal sealed class ParticleSystem : EngineSystem
    {
        private const int UpdatesPerSecond = 60;

        private static readonly ParticleEmitterSorter emitterSorter = new ParticleEmitterSorter();

        /// <summary>List of particles systems in this collection</summary>
        private readonly List<ParticleEmitter> particleEmitters = new List<ParticleEmitter>();
        /// <summary>Copy of the particle systems list used for updating without locking too long</summary>
        private readonly List<ParticleEmitter> updateList = new List<ParticleEmitter>();
        /// <summary>Quick lookup for the index of a particle entity</summary>
        private readonly Dictionary<IEntity, int> particleEmitterIndex = new Dictionary<IEntity, int>();


        private readonly List<ParticleEmitter> renderList = new List<ParticleEmitter>();

        private readonly HashSet<IEntity> emittersToRender = new HashSet<IEntity>();
        private readonly HashSet<IEntity> runningEmitters = new HashSet<IEntity>();
        private readonly List<IEntity> emittersToDelete = new List<IEntity>();

        private readonly IParticleControllerGenerator[] controllerGenerators;
        private readonly ShaderManager shaderManager = new ShaderManager();
        private readonly Thread particleUpdateThread;
        private volatile bool stopThread;
        private Vector3i cameraLocation;

        public ParticleSystem() : base("Particles")
        {
            controllerGenerators = TiVEController.PluginManager.GetPluginsOfType<IParticleControllerGenerator>().ToArray();
            particleUpdateThread = new Thread(ParticleUpdateLoop);
            particleUpdateThread.Priority = ThreadPriority.Normal;
            particleUpdateThread.IsBackground = true;
            particleUpdateThread.Name = "ParticleUpdate";
        }

        public override void Dispose()
        {
            stopThread = true;
            if (particleUpdateThread != null && particleUpdateThread.IsAlive)
                particleUpdateThread.Join();

            foreach (ParticleEmitter emitter in particleEmitters.Where(pe => pe != null))
                emitter.Dispose();
            particleEmitters.Clear();

            renderList.Clear();
            updateList.Clear();
            emittersToRender.Clear();
            emittersToDelete.Clear();
            runningEmitters.Clear();
            particleEmitters.Clear();
            particleEmitterIndex.Clear();
            shaderManager.Dispose();
        }

        public override bool Initialize()
        {
            particleUpdateThread.Start();
            
            if (controllerGenerators == null || controllerGenerators.Length == 0)
                Messages.AddWarning("Could not find particle controller generators.");
            return shaderManager.Initialize();
        }

        public override void ChangeScene(Scene oldScene, Scene newScene)
        {
        }

        protected override bool UpdateInternal(int ticksSinceLastFrame, Scene currentScene)
        {
            CameraComponent cameraData = FindCamera(currentScene);
            if (cameraData == null)
                return true; // Couldn't find a camera to determine view projection matrix

            FindSystemsToRender(cameraData);

            if (currentScene.LoadingInitialChunks)
                return true;
            
            ShaderProgram shader = shaderManager.GetShaderProgram(ShaderProgram.GetShaderName(true));
            shader.Bind();
            shader.SetUniform("matrix_ModelViewProjection", ref cameraData.ViewProjectionMatrix);
            shader.SetUniform("voxelSize", 1);
            shader.SetUniform("lightCount", 0);
            shader.SetUniform("cameraLoc", ref cameraData.Location);

            Vector3f translation = Vector3f.Zero;
            shader.SetUniform("modelTranslation", ref translation);

            renderList.Clear();
            using (new PerformanceLock(particleEmitters))
                renderList.AddRange(particleEmitters.Where(pe => pe != null));

            // Sort by transparency type and distance to the camera
            emitterSorter.CameraLocation = cameraLocation;
            renderList.Sort(emitterSorter);

            RenderStatistics stats = new RenderStatistics();
            TransparencyType lastType = TransparencyType.None;
            for (int i = 0; i < renderList.Count; i++)
            {
                ParticleEmitter emitter = renderList[i];

                // Make sure the correct transparency mode is setup
                if (emitter.Controller.TransparencyType != lastType)
                {
                    if (emitter.Controller.TransparencyType != TransparencyType.None)
                    {
                        TiVEController.Backend.DisableDepthWriting();
                        if (emitter.Controller.TransparencyType == TransparencyType.Additive)
                            TiVEController.Backend.SetBlendMode(BlendMode.Additive);
                        else
                            TiVEController.Backend.SetBlendMode(BlendMode.Realistic);
                    }
                    else
                    {
                        TiVEController.Backend.SetBlendMode(BlendMode.Realistic);
                        TiVEController.Backend.EnableDepthWriting();
                    }
                }

                RenderedLight[] lights = currentScene.LightData.GetLightsInChunk(
                    emitter.Location.X / ChunkComponent.VoxelSize, 
                    emitter.Location.Y / ChunkComponent.VoxelSize, 
                    emitter.Location.Z / ChunkComponent.VoxelSize, 10);
                shader.SetUniform("lightCount", lights.Length);
                shader.SetUniform("lights", lights);

                stats += renderList[i].Render();
                lastType = emitter.Controller.TransparencyType;
            }

            if (lastType != TransparencyType.None)
            {
                TiVEController.Backend.SetBlendMode(BlendMode.Realistic);
                TiVEController.Backend.EnableDepthWriting();
            }

            return true;
        }

        private void FindSystemsToRender(CameraComponent cameraData)
        {
            cameraLocation = new Vector3i((int)cameraData.Location.X, (int)cameraData.Location.Y, (int)cameraData.Location.Z);

            emittersToRender.Clear();
            foreach (IEntity entity in cameraData.VisibleEntitites)
            {
                ParticleComponent particleData = entity.GetComponent<ParticleComponent>();
                if (particleData == null)
                    continue;

                emittersToRender.Add(entity);
                if (!runningEmitters.Contains(entity))
                {
                    runningEmitters.Add(entity);
                    AddEmitter(entity, particleData);
                }
            }

            foreach (IEntity entity in runningEmitters)
            {
                if (!emittersToRender.Contains(entity))
                    emittersToDelete.Add(entity);
            }

            for (int i = 0; i < emittersToDelete.Count; i++)
            {
                IEntity entity = emittersToDelete[i];
                runningEmitters.Remove(entity);
                RemoveEmitter(entity);
            }
            emittersToDelete.Clear();
        }

        /// <summary>
        /// Finds the first enabled camera in the specified scene
        /// </summary>
        private static CameraComponent FindCamera(Scene scene)
        {
            foreach (IEntity cameraEntity in scene.GetEntitiesWithComponent<CameraComponent>())
            {
                CameraComponent cameraData = cameraEntity.GetComponent<CameraComponent>();
                Debug.Assert(cameraData != null);

                if (cameraData.Enabled)
                    return cameraData;
            }
            return null;
        }

        private void AddEmitter(IEntity entity, ParticleComponent particleData)
        {
            string controllerName = particleData.ControllerName;
            using (new PerformanceLock(particleEmitters))
            {
                if (controllerGenerators != null)
                {
                    ParticleController controller = null;
                    for (int i = 0; i < controllerGenerators.Length; i++)
                    {
                        controller = controllerGenerators[i].CreateController(controllerName);
                        if (controller != null)
                            break;
                    }

                    if (controller == null)
                        Messages.AddWarning("Could not find particle controller for " + controllerName);
                    else
                    {
                        ParticleEmitter newEmitter = new ParticleEmitter(controller);
                        newEmitter.Reset(particleData.Location);
                        int indexToAdd = particleEmitters.IndexOf(null);
                        if (indexToAdd >= 0)
                        {
                            particleEmitters[indexToAdd] = newEmitter;
                            particleEmitterIndex[entity] = indexToAdd;
                        }
                        else
                        {
                            particleEmitters.Add(newEmitter);
                            particleEmitterIndex[entity] = particleEmitters.Count - 1;
                        }
                    }
                }
            }
        }

        private void RemoveEmitter(IEntity entity)
        {
            using (new PerformanceLock(particleEmitters))
            {
                if (particleEmitterIndex.TryGetValue(entity, out int emitterIndex))
                {
                    ParticleEmitter emitter = particleEmitters[emitterIndex];
                    particleEmitters[emitterIndex] = null;
                    particleEmitterIndex.Remove(entity);
                    emitter.Dispose();
                }
            }
        }

        private void ParticleUpdateLoop()
        {
            float ticksPerSecond = Stopwatch.Frequency;
            long particleUpdateTime = Stopwatch.Frequency / UpdatesPerSecond;
            long lastTime = 0;
            Stopwatch sw = Stopwatch.StartNew();
            while (!stopThread)
            {
                long newTicks = sw.ElapsedTicks;
                if (newTicks >= lastTime + particleUpdateTime)
                {
                    float timeSinceLastUpdate = (newTicks - lastTime) / ticksPerSecond;
                    if (timeSinceLastUpdate > 0.1f)
                        timeSinceLastUpdate = 0.1f;

                    lastTime = newTicks;

                    UpdateEntities(timeSinceLastUpdate);
                }
                else if (lastTime + particleUpdateTime - TiVEController.MaxTicksForSleep > newTicks)
                    Thread.Sleep(1);
            }
            sw.Stop();
        }

        private void UpdateEntities(float timeSinceLastUpdate)
        {
            updateList.Clear();
            using (new PerformanceLock(particleEmitters))
                updateList.AddRange(particleEmitters.Where(pe => pe != null));

            for (int i = 0; i < updateList.Count; i++)
                updateList[i].UpdateAll(ref cameraLocation, timeSinceLastUpdate);
        }

        #region ParticleEmitterSorter class
        /// <summary>
        /// Helper class for sorting particles by their distance from the camera
        /// </summary>
        private sealed class ParticleEmitterSorter : IComparer<ParticleEmitter>
        {
            public Vector3i CameraLocation;

            public int Compare(ParticleEmitter pe1, ParticleEmitter pe2)
            {
                //if (pe1 == null && pe2 == null)
                //    return 0;

                //if (pe1 == null)
                //    return 1;

                //if (pe2 == null)
                //    return -1;

                int transComp = pe1.TransparencyType.CompareTo(pe2.TransparencyType);
                if (transComp != 0)
                    return transComp;

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
