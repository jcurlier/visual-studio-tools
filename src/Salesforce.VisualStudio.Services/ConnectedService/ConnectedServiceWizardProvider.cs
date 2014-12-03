using Microsoft.VisualStudio.ConnectedServices;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Salesforce.VisualStudio.Services.ConnectedService
{
    internal class ConnectedServiceWizardProvider : IConnectedServiceProviderWizardUI
    {
        private DesignTimeAuthenticationViewModel designTimeAuthenticationViewModel;
        private RuntimeAuthenticationViewModel runtimeAuthenticationViewModel;
        private ObjectSelectionViewModel objectSelectionViewModel;
        private UserSettings userSettings;
        private ObservableCollection<IConnectedServiceWizardPage> pages;

        public ConnectedServiceWizardProvider(IConnectedServiceProviderHost providerHost)
        {
            this.userSettings = UserSettings.Load();
            this.designTimeAuthenticationViewModel = new DesignTimeAuthenticationViewModel(this.userSettings);
            this.runtimeAuthenticationViewModel = new RuntimeAuthenticationViewModel(this.userSettings, () => this.designTimeAuthenticationViewModel.Authentication.MyDomain);
            this.objectSelectionViewModel = new ObjectSelectionViewModel(providerHost);

            this.designTimeAuthenticationViewModel.PropertyChanged += this.ViewModel_PropertyChanged;
            this.runtimeAuthenticationViewModel.PropertyChanged += this.ViewModel_PropertyChanged;

            this.pages = new ObservableCollection<IConnectedServiceWizardPage>();
            this.pages.Add(new DesignTimeAuthenticationWizardPage(this.designTimeAuthenticationViewModel, this.objectSelectionViewModel, providerHost));
            this.pages.Add(new RuntimeAuthenticationTypeWizardPage(this.runtimeAuthenticationViewModel));
            this.pages.Add(new RuntimeAuthenticationConfigWizardPage(this.runtimeAuthenticationViewModel));
            this.pages.Add(new ObjectSelectionWizardPage(this.objectSelectionViewModel, designTimeAuthenticationViewModel));

            foreach (IWizardPage page in this.Pages)
            {
                page.PropertyChanged += this.WizardPage_PropertyChanged;
            }
        }

        public ObservableCollection<IConnectedServiceWizardPage> Pages
        {
            get { return this.pages; }
        }

        public Task<IConnectedServiceInstance> GetFinishedServiceInstance()
        {
            this.userSettings.Save();

            ConnectedServiceInstance serviceInstance = new ConnectedServiceInstance(
                    this.designTimeAuthenticationViewModel.Authentication,
                    this.runtimeAuthenticationViewModel.RuntimeAuthentication,
                    this.objectSelectionViewModel.GetSelectedObjects());

            serviceInstance.TelemetryHelper.LogInstanceObjectData(this.objectSelectionViewModel);

            return Task.FromResult<IConnectedServiceInstance>(serviceInstance);
        }

        public event EventHandler<EnableNavigationEventArgs> EnableNavigation;

        private void RaiseEnableNavigation(NavigationEnabledState navigationEnabledState)
        {
            if (this.EnableNavigation != null)
            {
                this.EnableNavigation(this, new EnableNavigationEventArgs() { State = navigationEnabledState });
            }
        }

        private void WizardPage_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Constants.IsValidPropertyName)
            {
                IWizardPage senderPage = (IWizardPage)sender;
                int invalidPageIndex = this.Pages.IndexOf(senderPage);

                for (int i = invalidPageIndex + 1; i < this.Pages.Count; i++)
                {
                    IWizardPage page = (IWizardPage)this.Pages[i];
                    page.IsEnabled = senderPage.IsValid;

                    // If an invalid page is reached, then all subsequent pages are currently disabled
                    // and should remain disabled.
                    if (!page.IsValid)
                    {
                        break;
                    }
                }
            }
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == DesignTimeAuthenticationViewModel.IsAuthenticationVerifiedPropertyName
                || e.PropertyName == Constants.IsValidPropertyName)
            {
                this.RefreshFinishButtonState();
            }
        }

        private void RefreshFinishButtonState()
        {
            bool isFinishedEnabled = this.designTimeAuthenticationViewModel.IsAuthenticationVerified
                && this.designTimeAuthenticationViewModel.IsValid
                && this.runtimeAuthenticationViewModel.IsValid;

            this.RaiseEnableNavigation(new NavigationEnabledState(null, null, isFinishedEnabled));
        }
    }
}
