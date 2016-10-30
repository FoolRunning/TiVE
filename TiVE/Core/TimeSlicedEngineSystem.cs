namespace ProdigalSoftware.TiVE.Core
{
    /// <summary>
    /// Base class for engine systems that need to update at regular intervals (i.e. framerate independent).
    /// </summary>
    internal abstract class TimeSlicedEngineSystem : EngineSystem
    {
        protected TimeSlicedEngineSystem(string debuggingName) : base(debuggingName)
        {
        }

        /// <summary>
        /// Do one update step
        /// </summary>
        /// <param name="timeSinceLastUpdate">The time (in seconds) since the last update</param>
        /// <param name="currentScene">The current scene</param>
        /// <returns>True to keep running, false to quit</returns>
        public bool Update(float timeSinceLastUpdate, Scene currentScene)
        {
            timing.StartTime();
            bool keepRunning = UpdateInternal(timeSinceLastUpdate, currentScene);
            timing.PushTime();

            return keepRunning;
        }

        /// <summary>
        /// Do one update step
        /// </summary>
        /// <param name="timeSinceLastUpdate">The time (in seconds) since the last update</param>
        /// <param name="currentScene">The current scene</param>
        /// <returns>True to keep running, false to quit</returns>
        protected abstract bool UpdateInternal(float timeSinceLastUpdate, Scene currentScene);
    }
}
