using JetBrains.Annotations;
using MoonSharp.Interpreter;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework.Components
{
    /// <summary>
    /// Component for entities that are renderable
    /// </summary>
    [PublicAPI]
    [MoonSharpUserData]
    public sealed class RenderComponent : IComponent
    {
        /// <summary>True if the entity is in the visible area of the screen, false otherwise</summary>
        internal volatile bool Visible;
        /// <summary>Number of voxels that make up the entity</summary>
        internal int VoxelCount;
        /// <summary>Number of voxels that make up the entity when rendered</summary>
        internal int RenderedVoxelCount;
        /// <summary>Number of polygons that make up the entity when rendered</summary>
        internal int PolygonCount;
        /// <summary>Mesh data for the entity for rendering</summary>
        internal IVertexDataInfo MeshData;
        /// <summary>Detail level of the mesh used to represent the voxels that make up this component</summary>
        internal int LoadedVoxelDetailLevel = -1;
        /// <summary>Object to use for locking when accessing the mesh data</summary>
        internal readonly object SyncLock = new object();

        [PublicAPI]
        public BoundingBox BoundingBox;

        [PublicAPI]
        public Vector3f Location;

        public RenderComponent()
        {
        }

        public RenderComponent(Vector3f location)
        {
            Location = location;
        }
    }
}
