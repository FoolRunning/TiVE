namespace ProdigalSoftware.TiVE.SoundSystem
{
    internal sealed class AudioDataFormat
    {
        //public readonly bool BigEndian;
        public readonly int FrameCount;
        public readonly int SampleRate;
        public readonly int NumChannels;

        public AudioDataFormat(int frameCount, int sampleRate, int numChannels)
        {
            FrameCount = frameCount;
            SampleRate = sampleRate;
            NumChannels = numChannels;
        }
    }
}
