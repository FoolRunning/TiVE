using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal sealed class MeshBuilder
    {
        private readonly bool includeColor;
        //private readonly bool includeTexCoords;
        //private readonly bool includeNormal;
        private readonly BeginMode primitiveType;
        private readonly List<float> vertexData = new List<float>(5000);
        private readonly List<byte> colorData = new List<byte>();
        //private readonly List<float> textureData = new List<float>();
        //private readonly List<float> normalData = new List<float>();
        private readonly List<uint> indexData = new List<uint>(2000);
        private uint vertexCount;

        public MeshBuilder(BeginMode primitiveType, bool includeColor/*, bool includeTexCoords, bool includeNormal*/)
        {
            this.primitiveType = primitiveType;
            this.includeColor = includeColor;
            //this.includeTexCoords = includeTexCoords;
            //this.includeNormal = includeNormal;
        }


        public uint AddVertex(float x, float y, float z, byte cr = 0, byte cg = 0, byte cb = 0, byte ca = 0/*, 
            float tx = 0, float ty = 0, float nx = 0, float ny = 0, float nz = 0*/)
        {
            vertexData.Add(x);
            vertexData.Add(y);
            vertexData.Add(z);

            if (includeColor)
            {
                colorData.Add(cr);
                colorData.Add(cg);
                colorData.Add(cb);
                colorData.Add(ca);
            }

            //if (includeTexCoords)
            //{
            //    textureData.Add(tx);
            //    textureData.Add(ty);
            //}

            //if (includeNormal)
            //{
            //    Vector3 vector = new Vector3(nx, ny, nz);
            //    vector.Normalize();
            //    normalData.Add(vector.X);
            //    normalData.Add(vector.Y);
            //    normalData.Add(vector.Z);
            //}

            return vertexCount++;
        }

        public void AddIndex(uint index)
        {
            indexData.Add(index);
        }

        public Mesh GetMesh()
        {
            return new Mesh(vertexData.ToArray(), colorData.ToArray(), /*textureData.ToArray(), normalData.ToArray(),*/ indexData.ToArray(), primitiveType);
        }
    }
}
