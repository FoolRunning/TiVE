using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using ikvm.runtime;
using javax.sound.sampled;
using marytts;
using marytts.server;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE.SoundSystem.MaryTTSSpeechEngineImpl
{
    internal sealed class MaryTTSEngine : ISpeechEngine
    {
        private readonly StringBuilder effectParamsBldr = new StringBuilder();
        private readonly MaryInterface mary;

        static MaryTTSEngine()
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            java.lang.System.setProperty("mary.base", assemblyPath);

            Startup.addBootClassPathAssembly(Assembly.LoadFile(Path.Combine(assemblyPath, "marytts5.2.dll")));
            Startup.addBootClassPathAssembly(Assembly.LoadFile(Path.Combine(assemblyPath, "marytts-lang-de.dll")));
            Startup.addBootClassPathAssembly(Assembly.LoadFile(Path.Combine(assemblyPath, "marytts-lang-en.dll")));
            Startup.addBootClassPathAssembly(Assembly.LoadFile(Path.Combine(assemblyPath, "marytts-lang-fr.dll")));
            Startup.addBootClassPathAssembly(Assembly.LoadFile(Path.Combine(assemblyPath, "marytts-lang-it.dll")));
            Startup.addBootClassPathAssembly(Assembly.LoadFile(Path.Combine(assemblyPath, "marytts-lang-ru.dll")));
            Startup.addBootClassPathAssembly(Assembly.LoadFile(Path.Combine(assemblyPath, "marytts-lang-sv.dll")));
            Startup.addBootClassPathAssembly(Assembly.LoadFile(Path.Combine(assemblyPath, "marytts-lang-te.dll")));
            Startup.addBootClassPathAssembly(Assembly.LoadFile(Path.Combine(assemblyPath, "marytts-lang-tr.dll")));
            
            string voicesPath = Path.Combine(assemblyPath, "lib");
            foreach (string voiceDll in Directory.EnumerateFiles(voicesPath, "*.dll"))
                Startup.addBootClassPathAssembly(Assembly.LoadFile(voiceDll));
        }

        public MaryTTSEngine()
        {
            Mary.startup();
            mary = new LocalMaryInterface();
        }

        #region Implementation of ISpeechProvider
        public void Dispose()
        {
            Mary.shutdown();
        }

        public AudioData GetSpeechAudio(string text, SpeechParameters parameters)
        {
            if (parameters == null)
            {
                Messages.AddWarning("No speech parameters when saying text '" + text + "'");
                return null;
            }

            if (!SetSpeechSettings(parameters))
                return null;

            AudioInputStream audio = mary.generateAudio(text);
            
            byte[] data = new byte[audio.getFrameLength() * audio.getFormat().getFrameSize() * audio.getFormat().getChannels()];
            
            int totalRead = 0;
            int numRead;
            while ((numRead = audio.read(data, totalRead, data.Length - totalRead)) > 0)
                totalRead += numRead;

            Debug.Assert(data.Length == totalRead);

            return new AudioData(data, (int)audio.getFrameLength(), (int)audio.getFormat().getSampleRate(), audio.getFormat().getChannels());
        }
        #endregion

        private bool SetSpeechSettings(SpeechParameters parameters)
        {
            // Set settings that are missing from some HSMM voices that make them sound better
            // ENHANCE: Do this only once per voice
            java.lang.System.setProperty("voice." + parameters.VoiceName + ".useGV", "true");
            java.lang.System.setProperty("voice." + parameters.VoiceName + ".maxMgcGvIter", "300");
            java.lang.System.setProperty("voice." + parameters.VoiceName + ".maxLf0GvIter", "300");
            java.lang.System.setProperty("voice." + parameters.VoiceName + ".maxStrGvIter", "300");

            try
            {
                mary.setVoice(parameters.VoiceName);

                effectParamsBldr.Length = 0;
                if (parameters.SpeedPercentage != 1.0f)
                    effectParamsBldr.AppendFormat("Rate(durScale:{0})+", parameters.SpeedPercentage.ToString("f2", CultureInfo.InvariantCulture));
                //effectParamsBldr.Append("JetPilot");
                mary.setAudioEffects(effectParamsBldr.ToString());
            }
            catch (Exception e)
            {
                Messages.AddWarning(e.Message);
                return false;
            }

            return true;
        }
    }
}
