using System;
using System.Runtime.CompilerServices;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.Voxels
{
    internal sealed class MeshBuilder
    {
        private readonly Vector3b[] locationData;
        private readonly Color4b[] colorData;
        private readonly int[] indexData;
        private int indexCount;
        private int vertexCount;
        private bool locked;

        private readonly object syncRoot = new object();

        public MeshBuilder(int initialItemSize, int initialIndexSize)
        {
            locationData = new Vector3b[initialItemSize];
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
        public int Add(int x, int y, int z, Color4b color)
        {
            locationData[vertexCount] = new Vector3b((byte)x, (byte)y, (byte)z);
            colorData[vertexCount] = color;

            return vertexCount++;
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

        public IVertexDataCollection GetInstanceData(params IRendererData[] instanceMeshData)
        {
            IVertexDataCollection dataCollection = TiVEController.Backend.CreateVertexDataCollection();
            foreach (IRendererData data in instanceMeshData)
                dataCollection.AddBuffer(data);
            dataCollection.AddBuffer(TiVEController.Backend.CreateData(locationData, vertexCount, 3, DataType.Instance, ValueType.Byte, false, false));
            dataCollection.AddBuffer(TiVEController.Backend.CreateData(colorData, vertexCount, 4, DataType.Instance, ValueType.Byte, true, false));
            return dataCollection;
        }

        public IRendererData GetLocationData()
        {
            return TiVEController.Backend.CreateData(locationData, vertexCount, 3, DataType.Vertex, ValueType.Byte, false, false);
        }

        public IRendererData GetColorData()
        {
            return TiVEController.Backend.CreateData(colorData, vertexCount, 4, DataType.Vertex, ValueType.Byte, true, false);
        }

        public IRendererData GetIndexData()
        {
            return TiVEController.Backend.CreateData(indexData, indexCount, 1, DataType.Index, ValueType.UInt, false, false);
        }

        public override string ToString()
        {
            return string.Format("MeshBuilder locked={0} ({1}vert {2}ind)", locked, vertexCount, indexCount);
        }
    }
}
