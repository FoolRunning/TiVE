namespace ProdigalSoftware.TiVE.Renderer
{
    internal interface IGameWorldRenderer
    {
        void Update(Camera camera, float timeSinceLastFrame);

        RenderStatistics Draw(Camera camera);
    }
}
