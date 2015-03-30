using System;
using ProdigalSoftware.TiVE.RenderSystem;
using ProdigalSoftware.TiVE.RenderSystem.Lighting;
using ProdigalSoftware.TiVE.RenderSystem.World;

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
