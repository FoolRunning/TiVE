namespace ProdigalSoftware.TiVE.Renderer
{
    internal enum PrimitiveType
    {
        Points,
        Lines,
        Triangles,
        Quads,
    }

    interface IMesh
    {
        void Delete();

        bool Initialize();

        void Draw();
    }
}
