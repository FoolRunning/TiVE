namespace ProdigalSoftware.TiVE.SoundSystem
{
    internal sealed class AudioData
    {
        public readonly byte[] Data;
        public readonly int FrameCount;
        public readonly int SampleRate;
        public readonly int NumChannels;

        public AudioData(byte[] data, int frameCount, int sampleRate, int numChannels)
        {
            Data = data;
            FrameCount = frameCount;
            SampleRate = sampleRate;
            NumChannels = numChannels;
        }
    }
}
