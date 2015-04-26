using System;

namespace ProdigalSoftware.TiVE.Core.Backend
{
    internal interface ITexture : IDisposable
    {
        int Id { get; }

        void Initialize();

        void Activate();

        void UpdateTextureData(byte[] newData);
    }
}
