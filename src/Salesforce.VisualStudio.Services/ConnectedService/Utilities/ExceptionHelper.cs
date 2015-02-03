using System;
using System.Threading;

namespace Salesforce.VisualStudio.Services.ConnectedService.Utilities
{
    /// <summary>
    /// A utility class for working with exceptions.
    /// </summary>
    internal static class ExceptionHelper
    {
        /// <summary>
        /// If the given exception is "ignorable under some circumstances" return false.
        /// Otherwise it's "really bad", and return true.
        /// This makes it possible to catch(Exception ex) without catching disasters.
        /// </summary>
        public static bool IsCriticalException(Exception e)
        {
            return e is StackOverflowException
                || e is OutOfMemoryException
                || e is ThreadAbortException
                || e is AccessViolationException;
        }
    }
}
