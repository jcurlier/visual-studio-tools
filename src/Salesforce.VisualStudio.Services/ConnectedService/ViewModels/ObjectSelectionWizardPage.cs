using Microsoft.VisualStudio.ConnectedServices;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.Views;
using System.Threading.Tasks;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    internal class ObjectSelectionWizardPage : WizardPage<ObjectSelectionViewModel>
    {
        private DesignTimeAuthenticationViewModel designTimeAuthenticationViewModel;
        private DesignTimeAuthentication lastDesignTimeAuthentication;

        public ObjectSelectionWizardPage(
            ObjectSelectionViewModel objectSelectionViewModel,
            DesignTimeAuthenticationViewModel designTimeAuthenticationViewModel)
            : base(objectSelectionViewModel)
        {
            this.View = new ObjectSelectionPage(this.ViewModel);
            this.designTimeAuthenticationViewModel = designTimeAuthenticationViewModel;
        }

        public override string Title
        {
            get { return Resources.ObjectSelectionWizardPage_Title; }
        }

        public override string Description
        {
            get { return Resources.ObjectSelectionWizardPage_Description; }
        }

        public override string Legend
        {
            get { return Resources.ObjectSelectionWizardPage_Legend; }
        }

        public override async Task<NavigationEnabledState> OnPageEntering()
        {
            if (this.lastDesignTimeAuthentication == null ||
                !this.lastDesignTimeAuthentication.Equals(this.designTimeAuthenticationViewModel.Authentication))
            {
                await this.ViewModel.RefreshObjects(this.designTimeAuthenticationViewModel.Authentication);
                this.lastDesignTimeAuthentication = this.designTimeAuthenticationViewModel.Authentication;
            }

            return await base.OnPageEntering();
        }
    }
}
