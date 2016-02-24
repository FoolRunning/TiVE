using System;

namespace ProdigalSoftware.TiVE.SpeechSystem
{
    internal interface ISpeechProvider : IDisposable
    {
        void Initialize();

        void SayText(string text, SpeechParameters parameters = null);
    }
}
