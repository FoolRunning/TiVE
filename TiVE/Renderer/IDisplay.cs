using System;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal interface IDisplay : IDisposable
    {
        void RunMainLoop(GameLogic game);
    }
}
