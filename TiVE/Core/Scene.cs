using System;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using ProdigalSoftware.TiVE.RenderSystem;
using ProdigalSoftware.TiVE.RenderSystem.Lighting;
using ProdigalSoftware.TiVE.RenderSystem.World;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;
using ProdigalSoftware.Utils;
//#define DEBUG_NODES

namespace ProdigalSoftware.TiVE.Core
{
    internal sealed class Scene : IScene, IDisposable
    {
        private const int InitialEntityListSize = 10;
        private const int InitialChunkListSize = 20000;

        private readonly Dictionary<Type, List<IEntity>> entityComponentTypeMap = new Dictionary<Type, List<IEntity>>(30);
        private readonly List<IEntity> entities = new List<IEntity>(3000);

        public bool LoadingInitialChunks { get; set; }

        public BlockList BlockList { get; private set; }

        public LightProvider LightProvider { get; private set; }

        public GameWorldLightData LightData { get; private set; }

        public GameWorld GameWorld { get; private set; }

        public RootRenderNode RenderNode { get; private set; }

        #region Implementation of IScene
        public void Dispose()
        {
#if DEBUG_NODES
            if (RenderNode != null)
                RenderNode.Dispose();
#endif

            if (BlockList != null)
                BlockList.Dispose();
        }

        public IEnumerable<IEntity> AllEntities
        {
            get { return entities; }
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
            CreateEntitiesForBlockComponents(newGameWorld, newBlockList);

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
            RenderNode = new RootRenderNode(GameWorld, this);
            LightProvider = LightProvider.Get(this);

            // Calculate static lighting
            LightData = new GameWorldLightData(this);
            LightData.Calculate();
        }
        #endregion

        #region Private helper methods
        private void CreateEntitiesForBlockComponents(GameWorld gameWorld, BlockList blockList)
        {
            for (int z = 0; z < gameWorld.BlockSize.Z; z++)
            {
                for (int x = 0; x < gameWorld.BlockSize.X; x++)
                {
                    for (int y = 0; y < gameWorld.BlockSize.Y; y++)
                    {
                        BlockImpl block = (BlockImpl)blockList[gameWorld[x, y, z]];
                        ParticleComponent particleData = block.GetComponent<ParticleComponent>();
                        if (particleData != null)
                        {
                            IEntity entity = CreateNewEntity(string.Format("BlockParticles({0}, {1}, {2})", x, y, z));
                            entity.AddComponent(new ParticleComponent(particleData.ControllerName,
                                new Vector3i(x * Block.VoxelSize + particleData.Location.X,
                                    y * Block.VoxelSize + particleData.Location.Y,
                                    z * Block.VoxelSize + particleData.Location.Z)));
                        }
                    }
                }
            }
        }

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

            if (component is SpriteComponent)
                AddEntityToRenderNode(entity, ((SpriteComponent)component).BoundingBox, RenderNode);
            else if (component is ParticleComponent)
            {
                Vector3i loc = ((ParticleComponent)component).Location;
                AddEntityToRenderNode(entity, 
                    new BoundingBox(new Vector3f(loc.X - 20, loc.Y - 20, loc.Z - 20), new Vector3f(loc.X + 20, loc.Y + 20, loc.Z + 20)), RenderNode);
            }
        }

        private static void AddEntityToRenderNode(IEntity entity, BoundingBox boundingBox, RenderNodeBase node)
        {
            LeafRenderNode leafNode = node as LeafRenderNode;
            if (leafNode != null)
            {
                leafNode.Entities.Add(entity);
                return;
            }

            RenderNode renderNode = (RenderNode)node;
            foreach (RenderNodeBase childNode in renderNode.ChildNodes
                .Where(c => c != null && c.IntersectsWith(boundingBox)))
            {
                AddEntityToRenderNode(entity, boundingBox, childNode);
            }
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
