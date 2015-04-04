using JetBrains.Annotations;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework.Components
{
    /// <summary>
    /// Component for entities that emit particles
    /// </summary>
    [PublicAPI]
    public sealed class ParticleComponent : IBlockComponent, IComponent
    {
        [UsedImplicitly] public readonly string ControllerName;
        [UsedImplicitly] public Vector3i Location;
        [UsedImplicitly] public bool IsAlive = true;

        public ParticleComponent(string controllerName, Vector3i location = new Vector3i())
        {
            ControllerName = controllerName;
            Location = location;
        }
    }
}
