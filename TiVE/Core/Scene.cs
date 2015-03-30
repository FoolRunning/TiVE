using System;
using System.Collections.Generic;
using System.Linq;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Core
{
    internal sealed class Scene : IScene, IDisposable
    {
        private const int InitialEntityListSize = 10;

        private readonly Dictionary<Type, List<IEntity>> entityComponentTypeMap = new Dictionary<Type, List<IEntity>>(50);
        private readonly List<IEntity> entities = new List<IEntity>(50);

        public IEnumerable<IEntity> AllEntities
        {
            get { return entities; }
        }

        public void Dispose()
        {

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

        private void AddEntityToTypeMap(IEntity entity, IComponent component)
        {
            Type componentType = component.GetType();
            List<IEntity> entitiesWithType;
            if (!entityComponentTypeMap.TryGetValue(componentType, out entitiesWithType))
                entityComponentTypeMap[componentType] = entitiesWithType = new List<IEntity>(InitialEntityListSize);

            entitiesWithType.Add(entity);
        }

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

            public string Name
            {
                get { return name; }
            }

            public IEnumerable<IComponent> Components
            {
                get { return components; }
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

            public void AddComponent(IComponent component)
            {
                if (components == null)
                    components = new IComponent[1];
                else
                    Array.Resize(ref components, components.Length + 1);

                components[components.Length - 1] = component;
                owningScene.AddEntityToTypeMap(this, component);
            }

            public void ClearComponents()
            {
                components = null;
            }
        }
    }
}
