using Microsoft.VisualStudio.ConnectedServices;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.Views;
using System.Threading.Tasks;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    internal class ObjectSelectionWizardPage : WizardPage<ObjectSelectionViewModel>
    {
        private DesignTimeAuthenticationViewModel designTimeAuthenticationViewModel;

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
            await this.ViewModel.WaitOnRefreshObjects();

            return await base.OnPageEntering();
        }
    }
}
