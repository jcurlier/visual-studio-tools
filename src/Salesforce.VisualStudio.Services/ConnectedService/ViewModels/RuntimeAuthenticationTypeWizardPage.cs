using Salesforce.VisualStudio.Services.ConnectedService.Views;
using System.ComponentModel;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    internal class RuntimeAuthenticationTypeWizardPage : WizardPage<RuntimeAuthenticationViewModel>
    {
        public RuntimeAuthenticationTypeWizardPage(RuntimeAuthenticationViewModel runtimeAuthenticationViewModel)
            : base(runtimeAuthenticationViewModel)
        {
            this.View = new RuntimeAuthenticationTypePage(runtimeAuthenticationViewModel);
        }

        public override string Title
        {
            get { return Resources.RuntimeAuthenticationTypeWizardPage_Title; }
        }

        public override string Description
        {
            get { return Resources.RuntimeAuthenticationTypeWizardPage_Description; }
        }

        public override string Legend
        {
            get { return Resources.RuntimeAuthenticationTypeWizardPage_Legend; }
        }

        public override bool IsValid
        {
            get { return true; }
        }

        public override bool HasErrors
        {
            get { return false; }
        }

        protected override void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Do nothing because any errors on the ViewModel are from the 
            // RuntimeAuthenticationConfigWizardPage instead.
        }
    }
}
