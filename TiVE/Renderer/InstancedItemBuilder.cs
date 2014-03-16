using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal sealed class InstancedItemBuilder
    {
        private readonly List<Vector3> instanceLocationData = new List<Vector3>(5000);
        private readonly List<Color4> instanceColorData = new List<Color4>(5000);

        public void BeginNewItemInstances()
        {
            instanceLocationData.Clear();
            instanceColorData.Clear();
        }

        public void AddInstance(float x, float y, float z, float cr, float cg, float cb, float ca)
        {
            instanceLocationData.Add(new Vector3(x, y, z));
            instanceColorData.Add(new Color4(cr, cg, cb, ca));
        }

        public void AddInstance(float x, float y, float z, byte cr, byte cg, byte cb, byte ca)
        {
            instanceLocationData.Add(new Vector3(x, y, z));
            instanceColorData.Add(new Color4(cr, cg, cb, ca));
        }

        public IVertexDataCollection GetInstanceData(params IRendererData[] instanceMeshData)
        {
            IVertexDataCollection dataCollection = TiVEController.Backend.CreateVertexDataCollection();
            foreach (IRendererData data in instanceMeshData)
                dataCollection.AddBuffer(data);
            dataCollection.AddBuffer(TiVEController.Backend.CreateData(instanceLocationData.ToArray(), 3, DataType.Instance, false));
            dataCollection.AddBuffer(TiVEController.Backend.CreateData(instanceColorData.ToArray(), 4, DataType.Instance, false));
            return dataCollection;
        }
    }
}
