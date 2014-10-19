using System;

namespace ProdigalSoftware.TiVE.Sound
{
    internal interface ISoundEngine : IDisposable
    {
        void Stream2DSound(string filePath, bool loop);
        
        void Stream3DSound(string filePath, int x, int y, int z, bool loop);

        void CacheSound(string name, string filePath);

        void PlayCachedSound2D(string name);

        void PlayCachedSound3D(string name, int x, int y, int z);

        void Set3DListenerLocation(int x, int y, int z, int lookAtX, int lookAtY, int lookAtZ);
    }
}
