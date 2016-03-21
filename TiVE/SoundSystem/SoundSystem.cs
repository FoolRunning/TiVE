using System.Collections.Generic;
using System.Threading;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVE.SoundSystem.IrrKlangImpl;
using ProdigalSoftware.TiVE.SoundSystem.MaryTTSSpeechEngineImpl;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.SoundSystem
{
    internal sealed class SoundSystem : EngineSystem
    {
        private const int MaxConcurrentSpeech = 3;

        private ISoundEngine soundEngine;
        private ISpeechEngine speechEngine;
        private readonly Queue<SpeechTask> speechTaskQueue = new Queue<SpeechTask>(10);
        private readonly Queue<ISoundTask> soundTaskQueue = new Queue<ISoundTask>(50);

        private Thread speechSynthesisThread;
        private int speechNumber;
        private volatile bool stopRunning;
        private volatile bool initialized;

        public SoundSystem() : base("Sound")
        {
        }

        #region Implementation of EngineSystem
        public override void Dispose()
        {
            stopRunning = true;
            if (speechSynthesisThread != null)
                speechSynthesisThread.Join();

            soundEngine.Dispose();
        }

        public override bool Initialize()
        {
            soundEngine = new IrrKlangSoundEngine();

            speechSynthesisThread = new Thread(RunSpeechSynthesis);
            speechSynthesisThread.IsBackground = true;
            speechSynthesisThread.Name = "SpeechSynthesis";
            speechSynthesisThread.Priority = ThreadPriority.AboveNormal;
            speechSynthesisThread.Start();
            return true;
        }

        public override void PrepareForScene(string sceneName)
        {
            if (sceneName != "Loading")
            {
                using (new PerformanceLock(speechTaskQueue))
                    speechTaskQueue.Clear();
                using (new PerformanceLock(soundTaskQueue))
                    soundTaskQueue.Clear();

                while (!initialized)
                    Thread.Sleep(1);
            }
        }

        public override void ChangeScene(Scene oldScene, Scene newScene)
        {
            using (new PerformanceLock(speechTaskQueue))
                speechTaskQueue.Clear();
            using (new PerformanceLock(soundTaskQueue))
                soundTaskQueue.Clear();
            soundEngine.StopSounds();

            // German
            //bits1-hsmm
            //bits3-hsmm
            //dfki-pavoque-neutral-hsmm

            // English UK
            //dfki-obadiah-hsmm
            //dfki-poppy-hsmm
            //dfki-prudence-hsmm
            //dfki-spike-hsmm

            // English US
            //cmu-bdl-hsmm
            //cmu-rms-hsmm
            //cmu-slt-hsmm

            // French
            //enst-camille-hsmm
            //enst-dennys-hsmm
            //upmc-jessica-hsmm
            //upmc-pierre-hsmm

            // Italian
            //istc-lucia-hsmm

            SayText("Hi! My name is George. I need to go now.", new SpeechParameters("cmu-rms-hsmm", 2.0f));
            SayText("Hey there! My name is Janice. I like to talk a lot and have a tendency to talk fast. I would like to talk to you for a while. " +
                    "Do you like to talk? I love to talk. I can't stop talking.", new SpeechParameters("cmu-slt-hsmm", 0.6f));
            //SayText("System is up and running. Can you understand me?", new SpeechParameters("dfki-obadiah-hsmm"));
        }

        protected override bool UpdateInternal(int ticksSinceLastUpdate, float timeBlendFactor, Scene currentScene)
        {
            soundEngine.UpdateSystem();

            ISoundTask task;
            using (new PerformanceLock(soundTaskQueue))
                task = soundTaskQueue.Count > 0 ? soundTaskQueue.Dequeue() : null;

            SoundFromAudioData soundFromDataTask = task as SoundFromAudioData;
            if (soundFromDataTask != null)
            {
                soundEngine.DeleteSound(soundFromDataTask.SoundName);
                using (ISound sound = soundEngine.CreateSound(soundFromDataTask.SoundName, soundFromDataTask.Data))
                    soundEngine.PlaySound2D(sound);
            }

            return true;
        }
        #endregion

        private void SayText(string text, SpeechParameters parameters)
        {
            using (new PerformanceLock(speechTaskQueue))
                speechTaskQueue.Enqueue(new SpeechTask(text, parameters));
        }

        private void RunSpeechSynthesis()
        {
            speechEngine = new MaryTTSEngine();
            initialized = true;

            while (!stopRunning)
            {
                SpeechTask speechTask;
                using (new PerformanceLock(speechTaskQueue))
                    speechTask = speechTaskQueue.Count > 0 ? speechTaskQueue.Dequeue() : null;

                if (speechTask == null)
                {
                    Thread.Sleep(1);
                    continue;
                }

                AudioData data = speechEngine.GetSpeechAudio(speechTask.Text, speechTask.Parameters);
                if (data != null)
                {
                    speechNumber = ++speechNumber % MaxConcurrentSpeech;
                    string speechName = "speech" + speechNumber;
                    using (new PerformanceLock(soundTaskQueue))
                        soundTaskQueue.Enqueue(new SoundFromAudioData(speechName, data));
                }
            }

            speechEngine.Dispose();
        }

        private sealed class SpeechTask
        {
            public readonly string Text;
            public readonly SpeechParameters Parameters;

            public SpeechTask(string text, SpeechParameters parameters)
            {
                Text = text;
                Parameters = parameters;
            }
        }

        private sealed class SoundFromAudioData : ISoundTask
        {
            public readonly string SoundName;
            public readonly AudioData Data;

            public SoundFromAudioData(string soundName, AudioData data)
            {
                SoundName = soundName;
                Data = data;
            }
        }

        private interface ISoundTask
        {
        }
    }
}
