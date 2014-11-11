using System;
using ProdigalSoftware.TiVE.Renderer;
using ProdigalSoftware.TiVE.Renderer.World;

namespace ProdigalSoftware.TiVE
{
    internal interface IGameWorldRenderer : IDisposable
    {
        GameWorld GameWorld { get; }

        BlockList BlockList { get; }

        void SetGameWorld(BlockList newBlockList, GameWorld newGameWorld);

        void RefreshLevel();

        void Update(Camera camera, float timeSinceLastFrame);

        RenderStatistics Draw(Camera camera);
    }
}
