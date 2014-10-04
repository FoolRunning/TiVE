using System;
using System.Threading;

namespace ProdigalSoftware.Utils
{
    /// <summary>
    /// Lock replacement for speed-critical code. If a lock takes longer then 1ms to obtain, then a message is written to the debug output.
    /// </summary>
    public struct PerformanceLock : IDisposable
    {
        private readonly object obj;

        /// <summary>
        /// Creates a new performance lock on the specified object. This constructor will block until the lock is gotten whether or not
        /// the lock is gotten in the 1ms threshold.
        /// </summary>
        public PerformanceLock(object obj)
        {
            this.obj = obj;

            bool lockTaken = false;
            Monitor.TryEnter(obj, 1, ref lockTaken);
            if (!lockTaken)
            {
                Console.WriteLine("Lock on " + obj + " took too long!");
                Monitor.Enter(obj); // wait forever now to make sure we get the lock
            }
        }

        /// <summary>
        /// Ends the lock on the object
        /// </summary>
        public void Dispose()
        {
            Monitor.Exit(obj);
        }
    }
}
