using Microsoft.VisualStudio.Shell;

namespace Salesforce.VisualStudio.Services.ConnectedService.Utilities
{
    internal static class HyperlinkHelper
    {
        /// <summary>
        /// Opens the specified page link in the system browser and then logs in the telemetry information.
        /// </summary>
        public static void OpenSystemBrowser(string page)
        {
            VsShellUtilities.OpenSystemBrowser(page);

            TelemetryHelper telemetryHelper = new TelemetryHelper();
            telemetryHelper.LogLinkClickData(page);
        }
    }
}
