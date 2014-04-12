﻿namespace ProdigalSoftware.TiVE.Renderer
{
    internal enum PrimitiveType
    {
        Points,
        Lines,
        Triangles,
        Quads,
    }

    internal enum BlendMode
    {
        None,
        Realistic,
        Additive,
    }

    internal interface IRendererBackend
    {
        IDisplay CreateDisplay();

        void Draw(PrimitiveType primitiveType, IVertexDataCollection data);

        IVertexDataCollection CreateVertexDataCollection();

        IRendererData CreateData<T>(T[] data, int elementCount, int elementsPerVertex, DataType dataType, 
            ValueType valueType, bool normalize, bool dynamic) where T : struct;
        
        IShaderProgram CreateShaderProgram();

        void SetBlendMode(BlendMode mode);

        void DisableDepthWriting();

        void EnableDepthWriting();
    }
}
