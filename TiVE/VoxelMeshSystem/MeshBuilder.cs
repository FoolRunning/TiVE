using System;
using System.Runtime.CompilerServices;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Internal;

namespace ProdigalSoftware.TiVE.VoxelMeshSystem
{
    internal sealed class MeshBuilder : IMeshBuilder
    {
        private readonly Vector3b[] locationData;
        private readonly Color4b[] colorData;
        private readonly int[] indexData;
        private int vertexCount;

        private int indexCount;
        private volatile bool locked;

        public MeshBuilder(int initialItemSize, int initialIndexSize)
        {
            locationData = new Vector3b[initialItemSize];
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Add(byte x, byte y, byte z, Color4b color)
        {
            locationData[vertexCount] = new Vector3b(x, y, z);
            colorData[vertexCount] = color;

            return vertexCount++;
        }

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
        
        private static readonly object syncRoot = new object();
        private static volatile int maxIndex;
        private static volatile int maxData;

        public IRendererData GetColorData()
        {
            return TiVEController.Backend.CreateData(colorData, vertexCount, 4, DataType.Vertex, DataValueType.Byte, true, false);
        }

        public IRendererData GetLocationData()
        {
            lock (syncRoot)
            {
                if (vertexCount > maxData)
                {
                    maxData = vertexCount;
                    Console.WriteLine("Max data: " + maxData);
                }
            }
            return TiVEController.Backend.CreateData(locationData, vertexCount, 3, DataType.Vertex, DataValueType.Byte, false, false);
        }

        private IRendererData GetIndexData()
        {
            lock (syncRoot)
            {
                if (indexCount > maxIndex)
                {
                    maxIndex = indexCount;
                    Console.WriteLine("Max index: " + maxIndex);
                }
            }
            return TiVEController.Backend.CreateData(indexData, indexCount, 1, DataType.Index, DataValueType.UInt, false, false);
        }

        public override string ToString()
        {
            return string.Format("MeshBuilder locked={0} ({1}vert {2}ind)", locked, vertexCount, indexCount);
        }
    }
}
