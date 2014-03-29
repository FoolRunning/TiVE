using System;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.Meshes
{
    internal class MeshBuilder
    {
        private WeakReference<IVertexDataCollection> lastCreatedMesh;
        private Vector3b[] locationData;
        private Color4b[] colorData;
        private int[] indexData;
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
                lock (syncRoot)
                {
                    if (locked)
                        return true;
                    if (lastCreatedMesh == null)
                        return false;

                    IVertexDataCollection strong;
                    return lastCreatedMesh.TryGetTarget(out strong) && !strong.IsInitialized;
                }
            }
        }

        public void StartNewMesh()
        {
            lock (syncRoot)
            {
                locked = true;
                vertexCount = 0;
                indexCount = 0;
            }
        }

        public void CancelMesh()
        {
            lock (syncRoot)
                locked = false;
        }

        public int Add(float x, float y, float z, float cr, float cg, float cb, float ca)
        {
            //vertexData.Add(new Vector3(x, y, z));
            //colorData.Add(new Color4b(cr, cg, cb, ca));

            return vertexCount++;
        }

        public int Add(int x, int y, int z, byte cr, byte cg, byte cb, byte ca)
        {
            if (vertexCount >= locationData.Length)
            {
                ResizeArray(ref locationData);
                ResizeArray(ref colorData);
            }

            locationData[vertexCount] = new Vector3b((byte)x, (byte)y, (byte)z);
            colorData[vertexCount] = new Color4b(cr, cg, cb, ca);

            return vertexCount++;
        }

        public void AddIndex(int index)
        {
            if (indexCount >= indexData.Length)
                ResizeArray(ref indexData);

            indexData[indexCount] = index;
            indexCount++;
        }

        public IVertexDataCollection GetMesh()
        {
            IVertexDataCollection meshData;
            lock (syncRoot)
            {
                locked = false;
                meshData = TiVEController.Backend.CreateVertexDataCollection();
                meshData.AddBuffer(GetLocationData());
                meshData.AddBuffer(GetColorData());
                if (indexCount > 0)
                    meshData.AddBuffer(GetIndexData());
                lastCreatedMesh = new WeakReference<IVertexDataCollection>(meshData);
            }
            return meshData;
        }

        public IVertexDataCollection GetInstanceData(params IRendererData[] instanceMeshData)
        {
            IVertexDataCollection dataCollection;
            lock (syncRoot)
            {
                locked = false;
                dataCollection = TiVEController.Backend.CreateVertexDataCollection();
                foreach (IRendererData data in instanceMeshData)
                    dataCollection.AddBuffer(data);
                dataCollection.AddBuffer(TiVEController.Backend.CreateData(locationData, vertexCount, 3, DataType.Instance, ValueType.Byte, false, false));
                dataCollection.AddBuffer(TiVEController.Backend.CreateData(colorData, vertexCount, 4, DataType.Instance, ValueType.Byte, true, false));
                lastCreatedMesh = new WeakReference<IVertexDataCollection>(dataCollection);
            }
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
            return TiVEController.Backend.CreateData(indexData, indexCount, 1, DataType.Index, ValueType.Int, false, false);
        }

        private static void ResizeArray<T>(ref T[] array)
        {
            int newSize = array.Length + (array.Length * 2 / 3) + 1;
            Array.Resize(ref array, newSize);
        }
    }
}
