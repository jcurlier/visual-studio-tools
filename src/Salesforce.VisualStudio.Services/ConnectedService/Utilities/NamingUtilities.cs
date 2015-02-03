using System;
using System.Globalization;

namespace Salesforce.VisualStudio.Services.ConnectedService.Utilities
{
    /// <summary>
    /// A utility class for generating unique names within the user's project.
    /// </summary>
    internal static class NamingUtilities
    {
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
    }
}
