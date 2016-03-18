using System;
using System.Runtime.CompilerServices;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Internal;

namespace ProdigalSoftware.TiVE.VoxelMeshSystem
{
    internal abstract class MeshBuilderBase : IMeshBuilder
    {
        protected readonly Color4b[] colorData;
        protected int vertexCount;

        private readonly int[] indexData;
        private int indexCount;
        private volatile bool locked;

        protected MeshBuilderBase(int initialItemSize, int initialIndexSize)
        {
            colorData = new Color4b[initialItemSize];
            indexData = new int[initialIndexSize];
        }

        public bool IsLocked 
        {
            get { return locked; }
        }

        #region Implementation of IMeshBuilder
        public IVertexDataInfo GetMesh()
        {
            IVertexDataCollection meshData = TiVEController.Backend.CreateVertexDataCollection();
            meshData.AddBuffer(GetLocationData());
            meshData.AddBuffer(GetColorData());
            if (indexCount > 0)
                meshData.AddBuffer(GetIndexData());
            return meshData;
        }

        public void DropMesh()
        {
            locked = false;
        }
        #endregion

        public void StartNewMesh()
        {
            if (locked)
                throw new InvalidOperationException("New mesh can not be started when there is a mesh in progress");

            locked = true;
            vertexCount = 0;
            indexCount = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddIndex(int index)
        {
            indexData[indexCount] = index;
            indexCount++;
        }

        public abstract IRendererData GetLocationData();

        public IRendererData GetColorData()
        {
            return TiVEController.Backend.CreateData(colorData, vertexCount, 4, DataType.Vertex, DataValueType.Byte, true, false);
        }

        private IRendererData GetIndexData()
        {
            return TiVEController.Backend.CreateData(indexData, indexCount, 1, DataType.Index, DataValueType.UInt, false, false);
        }

        public override string ToString()
        {
            return string.Format("MeshBuilder locked={0} ({1}vert {2}ind)", locked, vertexCount, indexCount);
        }
    }
}
