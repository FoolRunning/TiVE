using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal sealed class MeshBuilder
    {
        private readonly BeginMode primitiveType;
        private readonly List<Vector3> vertexData = new List<Vector3>(5000);
        private readonly List<Color4> colorData = new List<Color4>(5000);
        private readonly List<uint> indexData = new List<uint>(2000);
        private uint vertexCount;

        public MeshBuilder(BeginMode primitiveType)
        {
            this.primitiveType = primitiveType;
        }

        public void BeginNewMesh()
        {
            vertexData.Clear();
            colorData.Clear();
            indexData.Clear();
            vertexCount = 0;
        }

        public uint AddVertex(float x, float y, float z, float cr, float cg, float cb, float ca)
        {
            vertexData.Add(new Vector3(x, y, z));
            colorData.Add(new Color4(cr, cg, cb, ca));

            return vertexCount++;
        }

        public uint AddVertex(float x, float y, float z, byte cr, byte cg, byte cb, byte ca)
        {
            vertexData.Add(new Vector3(x, y, z));
            colorData.Add(new Color4(cr, cg, cb, ca));

            return vertexCount++;
        }

        public void AddIndex(uint index)
        {
            indexData.Add(index);
        }

        public Mesh GetMesh()
        {
            return new Mesh(vertexData.ToArray(), colorData.ToArray(), indexData.ToArray(), primitiveType);
        }
    }
}
