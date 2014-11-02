using System;
using System.Runtime.CompilerServices;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.Voxels
{
    internal abstract class MeshBuilderBase
    {
        protected readonly Color4b[] colorData;
        protected int vertexCount;

        private readonly int[] indexData;
        private int indexCount;
        private bool locked;

        private readonly object syncRoot = new object();

        protected MeshBuilderBase(int initialItemSize, int initialIndexSize)
        {
            colorData = new Color4b[initialItemSize];
            indexData = new int[initialIndexSize];
        }

        public bool IsLocked 
        {
            get 
            {
                using (new PerformanceLock(syncRoot))
                    return locked;
            }
        }

        public void DropMesh()
        {
            using (new PerformanceLock(syncRoot))
                locked = false;
        }

        public void StartNewMesh()
        {
            using (new PerformanceLock(syncRoot))
            {
                if (locked)
                    throw new InvalidOperationException("New mesh can not be started when there is a mesh in progress");

                locked = true;
                vertexCount = 0;
                indexCount = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddIndex(int index)
        {
            indexData[indexCount] = index;
            indexCount++;
        }

        public IVertexDataCollection GetMesh()
        {
            IVertexDataCollection meshData = TiVEController.Backend.CreateVertexDataCollection();
            meshData.AddBuffer(GetLocationData());
            meshData.AddBuffer(GetColorData());
            if (indexCount > 0)
                meshData.AddBuffer(GetIndexData());
            return meshData;
        }

        public abstract IRendererData GetLocationData();

        private IRendererData GetColorData()
        {
            return TiVEController.Backend.CreateData(colorData, vertexCount, 4, DataType.Vertex, ValueType.Byte, true, false);
        }

        private IRendererData GetIndexData()
        {
            return TiVEController.Backend.CreateData(indexData, indexCount, 1, DataType.Index, ValueType.UInt, false, false);
        }

        public override string ToString()
        {
            return string.Format("MeshBuilder locked={0} ({1}vert {2}ind)", locked, vertexCount, indexCount);
        }
    }
}
