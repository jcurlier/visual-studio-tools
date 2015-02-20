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

        protected SalesforceConnectedServiceWizardPage(SalesforceConnectedServiceWizard wizard)
            : base(wizard)
        {
            this.isValid = true;
        }

        public new SalesforceConnectedServiceWizard Wizard
        {
            get { return (SalesforceConnectedServiceWizard)base.Wizard; }
        }

        public bool IsValid
        {
            get { return this.isValid; }
            set
            {
                if (this.isValid != value)
                {
                    this.isValid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public void NavigateHyperlink(Uri uri)
        {
            string page = uri.AbsoluteUri;
            VsShellUtilities.OpenSystemBrowser(page);

            this.Wizard.TelemetryHelper.TrackLinkClickedEvent(page);
        }
    }
}