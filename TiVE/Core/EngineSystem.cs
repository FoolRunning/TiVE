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

        /// <summary>
        /// Do one update step
        /// </summary>
        /// <param name="ticksSinceLastUpdate">The number of ticks (10,000th of a second) since the last update was called</param>
        /// <param name="currentScene">The current scene</param>
        /// <returns>True to keep running, false to quit</returns>
        protected abstract bool UpdateInternal(int ticksSinceLastUpdate, Scene currentScene);
    }
}
