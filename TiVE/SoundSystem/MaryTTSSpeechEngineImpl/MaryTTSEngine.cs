using System.Diagnostics;
using System.IO;
using System.Reflection;
using ikvm.runtime;
using javax.sound.sampled;
using marytts;
using marytts.signalproc.effects;

namespace ProdigalSoftware.TiVE.SoundSystem.MaryTTSSpeechEngineImpl
{
    internal sealed class MaryTTSEngine : ISpeechEngine
    {
        private readonly MaryInterface mary;

        static MaryTTSEngine()
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Startup.addBootClassPathAssembly(Assembly.LoadFile(Path.Combine(assemblyPath, "marytts5.2.dll")));
            Startup.addBootClassPathAssembly(Assembly.LoadFile(Path.Combine(assemblyPath, "marytts-lang-de.dll")));
            Startup.addBootClassPathAssembly(Assembly.LoadFile(Path.Combine(assemblyPath, "marytts-lang-en.dll")));
            Startup.addBootClassPathAssembly(Assembly.LoadFile(Path.Combine(assemblyPath, "marytts-lang-fr.dll")));
            Startup.addBootClassPathAssembly(Assembly.LoadFile(Path.Combine(assemblyPath, "marytts-lang-it.dll")));
            Startup.addBootClassPathAssembly(Assembly.LoadFile(Path.Combine(assemblyPath, "marytts-lang-ru.dll")));
            Startup.addBootClassPathAssembly(Assembly.LoadFile(Path.Combine(assemblyPath, "marytts-lang-sv.dll")));
            Startup.addBootClassPathAssembly(Assembly.LoadFile(Path.Combine(assemblyPath, "marytts-lang-te.dll")));
            Startup.addBootClassPathAssembly(Assembly.LoadFile(Path.Combine(assemblyPath, "marytts-lang-tr.dll")));

            string voicesPath = Path.Combine(assemblyPath, "SoundSystem", "SpeechData");
            foreach (string voiceDll in Directory.EnumerateFiles(voicesPath, "*.dll"))
                Startup.addBootClassPathAssembly(Assembly.LoadFile(voiceDll));
        }

        public MaryTTSEngine()
        {
            mary = new LocalMaryInterface();
            //mary.setLocale(Locale.UK);
            mary.setVoice("dfki-prudence-hsmm");
            //mary.setVoice("dfki-obadiah-hsmm");
        }

        #region Implementation of ISpeechProvider
        public void Dispose()
        {
            // Nothing to do
        }

        public AudioData GetSpeechAudio(string text, SpeechParameters parameters = null)
        {
            mary.setAudioEffects("Stadium(amount:1.00)+Rate(durScale:0.50)");
            AudioInputStream audio = mary.generateAudio(text);
            
            byte[] data = new byte[audio.getFrameLength() * audio.getFormat().getFrameSize() * audio.getFormat().getChannels()];
            
            int totalRead = 0;
            int numRead;
            while ((numRead = audio.read(data, totalRead, data.Length - totalRead)) > 0)
                totalRead += numRead;

            Debug.Assert(data.Length == totalRead);

            return new AudioData(data, new AudioDataFormat((int)audio.getFrameLength(), (int)audio.getFormat().getSampleRate(), audio.getFormat().getChannels()));
        }
        #endregion
    }
}
