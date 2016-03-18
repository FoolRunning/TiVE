namespace ProdigalSoftware.TiVE.SoundSystem
{
    internal sealed class AudioData
    {
        public readonly byte[] Data;
        public readonly AudioDataFormat Format;

        public AudioData(byte[] data, AudioDataFormat format)
        {
            Data = data;
            Format = format;
        }
    }
}
