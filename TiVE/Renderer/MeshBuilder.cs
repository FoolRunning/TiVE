using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal sealed class MeshBuilder
    {
        private readonly List<Vector3> vertexData = new List<Vector3>(5000);
        private readonly List<Color4> colorData = new List<Color4>(5000);

        public void BeginNewMesh()
        {
            vertexData.Clear();
            colorData.Clear();
        }

        public void AddVertex(float x, float y, float z, float cr, float cg, float cb, float ca)
        {
            vertexData.Add(new Vector3(x, y, z));
            colorData.Add(new Color4(cr, cg, cb, ca));
        }

        public void AddVertex(float x, float y, float z, byte cr, byte cg, byte cb, byte ca)
        {
            vertexData.Add(new Vector3(x, y, z));
            colorData.Add(new Color4(cr, cg, cb, ca));
        }

        public IVertexDataCollection GetMesh()
        {
            IVertexDataCollection dataCollection = TiVEController.Backend.CreateVertexDataCollection();
            dataCollection.AddBuffer(GetLocationData());
            dataCollection.AddBuffer(GetColorData());
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
    }
}
