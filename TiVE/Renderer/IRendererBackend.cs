namespace ProdigalSoftware.TiVE.Renderer
{
    internal interface IRendererBackend
    {
        IDisplay CreateDisplay();

        void Draw(PrimitiveType primitiveType, IVertexDataCollection vertexes);

        IVertexDataCollection CreateVertexDataCollection();

        IVertexData CreateVertexData<T>(T[] data, int dataPerVertex, BufferType bufferType, bool dynamic) where T : struct;

        IShaderProgram CreateShaderProgram();
    }
}
