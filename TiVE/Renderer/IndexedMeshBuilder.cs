using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal sealed class IndexedMeshBuilder
    {
        private readonly List<Vector3> vertexData = new List<Vector3>(50);
        private readonly List<Color4> colorData = new List<Color4>(50);
        private readonly List<byte> indexData = new List<byte>(100);
        private byte vertexCount;

        public void BeginNewMesh()
        {
            vertexData.Clear();
            colorData.Clear();
            indexData.Clear();
            vertexCount = 0;
        }

        public byte AddVertex(float x, float y, float z, float cr, float cg, float cb, float ca)
        {
            vertexData.Add(new Vector3(x, y, z));
            colorData.Add(new Color4(cr, cg, cb, ca));

            return vertexCount++;
        }

        public byte AddVertex(float x, float y, float z, byte cr, byte cg, byte cb, byte ca)
        {
            vertexData.Add(new Vector3(x, y, z));
            colorData.Add(new Color4(cr, cg, cb, ca));

            return vertexCount++;
        }

        public void AddIndex(byte index)
        {
            indexData.Add(index);
        }

        public IVertexDataCollection GetMesh()
        {
            IVertexDataCollection dataCollection = TiVEController.Backend.CreateVertexDataCollection();
            dataCollection.AddBuffer(GetLocationData());
            dataCollection.AddBuffer(GetColorData());
            dataCollection.AddBuffer(GetIndexData());
            return dataCollection;
        }

        public IRendererData GetLocationData()
        {
            return TiVEController.Backend.CreateData(vertexData.ToArray(), 3, DataType.Vertex, false);
        }

        public IRendererData GetColorData()
        {
            return TiVEController.Backend.CreateData(colorData.ToArray(), 4, DataType.Vertex, false);
        }

        public IRendererData GetIndexData()
        {
            return TiVEController.Backend.CreateData(indexData.ToArray(), 1, DataType.Index, false);
        }
    }
}
