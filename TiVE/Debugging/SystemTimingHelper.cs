using System;
using System.Diagnostics;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Debugging
{
    internal sealed class SystemTimingHelper
    {
        private readonly string formatString;
        private long startTicks;
        private float minTime = float.MaxValue;
        private float maxTime;
        private float totalTime;
        private int dataCount;

        private readonly object syncObj = new object();

        public SystemTimingHelper(int digitsAfterDecimal, bool showMinMax)
        {
            if (showMinMax)
                formatString = "{0:F" + digitsAfterDecimal + "}({1:F" + digitsAfterDecimal + "}-{2:F" + digitsAfterDecimal + "})";
            else
                formatString = "{0:F" + digitsAfterDecimal + "}";
        }

        public string DisplayedValue { get; private set; }

        /// <summary>
        /// Updates the display time with the average of the data points
        /// </summary>
        public void UpdateDisplayedTime()
        {
            using (new PerformanceLock(syncObj))
            {
                DisplayedValue = string.Format(formatString, totalTime / Math.Max(dataCount, 1), minTime, maxTime);
                totalTime = 0;
                dataCount = 0;
                minTime = float.MaxValue;
                maxTime = 0;
            }
        }

        public void StartTime()
        {
            using (new PerformanceLock(syncObj))
                startTicks = Stopwatch.GetTimestamp();
        }

        public void PushTime()
        {
            long endTime = Stopwatch.GetTimestamp();
            float newTime = (endTime - startTicks) * 1000.0f / Stopwatch.Frequency;
            using (new PerformanceLock(syncObj))
            {
                totalTime += newTime;
                dataCount++;

                if (newTime < minTime)
                    minTime = newTime;

                if (newTime > maxTime)
                    maxTime = newTime;
            }
        }

        public override string ToString()
        {
            return DisplayedValue;
        }
    }
}
