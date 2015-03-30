using System;
using System.Diagnostics;
using ProdigalSoftware.TiVE.Debugging;

namespace ProdigalSoftware.TiVE.Core
{
    internal abstract class EngineSystem : IDisposable
    {
        private static readonly int timeBetweenTimingUpdates = (int)(Stopwatch.Frequency / 2); // 1/2 second

        private readonly SystemTimingHelper timing = new SystemTimingHelper(2, true);
        private int ticksSinceLastTimingUpdate;

        protected EngineSystem(string debuggingName)
        {
            DebuggingName = debuggingName;
        }

        public string TimingInfo
        {
            get { return timing.DisplayedValue; }
        }

        public string DebuggingName { get; private set; }

        public abstract void Dispose();

        public abstract bool Initialize();

        public void Update(int ticksSinceLastFrame, Scene currentScene)
        {
            ticksSinceLastTimingUpdate += ticksSinceLastFrame;
            if (ticksSinceLastTimingUpdate >= timeBetweenTimingUpdates)
            {
                timing.UpdateDisplayedTime();
                ticksSinceLastTimingUpdate -= timeBetweenTimingUpdates;
            }

            timing.StartTime();
            UpdateInternal(ticksSinceLastFrame, currentScene);
            timing.PushTime();
        }

        protected abstract void UpdateInternal(int ticksSinceLastFrame, Scene currentScene);
    }
}
