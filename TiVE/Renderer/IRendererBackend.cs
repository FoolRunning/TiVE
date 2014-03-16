namespace ProdigalSoftware.TiVE.Renderer
{

    internal interface IRendererBackend
    {
        IDisplay CreateDisplay();

        void Draw(PrimitiveType primitiveType, IVertexDataCollection data);

        IVertexDataCollection CreateVertexDataCollection();

        IRendererData CreateData<T>(T[] data, int dataPerVertex, DataType dataType, bool dynamic) where T : struct;
        
        IShaderProgram CreateShaderProgram();
    }
}
