namespace ProdigalSoftware.TiVEPluginFramework.Internal
{
    internal interface IMeshBuilder
    {
        IVertexDataInfo GetMesh();
        void DropMesh();
    }
}
