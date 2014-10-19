using System;
using IrrKlang;

namespace ProdigalSoftware.TiVE.Sound.irrKlang
{
    internal sealed class IrrKlangSoundEngine : ISoundEngine
    {
        private readonly IrrKlang.ISoundEngine soundEngine;

        public IrrKlangSoundEngine()
        {
            soundEngine = new IrrKlang.ISoundEngine();
        }

        public void Dispose()
        {
            soundEngine.Dispose();
        }

        #region Implementation of ISoundEngine
        public void Stream2DSound(string filePath, bool loop)
        {
            soundEngine.Play2D(filePath, loop, false, StreamMode.AutoDetect, false);
        }

        public void Stream3DSound(string filePath, int x, int y, int z, bool loop)
        {
            soundEngine.Play3D(filePath, x, y, z, loop, false, StreamMode.AutoDetect, false);
        }

        public void CacheSound(string name, string filePath)
        {
            //soundEngine.AddSoundSourceFromFile()
            throw new NotImplementedException();
        }

        public void PlayCachedSound2D(string name)
        {
            throw new NotImplementedException();
        }

        public void PlayCachedSound3D(string name, int x, int y, int z)
        {
            throw new NotImplementedException();
        }

        public void Set3DListenerLocation(int x, int y, int z, int lookAtX, int lookAtY, int lookAtZ)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
