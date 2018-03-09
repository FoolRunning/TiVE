using System;
using System.Runtime.CompilerServices;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Internal;

namespace ProdigalSoftware.TiVE.VoxelMeshSystem
{
    internal sealed class MeshBuilder : IMeshBuilder
    {
        private readonly Vector4b[] locationData;
        private readonly Color4b[] colorData;
        private readonly Vector3f[] normalData;
        private readonly Vector4b[] voxelStateData;
        private int vertexCount;

        private volatile bool locked;

        public MeshBuilder(int initialItemSize)
        {
            locationData = new Vector4b[initialItemSize];
            colorData = new Color4b[initialItemSize];
            normalData = new Vector3f[initialItemSize];
            voxelStateData = new Vector4b[initialItemSize];
        }

        public bool IsLocked => locked;

        #region Implementation of IMeshBuilder
        public IVertexDataInfo GetMesh()
        {
            IVertexDataCollection meshData = TiVEController.Backend.CreateVertexDataCollection();
            meshData.AddBuffer(GetLocationData());
            meshData.AddBuffer(GetColorData());
            meshData.AddBuffer(GetNormalData());
            meshData.AddBuffer(GetVoxelStateData());
            return meshData;
        }

        public void DropMesh()
        {
            locked = false;
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddVoxel(CubeSides sides, byte x, byte y, byte z, Color4b color, Vector3f normal, byte ambientOcclusionFactor)
        {
            locationData[vertexCount] = new Vector4b(x, y, z, (byte)sides);
            colorData[vertexCount] = color;
            normalData[vertexCount] = normal;
            voxelStateData[vertexCount++] = new Vector4b(ambientOcclusionFactor, 0, 0, 0);
        }

        public void StartNewMesh()
        {
            if (locked)
                throw new InvalidOperationException("New mesh can not be started when there is a mesh in progress");

            locked = true;
            vertexCount = 0;
        }

        private static readonly object syncRoot = new object();
        private static volatile int maxData;

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
            return TiVEController.Backend.CreateData(locationData, vertexCount, 4, DataType.Vertex, DataValueType.Byte, false, false);
        }

        public IRendererData GetColorData()
        {
            return TiVEController.Backend.CreateData(colorData, vertexCount, 4, DataType.Vertex, DataValueType.Byte, true, false);
        }

        public IRendererData GetNormalData()
        {
            return TiVEController.Backend.CreateData(normalData, vertexCount, 3, DataType.Vertex, DataValueType.Float, false, false);
        }

        public IRendererData GetVoxelStateData()
        {
            return TiVEController.Backend.CreateData(voxelStateData, vertexCount, 4, DataType.Vertex, DataValueType.Byte, false, false);
        }

        public override string ToString()
        {
            return string.Format("MeshBuilder locked={0} ({1}vert)", locked, vertexCount);
        }
    }
}
