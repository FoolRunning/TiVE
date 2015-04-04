using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVE.RenderSystem;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;
using ProdigalSoftware.TiVEPluginFramework.Generators;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.ParticleSystem
{
    internal sealed class ParticleSystem : TimeSlicedEngineSystem
    {
        private const int UpdatesPerSecond = 60;

        private readonly List<ParticleEmitterCollection> renderList = new List<ParticleEmitterCollection>();

        private readonly HashSet<IEntity> systemsToRender = new HashSet<IEntity>();
        private readonly HashSet<IEntity> runningSystems = new HashSet<IEntity>();
        private readonly List<IEntity> systemsToDelete = new List<IEntity>();

        private readonly Dictionary<string, ParticleEmitterCollection> particleSystemCollections = new Dictionary<string, ParticleEmitterCollection>();
        private readonly IParticleControllerGenerator controllerGenerator;
        private readonly ShaderManager shaderManager = new ShaderManager();
        private Vector3i cameraLocation;
        private Scene loadedScene;
        private Thread particleUpdateThread;
        private volatile bool stopThread;

        public ParticleSystem() : base("Particles", UpdatesPerSecond)
        {
            controllerGenerator = TiVEController.PluginManager.GetPluginsOfType<IParticleControllerGenerator>().FirstOrDefault();
        }

        public override void Dispose()
        {
            stopThread = true;
            if (particleUpdateThread != null && particleUpdateThread.IsAlive)
                particleUpdateThread.Join();

            foreach (ParticleEmitterCollection systemCollection in particleSystemCollections.Values)
                systemCollection.Dispose();
            particleSystemCollections.Clear();
            shaderManager.Dispose();
            loadedScene = null;
        }

        public override bool Initialize()
        {
            if (TiVEController.UserSettings.Get(UserSettings.UseThreadedParticlesKey))
            {
                particleUpdateThread = new Thread(ParticleUpdateLoop);
                particleUpdateThread.Priority = ThreadPriority.BelowNormal;
                particleUpdateThread.IsBackground = true;
                particleUpdateThread.Name = "ParticleUpdate";
                particleUpdateThread.Start();
            }
            
            if (controllerGenerator == null)
                Messages.AddWarning("Could not find particle controller generator.");
            return shaderManager.Initialize();
        }

        protected override bool UpdateInternal(int ticksSinceLastFrame, Scene currentScene)
        {
            loadedScene = currentScene;

            CameraComponent cameraData = FindCamera(currentScene);
            if (cameraData == null)
                return base.UpdateInternal(ticksSinceLastFrame, currentScene); // Couldn't find a camera to determine view projection matrix

            renderList.Clear();
            using (new PerformanceLock(particleSystemCollections))
                renderList.AddRange(particleSystemCollections.Values);

            RenderStatistics stats = new RenderStatistics();
            // Render opaque particles first
            foreach (ParticleEmitterCollection system in renderList.Where(s => s.TransparencyType == TransparencyType.None))
                stats += system.Render(shaderManager, ref cameraData.ViewProjectionMatrix);

            // Render transparent particles last
            foreach (ParticleEmitterCollection system in renderList.Where(s => s.TransparencyType == TransparencyType.Additive))
                stats += system.Render(shaderManager, ref cameraData.ViewProjectionMatrix);

            foreach (ParticleEmitterCollection system in renderList.Where(s => s.TransparencyType == TransparencyType.Realistic))
                stats += system.Render(shaderManager, ref cameraData.ViewProjectionMatrix);

            return base.UpdateInternal(ticksSinceLastFrame, currentScene);
        }

        protected override bool Update(float timeSinceLastUpdate, Scene currentScene)
        {
            CameraComponent cameraData = FindCamera(currentScene);
            if (cameraData == null)
                return true; // Couldn't find a camera to determine what should be updated

            cameraLocation = new Vector3i((int)cameraData.Location.X, (int)cameraData.Location.Y, (int)cameraData.Location.Z);
            systemsToRender.Clear();
            foreach (IEntity entity in cameraData.VisibleEntitites)
            {
                ParticleComponent particleData = entity.GetComponent<ParticleComponent>();
                if (particleData == null)
                    continue;

                systemsToRender.Add(entity);
                if (!runningSystems.Contains(entity))
                {
                    runningSystems.Add(entity);
                    AddParticleSystem(entity, particleData);
                }
            }

            foreach (IEntity entity in runningSystems)
            {
                if (!systemsToRender.Contains(entity))
                    systemsToDelete.Add(entity);
            }

            for (int i = 0; i < systemsToDelete.Count; i++)
            {
                IEntity entity = systemsToDelete[i];
                ParticleComponent particleData = entity.GetComponent<ParticleComponent>();
                Debug.Assert(particleData != null);

                runningSystems.Remove(entity);
                RemoveParticleSystem(entity, particleData);
            }
            systemsToDelete.Clear();

            return true;
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

        private void AddParticleSystem(IEntity entity, ParticleComponent particleData)
        {
            string controllerName = particleData.ControllerName;
            ParticleEmitterCollection collection;
            using (new PerformanceLock(particleSystemCollections))
            {
                if (!particleSystemCollections.TryGetValue(controllerName, out collection) && controllerGenerator != null)
                {
                    ParticleController controller = controllerGenerator.CreateController(controllerName);
                    if (controller == null)
                        Messages.AddWarning("Could not find particle controller for " + controllerName);
                    else
                        particleSystemCollections[controllerName] = collection = new ParticleEmitterCollection(controller);
                }
            }
            if (collection != null)
                collection.Add(entity);
        }

        private void RemoveParticleSystem(IEntity entity, ParticleComponent particleData)
        {
            ParticleEmitterCollection collection;
            using (new PerformanceLock(particleSystemCollections))
                particleSystemCollections.TryGetValue(particleData.ControllerName, out collection);

            if (collection != null)
                collection.Remove(entity);
        }

        private void ParticleUpdateLoop()
        {
            float ticksPerSecond = Stopwatch.Frequency;
            long particleUpdateTime = Stopwatch.Frequency / UpdatesPerSecond;
            long lastTime = 0;
            List<ParticleEmitterCollection> updateList = new List<ParticleEmitterCollection>();
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

                    updateList.Clear();
                    using (new PerformanceLock(particleSystemCollections))
                        updateList.AddRange(particleSystemCollections.Values);

                    Scene currentScene = loadedScene;
                    if (currentScene != null)
                    {
                        Vector3i worldSize = currentScene.GameWorld != null ? currentScene.GameWorld.VoxelSize : new Vector3i();
                        for (int i = 0; i < updateList.Count; i++)
                            updateList[i].UpdateAll(worldSize, cameraLocation, currentScene.LightProvider, timeSinceLastUpdate);
                    }
                }
                else if (lastTime + particleUpdateTime - TiVEController.MaxTicksForSleep > newTicks)
                    Thread.Sleep(1);
            }
            sw.Stop();
        }
    }
}
