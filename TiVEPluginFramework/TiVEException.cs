using System;
using JetBrains.Annotations;
using MoonSharp.Interpreter;

namespace ProdigalSoftware.TiVEPluginFramework
{
    /// <summary>
    /// Base class for any custom exceptions that are thrown by TiVE
    /// </summary>
    [MoonSharpUserData(AccessMode = InteropAccessMode.HideMembers)]
    internal class TiVEException : Exception
    {
        /// <summary>
        /// Creates a new TiVEException with the specified message
        /// </summary>
        [PublicAPI]
        public TiVEException(string message) : base(message)
        {
        }
    }

    [MoonSharpUserData(AccessMode = InteropAccessMode.HideMembers)]
    internal class FileTooNewException : TiVEException
    {
        public FileTooNewException(string objectName) : base(objectName + " was saved with a newer version of TiVE")
        {
        }
    }
}
