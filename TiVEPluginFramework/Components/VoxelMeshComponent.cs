using ProdigalSoftware.TiVEPluginFramework.Internal;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework.Components
{
    public abstract class VoxelMeshComponent : IComponent
    {
        #region Internal data
        internal const byte BlankDetailLevel = byte.MaxValue;

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
        internal byte LoadedVoxelDetailLevel = BlankDetailLevel;
        /// <summary>Object to use for locking when accessing the mesh data</summary>
        internal readonly object SyncLock = new object();
        #endregion

        public Vector3f Location;

        protected VoxelMeshComponent(Vector3f location)
        {
            Location = location;
        }
    }
}
