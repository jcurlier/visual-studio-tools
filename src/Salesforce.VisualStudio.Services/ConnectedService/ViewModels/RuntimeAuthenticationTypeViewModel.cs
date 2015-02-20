using Microsoft.VisualStudio.ConnectedServices;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.Utilities;
using Salesforce.VisualStudio.Services.ConnectedService.Views;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    /// <summary>
    /// The view model for the Runtime Authentication wizard page.
    /// </summary>
    internal class RuntimeAuthenticationTypeViewModel : SalesforceConnectedServiceWizardPage
    {
        private AuthenticationStrategy runtimeAuthStrategy;

        public RuntimeAuthenticationTypeViewModel(SalesforceConnectedServiceWizard wizard)
            : base(wizard)
        {
            this.runtimeAuthStrategy = AuthenticationStrategy.WebServerFlow;
            this.Title = Resources.RuntimeAuthenticationTypeViewModel_Title;
            this.Description = Resources.RuntimeAuthenticationTypeViewModel_Description;
            this.Legend = Resources.RuntimeAuthenticationTypeViewModel_Legend;
            this.View = new RuntimeAuthenticationTypePage(this);
        }

        public AuthenticationStrategy RuntimeAuthStrategy
        {
            get { return this.runtimeAuthStrategy; }
            set
            {
                this.runtimeAuthStrategy = value;
                this.OnPropertyChanged();
            }
        }
    }
}