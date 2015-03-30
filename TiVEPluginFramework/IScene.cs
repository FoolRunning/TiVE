using System.Collections.Generic;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public interface IScene
    {
        IEntity CreateNewEntity(string entityName);

        IEnumerable<IEntity> GetEntitiesWithComponent<T>() where T : class, IComponent;

        void DeleteEntity(IEntity entity);

        void SetGameWorld(string worldName);
    }
}
