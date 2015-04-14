using Microsoft.VisualStudio.ConnectedServices;
using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    /// <summary>
    /// A common wizard page base class that contains the commonality between all of the Salesforce wizard pages.
    /// </summary>
    internal abstract class SalesforceConnectedServiceWizardPage : ConnectedServiceWizardPage
    {
        private bool isValid;

        protected SalesforceConnectedServiceWizardPage()
        {
            this.isValid = true;
        }

        public new SalesforceConnectedServiceWizard Wizard
        {
            get
            {
                Debug.Assert(base.Wizard != null, "The Wizard property is only available after the page has been added to the Wizard.");
                return (SalesforceConnectedServiceWizard)base.Wizard;
            }
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