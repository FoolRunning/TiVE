using ProdigalSoftware.TiVE.Debugging;

namespace ProdigalSoftware.TiVE.Core
{
    internal abstract class EngineSystem
    {
        private const float TimeBetweenTimingUpdates = 0.5f; // 1/2 second

        private readonly SystemTimingHelper timing = new SystemTimingHelper(2, true);
        private float timeSinceLastTimingUpdate;

        public abstract string DebuggingName { get; }

        public abstract void Initialize();

        public void Update(float timeDelta)
        {
            timeSinceLastTimingUpdate += timeDelta;
            if (timeSinceLastTimingUpdate >= TimeBetweenTimingUpdates)
            {
                timing.UpdateDisplayedTime();
                timeSinceLastTimingUpdate -= 0.5f;
            }

            timing.StartTime();
            UpdateInternal(timeDelta);
            timing.PushTime();
        }

        protected abstract void UpdateInternal(float timeDelta);
    }
}
