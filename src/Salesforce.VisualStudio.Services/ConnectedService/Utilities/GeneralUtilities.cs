using System;
using System.Globalization;
using System.Threading;

namespace Salesforce.VisualStudio.Services.ConnectedService.Utilities
{
    /// <summary>
    /// A set of common general purpose utilities.
    /// </summary>
    internal static class GeneralUtilities
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

        public static string GetUniqueSuffix(Func<string, bool> isSuffixUsed)
        {
            string generatedArtifactSuffix = string.Empty;

            int suffixNumber = 1;
            while (isSuffixUsed(generatedArtifactSuffix))
            {
                generatedArtifactSuffix = suffixNumber.ToString(CultureInfo.InvariantCulture);
                suffixNumber++;
            }

            return generatedArtifactSuffix;
        }

        public static string FormatCurrentCulture(this string format, params object[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        public static string FormatInvariantCulture(this string format, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, format, args);
        }
    }
}
