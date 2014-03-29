using System;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal interface IGameWorldRenderer : IDisposable
    {
        void Draw(Camera camera, out RenderStatistics stats);
    }
}
