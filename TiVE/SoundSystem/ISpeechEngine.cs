using System;

namespace ProdigalSoftware.TiVE.SoundSystem
{
    internal interface ISpeechEngine : IDisposable
    {
        AudioData GetSpeechAudio(string text, SpeechParameters parameters = null);
    }
}
