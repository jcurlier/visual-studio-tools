using Microsoft.VisualStudio.ConnectedServices;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    internal abstract class SalesforceConnectedServiceWizardPage : ConnectedServiceWizardPage
    {
        protected SalesforceConnectedServiceWizardPage()
        {
        }

        public virtual bool IsValid
        {
            get { return true; }
        }
    }
}
