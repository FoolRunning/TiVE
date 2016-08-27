using System.IO;
using JetBrains.Annotations;
using ProdigalSoftware.TiVEPluginFramework.Internal;

namespace ProdigalSoftware.TiVEPluginFramework.Components
{
    public abstract class VoxelMeshComponent : IComponent
    {
        #region Internal data
        internal const byte BlankShadowType = byte.MaxValue;

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
        internal LODLevel VoxelDetailLevelToLoad = LODLevel.NotSet;
        /// <summary></summary>
        internal byte ShadowTypeToLoad = BlankShadowType;
        /// <summary>Detail level of the mesh used to represent the voxels that make up this component</summary>
        internal LODLevel VisibleVoxelDetailLevel = LODLevel.NotSet;
        /// <summary></summary>
        internal byte VisibleShadowType = BlankShadowType;
        /// <summary>The MeshBuilder containing the </summary>
        internal IMeshBuilder MeshBuilder;
        /// <summary>Object to use for locking when accessing the mesh data</summary>
        internal readonly object SyncLock = new object();
        #endregion

        [UsedImplicitly]
        public Vector3f Location;

        protected VoxelMeshComponent()
        {
        }

        protected VoxelMeshComponent(Vector3f location)
        {
            Location = location;
        }

        public abstract void SaveTo(BinaryWriter writer);
    }
}
