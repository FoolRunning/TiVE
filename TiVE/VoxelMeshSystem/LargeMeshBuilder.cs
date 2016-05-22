using System;
using System.Runtime.CompilerServices;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Internal;

namespace ProdigalSoftware.TiVE.VoxelMeshSystem
{
    internal sealed class LargeMeshBuilder : IMeshBuilder
    {
        private readonly Vector3s[] locationData;
        private readonly Color4b[] colorData;
        private readonly int[] indexData;
        private int vertexCount;
        private int indexCount;
        private volatile bool locked;

        public LargeMeshBuilder(int initialItemSize, int initialIndexSize)
        {
            locationData = new Vector3s[initialItemSize];
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Add(short x, short y, short z, Color4b color)
        {
            locationData[vertexCount] = new Vector3s(x, y, z);
            colorData[vertexCount] = color;

            return vertexCount++;
        }

        public IVertexDataCollection GetInstanceData(params IRendererData[] instanceMeshData)
        {
            IVertexDataCollection dataCollection = TiVEController.Backend.CreateVertexDataCollection();
            foreach (IRendererData data in instanceMeshData)
                dataCollection.AddBuffer(data);
            dataCollection.AddBuffer(TiVEController.Backend.CreateData(locationData, vertexCount, 3, DataType.Instance, DataValueType.Byte, false, false));
            dataCollection.AddBuffer(TiVEController.Backend.CreateData(colorData, vertexCount, 4, DataType.Instance, DataValueType.Byte, true, false));
            return dataCollection;
        }

        public IRendererData GetColorData()
        {
            return TiVEController.Backend.CreateData(colorData, vertexCount, 4, DataType.Vertex, DataValueType.Byte, true, false);
        }

        public IRendererData GetLocationData()
        {
            return TiVEController.Backend.CreateData(locationData, vertexCount, 3, DataType.Vertex, DataValueType.Short, false, false);
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
