using System;
using ProdigalSoftware.TiVE.Renderer;
using ProdigalSoftware.TiVE.Renderer.Lighting;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE
{
    internal interface IGameWorldRenderer : IDisposable
    {
        Camera Camera { get; set; }

        GameWorld GameWorld { get; }

        BlockList BlockList { get; }

        LightProvider LightProvider { get; }

        void SetGameWorld(BlockList newBlockList, GameWorld newGameWorld);

        void RefreshLevel();

        void Update(float timeSinceLastFrame);

        RenderStatistics Draw();
    }
}
