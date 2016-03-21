namespace ProdigalSoftware.TiVE.SoundSystem
{
    internal sealed class SpeechParameters
    {
        public readonly string VoiceName;

        public readonly float SpeedPercentage;

        public SpeechParameters(string voiceName, float speedPercentage = 1.0f)
        {
            VoiceName = voiceName;
            SpeedPercentage = speedPercentage;
        }
    }
}
