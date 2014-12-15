using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.Views;
using System;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    internal class RuntimeAuthenticationTypeViewModel : PageViewModel
    {
        public const string RuntimeAuthStrategyPropertyName = "RuntimeAuthStrategy";

        private AuthenticationStrategy runtimeAuthStrategy;

        public RuntimeAuthenticationTypeViewModel()
        {
            this.runtimeAuthStrategy = AuthenticationStrategy.WebServerFlow;
            this.View = new RuntimeAuthenticationTypePage(this);
        }

        public override string Title
        {
            get { return Resources.RuntimeAuthenticationTypeViewModel_Title; }
        }

        public override string Description
        {
            get { return Resources.RuntimeAuthenticationTypeViewModel_Description; }
        }

        public override string Legend
        {
            get { return Resources.RuntimeAuthenticationTypeViewModel_Legend; }
        }

        public AuthenticationStrategy RuntimeAuthStrategy
        {
            get { return this.runtimeAuthStrategy; }
            set
            {
                this.runtimeAuthStrategy = value;
                this.RaisePropertyChanged();
            }
        }
    }
}
