using System;
using System.Diagnostics;

namespace ProdigalSoftware.TiVE.Core
{
    /// <summary>
    /// Base class for engine systems that need to update at regular intervals (i.e. framerate independent).
    /// </summary>
    internal abstract class TimeSlicedEngineSystem : EngineSystem
    {
        #region Member variables
        private readonly float timePerUpdate;
        private readonly int ticksPerUpdate;
        private int ticksSinceLastUpdate;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new TimeSlicedEngineSystem that updates at the specified rate
        /// </summary>
        protected TimeSlicedEngineSystem(string debuggingName, int updatesPerSecond) : base(debuggingName)
        {
            if (updatesPerSecond <= 0)
                throw new ArgumentException("updatesPerSecond must be greater then zero");

            ticksPerUpdate = (int)(Stopwatch.Frequency / updatesPerSecond);
            timePerUpdate = 1.0f / updatesPerSecond;
        }
        #endregion

        #region Abstract methods
        /// <summary>
        /// Do one update step
        /// </summary>
        /// <param name="timeSinceLastUpdate">The time (in seconds) since the last update</param>
        /// <param name="currentScene">The current scene</param>
        /// <returns>True to keep running, false to quit</returns>
        protected abstract bool Update(float timeSinceLastUpdate, Scene currentScene);
        #endregion

        #region Implementation of EngineSystem
        protected override bool UpdateInternal(int ticksSinceLastFrame, Scene currentScene)
        {
            ticksSinceLastUpdate += ticksSinceLastFrame;
            while (ticksSinceLastUpdate >= ticksPerUpdate)
            {
                if (!Update(timePerUpdate, currentScene))
                    return false;
                ticksSinceLastUpdate -= ticksPerUpdate;
            }
            return true;
        }
        #endregion
    }
}
