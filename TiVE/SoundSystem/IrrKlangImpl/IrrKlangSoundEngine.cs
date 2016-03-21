using System;
using System.IO;
using IrrKlang;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE.SoundSystem.IrrKlangImpl
{
    internal sealed class IrrKlangSoundEngine : ISoundEngine
    {
        private readonly IrrKlang.ISoundEngine irrKlangSoundEngine;

        public IrrKlangSoundEngine()
        {
            irrKlangSoundEngine = new IrrKlang.ISoundEngine();
        }

        #region Implementation of ISoundEngine
        public void Dispose()
        {
            irrKlangSoundEngine.Dispose();
        }

        public ISound CreateSound(string name, string filePath)
        {
            ISoundSource soundSource;
            using (Stream stream = new FileStream(filePath, FileMode.Open))
                soundSource = irrKlangSoundEngine.AddSoundSourceFromIOStream(stream, name);

            return new IrrKlangSound(name, soundSource);
        }

        public ISound CreateSound(string name, AudioData data)
        {
            AudioFormat format = new AudioFormat();
            format.ChannelCount = data.NumChannels;
            format.FrameCount = data.FrameCount;
            format.SampleRate = data.SampleRate;
            format.Format = SampleFormat.Signed16Bit;

            return new IrrKlangSound(name, irrKlangSoundEngine.AddSoundSourceFromPCMData(data.Data, name, format));
        }

        public void DeleteSound(string name)
        {
            irrKlangSoundEngine.RemoveSoundSource(name);
        }

        public void Stream2DSound(string filePath, bool loop)
        {
            irrKlangSoundEngine.Play2D(filePath, loop, false, StreamMode.AutoDetect, false);
        }

        public void Stream3DSound(string filePath, int x, int y, int z, bool loop)
        {
            irrKlangSoundEngine.Play3D(filePath, x, y, z, loop, false, StreamMode.AutoDetect, false);
        }

        public void PlaySound2D(ISound sound)
        {
            irrKlangSoundEngine.Play2D(((IrrKlangSound)sound).SoundSource, false, false, false);
        }

        public void PlaySound3D(ISound sound, int x, int y, int z)
        {
            irrKlangSoundEngine.Play3D(((IrrKlangSound)sound).SoundSource, x, y, z, false, false, true);
        }

        public void Set3DListenerLocation(int x, int y, int z, int lookAtX, int lookAtY, int lookAtZ)
        {
            irrKlangSoundEngine.SetListenerPosition(x, y, z, lookAtX, lookAtY, lookAtZ);
        }

        public void StopSounds()
        {
            irrKlangSoundEngine.StopAllSounds();
        }

        public void UpdateSystem()
        {
            irrKlangSoundEngine.Update();
        }
        #endregion

        #region IrrKlangSound class
        private sealed class IrrKlangSound : ISound
        {
            public readonly ISoundSource SoundSource;
            private readonly string name;

            public IrrKlangSound(string name, ISoundSource soundSource)
            {
                if (soundSource == null)
                    throw new ArgumentNullException("soundSource");

                this.name = name;
                SoundSource = soundSource;
            }

            ~IrrKlangSound()
            {
                Messages.AddWarning("IrrKangSound '" + name + "' was not properly disposed");
            }

            public void Dispose()
            {
                SoundSource.Dispose();
                GC.SuppressFinalize(this);
            }

            public string Name
            {
                get { return name; }
            }
        }
        #endregion
    }
}
