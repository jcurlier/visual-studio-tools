using Microsoft.VisualStudio.ConnectedServices;
using Microsoft.VisualStudio.Shell;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.Utilities;
using System;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    /// <summary>
    /// A common wizard page base class that contains the commonality between all of the Salesforce wizard pages.
    /// </summary>
    internal abstract class SalesforceConnectedServiceWizardPage : ConnectedServiceWizardPage
    {
        private bool isValid;
        private TelemetryHelper telemetryHelper;

        protected SalesforceConnectedServiceWizardPage(ConnectedServiceProviderHost host, TelemetryHelper telemetryHelper, UserSettings userSettings)
        {
            this.Host = host;
            this.isValid = true;
            this.telemetryHelper = telemetryHelper;
            this.UserSettings = userSettings;
        }

        protected ConnectedServiceProviderHost Host { get; private set; }

        public bool IsValid
        {
            get { return this.isValid; }
            set
            {
                if (this.isValid != value)
                {
                    this.isValid = value;
                    this.OnNotifyPropertyChanged();
                }
            }
        }

        protected UserSettings UserSettings { get; private set; }

        public void NavigateHyperlink(Uri uri)
        {
            string page = uri.AbsoluteUri;
            VsShellUtilities.OpenSystemBrowser(page);

            this.telemetryHelper.TrackLinkClickedEvent(page);
        }
    }
}
