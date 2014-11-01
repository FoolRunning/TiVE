using System.Runtime.CompilerServices;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.Voxels
{
    internal sealed class LargeMeshBuilder : MeshBuilderBase
    {
        private readonly Vector3s[] locationData;

        public LargeMeshBuilder(int initialItemSize, int initialIndexSize) : base(initialItemSize, initialIndexSize)
        {
            locationData = new Vector3s[initialItemSize];
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
            dataCollection.AddBuffer(TiVEController.Backend.CreateData(locationData, vertexCount, 3, DataType.Instance, ValueType.Byte, false, false));
            dataCollection.AddBuffer(TiVEController.Backend.CreateData(colorData, vertexCount, 4, DataType.Instance, ValueType.Byte, true, false));
            return dataCollection;
        }

        public override IRendererData GetLocationData()
        {
            return TiVEController.Backend.CreateData(locationData, vertexCount, 3, DataType.Vertex, ValueType.Short, false, false);
        }
    }
}
