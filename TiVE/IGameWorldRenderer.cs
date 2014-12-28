using System;
using ProdigalSoftware.TiVE.Renderer;
using ProdigalSoftware.TiVE.Renderer.Lighting;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE
{
    internal interface IGameWorldRenderer : IDisposable
    {
        GameWorld GameWorld { get; }

        BlockList BlockList { get; }

        LightProvider LightProvider { get; }

        void SetGameWorld(BlockList newBlockList, GameWorld newGameWorld);

        void RefreshLevel();

        void Update(Camera camera, float timeSinceLastFrame);

        RenderStatistics Draw(Camera camera);
    }
}
