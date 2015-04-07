namespace ProdigalSoftware.TiVE.Core
{
    internal abstract class EngineSystem : EngineSystemBase
    {
        protected EngineSystem(string debuggingName) : base(debuggingName)
        {
        }

        /// <summary>
        /// Do one update step
        /// </summary>
        /// <param name="ticksSinceLastUpdate">The number of ticks (10,000th of a second) since the last update was called</param>
        /// <param name="timeBlendFactor">Factor (between 0 and 1) of the amount of time left in the time step from the previous update</param>
        /// <param name="currentScene">The current scene</param>
        /// <returns>True to keep running, false to quit</returns>
        public bool Update(int ticksSinceLastUpdate, float timeBlendFactor, Scene currentScene)
        {
            timing.StartTime();
            bool keepRunning = UpdateInternal(ticksSinceLastUpdate, timeBlendFactor, currentScene);
            timing.PushTime();

            return keepRunning;
        }

        /// <summary>
        /// Do one update step
        /// </summary>
        /// <param name="ticksSinceLastUpdate">The number of ticks (10,000th of a second) since the last update was called</param>
        /// <param name="timeBlendFactor">Factor (between 0 and 1) of the amount of time left in the time step from the previous update</param>
        /// <param name="currentScene">The current scene</param>
        /// <returns>True to keep running, false to quit</returns>
        protected abstract bool UpdateInternal(int ticksSinceLastUpdate, float timeBlendFactor, Scene currentScene);
    }
}
