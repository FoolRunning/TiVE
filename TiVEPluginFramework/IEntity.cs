using System.Collections.Generic;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public interface IEntity
    {
        string Name { get; }

        IEnumerable<IComponent> Components { get; }

        void AddComponent(IComponent component);

        T GetComponent<T>() where T : class, IComponent;

        IComponent GetComponent(string componentName);
    }
}
