using System.Collections.Generic;
using System.Threading;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVE.SoundSystem.IrrKlangImpl;
using ProdigalSoftware.TiVE.SoundSystem.MaryTTSSpeechEngineImpl;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.SoundSystem
{
    internal sealed class SoundSystem : EngineSystem
    {
        private ISoundEngine soundEngine;
        private ISpeechEngine speechEngine;
        private readonly Queue<ISoundTask> speechTaskQueue = new Queue<ISoundTask>(10);

        private Thread soundThread;
        private volatile bool stopRunning;
        private volatile bool initialized;

        public SoundSystem() : base("Sound")
        {
        }

        #region Implementation of EngineSystem
        public override void Dispose()
        {
            stopRunning = true;
            if (soundThread != null)
                soundThread.Join();
        }

        public override bool Initialize()
        {
            Messages.Print("Initializing sound system...");
            soundThread = new Thread(RunSpeech);
            soundThread.IsBackground = true;
            soundThread.Name = "Sound";
            soundThread.Priority = ThreadPriority.AboveNormal;
            soundThread.Start();
            Messages.AddDoneText();
            return true;
        }

        public override void ChangeScene(Scene oldScene, Scene newScene, bool onSeparateThread)
        {
            if (onSeparateThread)
            {
                while (!initialized)
                    Thread.Sleep(1);
            }

            using (new PerformanceLock(speechTaskQueue))
                speechTaskQueue.Clear();
            soundEngine.StopSounds();

            SayText("System is up and running! I don't know why I talk so fast. Can you understand me?");
        }

        protected override bool UpdateInternal(int ticksSinceLastUpdate, float timeBlendFactor, Scene currentScene)
        {
            return true;
        }
        #endregion

        private void SayText(string text, SpeechParameters parameters = null)
        {
            using (new PerformanceLock(speechTaskQueue))
                speechTaskQueue.Enqueue(new SpeechTask(text, parameters));
        }

        private void RunSpeech()
        {
            soundEngine = new IrrKlangSoundEngine();
            speechEngine = new MaryTTSEngine();
            initialized = true;

            while (!stopRunning)
            {
                ISoundTask task;
                using (new PerformanceLock(speechTaskQueue))
                    task = speechTaskQueue.Count > 0 ? speechTaskQueue.Dequeue() : null;

                soundEngine.UpdateSystem();

                if (task == null)
                {
                    Thread.Sleep(1);
                    continue;
                }

                SpeechTask speechTask = task as SpeechTask;
                if (speechTask != null)
                {
                    soundEngine.DeleteSound("speech");

                    AudioData data = speechEngine.GetSpeechAudio(speechTask.Text, speechTask.Parameters);
                    using (ISound sound = soundEngine.CreateSound("speech", data))
                        soundEngine.PlaySound2D(sound);
                }
            }

            soundEngine.Dispose();
        }

        private sealed class SpeechTask : ISoundTask
        {
            public readonly string Text;
            public readonly SpeechParameters Parameters;

            public SpeechTask(string text, SpeechParameters parameters)
            {
                Text = text;
                Parameters = parameters;
            }
        }

        private interface ISoundTask
        {
        }
    }
}
