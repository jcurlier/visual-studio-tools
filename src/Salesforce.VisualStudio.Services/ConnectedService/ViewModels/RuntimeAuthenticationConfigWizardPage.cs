using Microsoft.VisualStudio.ConnectedServices;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.Views;
using System;
using System.Threading.Tasks;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    internal class RuntimeAuthenticationConfigWizardPage : WizardPage<RuntimeAuthenticationViewModel>
    {
        public RuntimeAuthenticationConfigWizardPage(RuntimeAuthenticationViewModel runtimeAuthenticationViewModel)
            : base(runtimeAuthenticationViewModel)
        {
            this.View = new RuntimeAuthenticationConfigPage(this.ViewModel);
        }

        public override string Title
        {
            get { return Resources.RuntimeAuthenticationConfigWizardPage_Title; }
        }

        public override string Description
        {
            get { return Resources.RuntimeAuthenticationConfigWizardPage_Description; }
        }

        public override string Legend
        {
            get { return Resources.RuntimeAuthenticationConfigWizardPage_Legend; }
        }

        public override Task<WizardNavigationResult> OnPageLeaving()
        {
            if (this.ViewModel.RuntimeAuthStrategy == AuthenticationStrategy.WebServerFlow)
            {
                Uri myDomain = ((WebServerFlowInfo)this.ViewModel.RuntimeAuthentication).MyDomain;
                if (myDomain != null)
                {
                    UserSettings.AddToTopOfMruList(this.ViewModel.UserSettings.MruMyDomains, myDomain.ToString());
                }
            }

            return base.OnPageLeaving();
        }
    }
}
