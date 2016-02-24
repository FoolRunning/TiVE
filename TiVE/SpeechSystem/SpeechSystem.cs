using System.Collections.Generic;
using System.Threading;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.SpeechSystem
{
    internal sealed class SpeechSystem : EngineSystem
    {
        private readonly ISpeechProvider speechProvider = new FestivalSpeechProvider();
        private readonly Queue<SpeechTask> speechTaskQueue = new Queue<SpeechTask>(10);

        private Thread speechThread;
        private volatile bool stopRunning;


        public SpeechSystem() : base("SpeechSystem")
        {
        }

        #region Implementation of EngineSystem
        public override void Dispose()
        {
            stopRunning = true;
            speechThread.Join();
        }

        public override bool Initialize()
        {
            Messages.Print("Initializing speech system...");
            speechThread = new Thread(RunSpeech);
            speechThread.IsBackground = true;
            speechThread.Priority = ThreadPriority.AboveNormal;
            speechThread.Start();
            Messages.AddDoneText();

            SayText("System is up and running! I don't know why I talk so fast. Can you understand me?", new SpeechParameters("kal_diphone" /*"cmu_us_slt_arctic_hts"*/));
            return true;
        }

        public override void ChangeScene(Scene newScene)
        {
            
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
            speechProvider.Initialize();

            while (!stopRunning)
            {
                SpeechTask task;
                using (new PerformanceLock(speechTaskQueue))
                    task = speechTaskQueue.Count > 0 ? speechTaskQueue.Dequeue() : null;

                if (task == null)
                {
                    Thread.Sleep(1);
                    continue;
                }

                speechProvider.SayText(task.Text, task.Parameters);
            }

            speechProvider.Dispose();
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
    }
}
