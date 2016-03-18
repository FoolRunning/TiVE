using System;

namespace ProdigalSoftware.TiVE.SoundSystem
{
    internal interface ISound : IDisposable
    {
        string Name { get; }
    }
}
