using JetBrains.Annotations;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework.Components
{
    /// <summary>
    /// Component for entities that are renderable
    /// </summary>
    [PublicAPI]
    public sealed class RenderComponent : IComponent
    {
        /// <summary>Number of voxels that make up the entity</summary>
        internal int VoxelCount;
        /// <summary>Number of voxels that make up the entity when rendered</summary>
        internal int RenderedVoxelCount;
        /// <summary>Number of polygons that make up the entity when rendered</summary>
        internal int PolygonCount;
        /// <summary>Mesh data for the entity for rendering</summary>
        internal IVertexDataInfo MeshData;
        /// <summary>Object to use for locking when accessing the mesh data</summary>
        internal readonly object SyncLock = new object();

        [PublicAPI]
        public BoundingBox BoundingBox;

        [PublicAPI]
        public Vector3f Location;
    }
}
