using System;
using System.Diagnostics;
using ProdigalSoftware.TiVE.Debugging;

namespace ProdigalSoftware.TiVE.Core
{
    internal abstract class EngineSystemBase : IDisposable
    {
        private static readonly int timeBetweenTimingUpdates = (int)(Stopwatch.Frequency / 2); // 1/2 second

        protected readonly SystemTimingHelper timing = new SystemTimingHelper(2, true);

        private int ticksSinceLastTimingUpdate;

        protected EngineSystemBase(string debuggingName)
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

        public void UpdateTiming(int ticksSinceLastFrame)
        {
            ticksSinceLastTimingUpdate += ticksSinceLastFrame;
            if (ticksSinceLastTimingUpdate >= timeBetweenTimingUpdates)
            {
                timing.UpdateDisplayedTime();
                ticksSinceLastTimingUpdate -= timeBetweenTimingUpdates;
            }
        }
    }
}
