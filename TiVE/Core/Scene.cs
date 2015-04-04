using System;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using ProdigalSoftware.TiVE.RenderSystem;
using ProdigalSoftware.TiVE.RenderSystem.Lighting;
using ProdigalSoftware.TiVE.RenderSystem.World;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Core
{
    internal sealed class Scene : IScene, IDisposable
    {
        private const int InitialEntityListSize = 10;
        private const int InitialChunkListSize = 20000;

        private readonly Dictionary<Type, List<IEntity>> entityComponentTypeMap = new Dictionary<Type, List<IEntity>>(30);
        private readonly List<IEntity> entities = new List<IEntity>(3000);

        public IEnumerable<IEntity> AllEntities
        {
            get { return entities; }
        }

        public BlockList BlockList { get; private set; }

        public LightProvider LightProvider { get; private set; }

        public GameWorld GameWorld { get; private set; }

        public RenderNode RenderNode { get; private set; }

        #region Implementation of IScene
        public void Dispose()
        {
            if (RenderNode != null)
                RenderNode.Dispose();
        }

        public IEntity CreateNewEntity(string entityName)
        {
            Entity newEntity = new Entity(this, entityName);
            entities.Add(newEntity);
            return newEntity;
        }

        public IEnumerable<IEntity> GetEntitiesWithComponent<T>() where T : class, IComponent
        {
            List<IEntity> entitiesWithType;
            if (entityComponentTypeMap.TryGetValue(typeof(T), out entitiesWithType))
                return entitiesWithType;
            return Enumerable.Empty<IEntity>();
        }
        
        public void DeleteEntity(IEntity entity)
        {
            entities.Remove(entity);
            foreach (List<IEntity> entitiesWithType in entityComponentTypeMap.Values)
                entitiesWithType.Remove(entity);
            ((Entity)entity).ClearComponents();
        }

        public void SetGameWorld(string worldName)
        {
            BlockList newBlockList;
            GameWorld newGameWorld = GameWorldManager.LoadGameWorld(worldName, out newBlockList);
            SetGameWorld(newGameWorld, newBlockList);

            // This seems to be needed for the GC to realize that the light information and the game world are long-lived
            // to keep it from causing pauses shortly after starting the render loop.
            for (int i = 0; i < 3; i++)
                GC.Collect();
        }
        #endregion

        #region Public methods
        public void SetGameWorld(GameWorld newGameWorld, BlockList newBlockList)
        {
            if (newGameWorld == null)
                throw new ArgumentNullException("newGameWorld");
            if (newBlockList == null)
                throw new ArgumentNullException("newBlockList");

            GameWorld = newGameWorld;
            BlockList = newBlockList;
            GameWorld.Initialize(BlockList);
            RenderNode = new RenderNode(GameWorld, this);

            // Calculate static lighting
            LightProvider = LightProvider.Get(GameWorld);
            LightProvider.Calculate(BlockList, false);
        }
        #endregion

        #region Private helper methods
        private void AddEntityToTypeMap(IEntity entity, IComponent component)
        {
            Type componentType = component.GetType();
            List<IEntity> entitiesWithType;
            if (!entityComponentTypeMap.TryGetValue(componentType, out entitiesWithType))
            {
                int initialSize = component is ChunkComponent ? InitialChunkListSize : InitialEntityListSize;
                entityComponentTypeMap[componentType] = entitiesWithType = new List<IEntity>(initialSize);
            }

            entitiesWithType.Add(entity);
        }
        #endregion

        #region Entity class
        [MoonSharpUserData]
        private sealed class Entity : IEntity
        {
            private readonly string name;
            private readonly Scene owningScene;
            private IComponent[] components;

            public Entity(Scene owningScene, string name)
            {
                this.owningScene = owningScene;
                this.name = name;
            }

            #region Implementation of IEntity
            public string Name
            {
                get { return name; }
            }

            public IEnumerable<IComponent> Components
            {
                get { return components; }
            }

            public void AddComponent(IComponent component)
            {
                if (component == null)
                    throw new ArgumentNullException("component");

                if (components == null)
                    components = new IComponent[1];
                else
                    Array.Resize(ref components, components.Length + 1);

                components[components.Length - 1] = component;
                owningScene.AddEntityToTypeMap(this, component);
            }

            public T GetComponent<T>() where T : class, IComponent
            {
                for (int i = 0; i < components.Length; i++)
                {
                    T component = components[i] as T;
                    if (component != null)
                        return component;
                }
                return null;
            }

            public IComponent GetComponent(string componentName)
            {
                for (int i = 0; i < components.Length; i++)
                {
                    IComponent component = components[i];
                    if (component.GetType().Name == componentName)
                        return component;
                }
                return null;
            }
            #endregion

            public void ClearComponents()
            {
                components = null;
            }

            public override string ToString()
            {
                int componentCount = components != null ? components.Length : 0;
                return name + " (" + componentCount + " components)";
            }
        }
        #endregion
    }
}
