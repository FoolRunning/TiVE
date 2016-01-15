using System;

namespace ProdigalSoftware.TiVE.Debugging
{
    public sealed class ItemCountsHelper
    {
        private readonly string formatString;
        private long totalCount;
        private int minCount = int.MaxValue;
        private int maxCount;
        private int dataCount;

        public ItemCountsHelper(int maxDigits, bool showMinMax)
        {
            if (showMinMax)
                formatString = "{0:D" + maxDigits + "}({1:D" + maxDigits + "}-{2:D" + maxDigits + "})";
            else
                formatString = "{0:D" + maxDigits + "}";
        }

        public string DisplayedValue { get; private set; }

        /// <summary>
        /// Updates the display time with the average of the data points
        /// </summary>
        public void UpdateDisplayedTime()
        {
            DisplayedValue = string.Format(formatString, totalCount / Math.Max(dataCount, 1), minCount, maxCount);
            totalCount = 0;
            dataCount = 0;
            minCount = int.MaxValue;
            maxCount = 0;
        }

        /// <summary>
        /// Adds the specified value as a new data point
        /// </summary>
        public void PushCount(int newCount)
        {
            totalCount += newCount;
            dataCount++;

            if (newCount < minCount)
                minCount = newCount;

            if (newCount > maxCount)
                maxCount = newCount;
        }

        public override string ToString()
        {
            return DisplayedValue;
        }
    }
}
