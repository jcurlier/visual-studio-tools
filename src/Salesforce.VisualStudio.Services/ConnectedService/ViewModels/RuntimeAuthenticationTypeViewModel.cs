using Microsoft.VisualStudio.ConnectedServices;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.Utilities;
using Salesforce.VisualStudio.Services.ConnectedService.Views;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    internal class RuntimeAuthenticationTypeViewModel : SalesforceConnectedServiceWizardPage
    {
        public const string RuntimeAuthStrategyPropertyName = "RuntimeAuthStrategy";

        private AuthenticationStrategy runtimeAuthStrategy;

        public RuntimeAuthenticationTypeViewModel(ConnectedServiceProviderHost host, TelemetryHelper telemetryHelper, UserSettings userSettings)
            : base(host, telemetryHelper, userSettings)
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
                this.OnNotifyPropertyChanged();
            }
        }
    }
}
