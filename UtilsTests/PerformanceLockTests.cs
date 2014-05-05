using System.Threading;
using NUnit.Framework;
using ProdigalSoftware.Utils;

namespace UtilsTests
{
    /// <summary>
    /// Tests for the PerformanceLock class
    /// </summary>
    [TestFixture]
    public class PerformanceLockTests
    {
        private readonly object syncLock1 = new object();
        private readonly object syncLock2 = new object();

        /// <summary>
        /// Tests that the PerformanceLock gets the monitor lock during it's life
        /// </summary>
        [Test]
        public void GetsLock()
        {
            Assert.That(Monitor.IsEntered(syncLock1), Is.False);
            Assert.That(Monitor.IsEntered(syncLock2), Is.False);
            
            using (new PerformanceLock(syncLock1))
            {
                Assert.That(Monitor.IsEntered(syncLock1), Is.True);
                Assert.That(Monitor.IsEntered(syncLock2), Is.False);
            }

            Assert.That(Monitor.IsEntered(syncLock1), Is.False);
            Assert.That(Monitor.IsEntered(syncLock2), Is.False);
        }
    }
}
