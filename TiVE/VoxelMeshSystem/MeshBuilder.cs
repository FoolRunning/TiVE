using System.Runtime.CompilerServices;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.VoxelMeshSystem
{
    internal sealed class MeshBuilder : MeshBuilderBase
    {
        private readonly Vector3b[] locationData;

        public MeshBuilder(int initialItemSize, int initialIndexSize) : base(initialItemSize, initialIndexSize)
        {
            locationData = new Vector3b[initialItemSize];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Add(byte x, byte y, byte z, Color4b color)
        {
            locationData[vertexCount] = new Vector3b(x, y, z);
            colorData[vertexCount] = color;

            return vertexCount++;
        }

        public override IRendererData GetLocationData()
        {
            return TiVEController.Backend.CreateData(locationData, vertexCount, 3, DataType.Vertex, DataValueType.Byte, false, false);
        }
    }
}
