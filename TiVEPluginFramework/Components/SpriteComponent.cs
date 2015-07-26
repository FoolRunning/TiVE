using System.Collections.Generic;
using JetBrains.Annotations;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework.Components
{
    /// <summary>
    /// Component for entities that are renderable
    /// </summary>
    [PublicAPI]
    public sealed class SpriteComponent : VoxelMeshComponent
    {
        [UsedImplicitly] public BoundingBox BoundingBox;
        [UsedImplicitly] public readonly List<SpriteAnimation> Animations = new List<SpriteAnimation>(); 

        public SpriteComponent(Vector3f location, BoundingBox boundingBox) : base(location)
        {
            Location = location;
            BoundingBox = boundingBox;
        }
    }
}
