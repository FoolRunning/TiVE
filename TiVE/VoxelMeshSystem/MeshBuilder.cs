﻿using System;
using System.Runtime.CompilerServices;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Internal;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.VoxelMeshSystem
{
    internal sealed class MeshBuilder : IMeshBuilder
    {
        private readonly Vector4b[] locationData;
        private readonly Color4b[] colorData;
        private int vertexCount;

        private volatile bool locked;

        public MeshBuilder(int initialItemSize)
        {
            locationData = new Vector4b[initialItemSize];
            colorData = new Color4b[initialItemSize];
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
            return meshData;
        }

        public void DropMesh()
        {
            locked = false;
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int AddVoxel(VoxelSides sides, byte x, byte y, byte z, Color4b color)
        {
            locationData[vertexCount] = new Vector4b(x, y, z, (byte)sides);
            colorData[vertexCount++] = color;

            int numOfSides = ((int)sides).NumberOfSetBits();
            return numOfSides + numOfSides;
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
            return TiVEController.Backend.CreateData(locationData, vertexCount, 4, DataType.Vertex, DataValueType.Byte, false, false);
        }

        public override string ToString()
        {
            return string.Format("MeshBuilder locked={0} ({1}vert)", locked, vertexCount);
        }
    }
}
