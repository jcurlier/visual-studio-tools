using Microsoft.VisualStudio.ConnectedServices;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    internal abstract class SalesforceConnectedServiceWizardPage : ConnectedServiceWizardPage
    {
        private bool isValid;

        protected SalesforceConnectedServiceWizardPage()
        {
            this.isValid = true;
        }

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
    }
}
