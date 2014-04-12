using System;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal interface IGameWorldRenderer : IDisposable
    {
        void Update(Camera camera, float timeSinceLastFrame);

        void Draw(Camera camera, out RenderStatistics stats);
    }
}
