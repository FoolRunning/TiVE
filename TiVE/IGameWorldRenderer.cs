using ProdigalSoftware.TiVE.Renderer;

namespace ProdigalSoftware.TiVE
{
    internal interface IGameWorldRenderer
    {
        void Update(Camera camera, float timeSinceLastFrame);

        RenderStatistics Draw(Camera camera);
    }
}
