using System;

namespace ProdigalSoftware.TiVE
{
    /// <summary>
    /// Base class for any custom exceptions that are thrown by TiVE
    /// </summary>
    public class TiVEException : Exception
    {
        /// <summary>
        /// Creates a new TiVEException with the specified message
        /// </summary>
        internal TiVEException(string message) : base(message)
        {
        }
    }
}
