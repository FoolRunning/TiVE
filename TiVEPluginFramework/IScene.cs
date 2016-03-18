using System.Collections.Generic;
using JetBrains.Annotations;
using ProdigalSoftware.TiVEPluginFramework.Components;

namespace ProdigalSoftware.TiVEPluginFramework
{
    /// <summary>
    /// Represents an active Scene in TiVE
    /// </summary>
    [PublicAPI]
    public interface IScene
    {
        /// <summary>
        /// Gets a list of all entities in a scene
        /// </summary>
        IEnumerable<IEntity> AllEntities { get; }

        /// <summary>
        /// Creates a new entity with the specified name
        /// </summary>
        IEntity CreateNewEntity(string entityName);

        /// <summary>
        /// Gets a list of all entities that have a component of the specified type
        /// </summary>
        IEnumerable<IEntity> GetEntitiesWithComponent<T>() where T : class, IComponent;

        /// <summary>
        /// Deletes the specified entity from the scene
        /// </summary>
        void DeleteEntity(IEntity entity);

        /// <summary>
        /// Sets the game world of the scene to the world with the specified name
        /// </summary>
        void SetGameWorld(string worldName);

        /// <summary>
        /// Finds the first enabled camera in this Scene
        /// </summary>
        CameraComponent FindCamera();
    }
}
