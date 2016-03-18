using System;

namespace ProdigalSoftware.TiVE.SoundSystem
{
    internal interface ISoundEngine : IDisposable
    {
        ISound CreateSound(string name, string filePath);

        ISound CreateSound(string name, AudioData data);

        void DeleteSound(string name);

        void Stream2DSound(string filePath, bool loop);
        
        void Stream3DSound(string filePath, int x, int y, int z, bool loop);

        void PlaySound2D(ISound sound);

        void PlaySound3D(ISound sound, int x, int y, int z);

        void Set3DListenerLocation(int x, int y, int z, int lookAtX, int lookAtY, int lookAtZ);

        void StopSounds();

        void UpdateSystem();
    }
}
