namespace ProdigalSoftware.TiVE.Renderer
{
    internal interface IGameWorldRenderer
    {
        void Update(Camera camera, float timeSinceLastFrame);

        void Draw(Camera camera, out RenderStatistics stats);
    }
}
