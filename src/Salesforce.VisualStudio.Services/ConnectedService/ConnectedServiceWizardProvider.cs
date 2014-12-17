using Microsoft.VisualStudio.ConnectedServices;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Salesforce.VisualStudio.Services.ConnectedService
{
    internal class ConnectedServiceWizardProvider : IConnectedServiceProviderWizardUI
    {
        private DesignTimeAuthenticationViewModel designTimeAuthenticationViewModel;
        private RuntimeAuthenticationTypeViewModel runtimeAuthenticationTypeViewModel;
        private RuntimeAuthenticationConfigViewModel runtimeAuthenticationConfigViewModel;
        private ObjectSelectionViewModel objectSelectionViewModel;
        private UserSettings userSettings;
        private ObservableCollection<IConnectedServiceWizardPage> pages;

        public ConnectedServiceWizardProvider(IConnectedServiceProviderHost providerHost)
        {
            this.userSettings = UserSettings.Load();
            this.designTimeAuthenticationViewModel = new DesignTimeAuthenticationViewModel(this.userSettings, providerHost);
            this.designTimeAuthenticationViewModel.PageLeaving += DesignTimeAuthenticationViewModel_PageLeaving;
            this.runtimeAuthenticationTypeViewModel = new RuntimeAuthenticationTypeViewModel();
            this.runtimeAuthenticationConfigViewModel = new RuntimeAuthenticationConfigViewModel(this.userSettings, () => this.designTimeAuthenticationViewModel.Authentication.MyDomain);
            this.runtimeAuthenticationConfigViewModel.RuntimeAuthStrategy = this.runtimeAuthenticationTypeViewModel.RuntimeAuthStrategy;
            this.objectSelectionViewModel = new ObjectSelectionViewModel(providerHost);

            this.pages = new ObservableCollection<IConnectedServiceWizardPage>();
            this.pages.Add(this.designTimeAuthenticationViewModel);
            this.pages.Add(this.runtimeAuthenticationTypeViewModel);
            this.pages.Add(this.runtimeAuthenticationConfigViewModel);
            this.pages.Add(this.objectSelectionViewModel);

            foreach (PageViewModel page in this.Pages)
            {
                page.PropertyChanged += this.PageViewModel_PropertyChanged;
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
                    this.runtimeAuthenticationConfigViewModel.RuntimeAuthentication,
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

        private void DesignTimeAuthenticationViewModel_PageLeaving(object sender, EventArgs e)
        {
            // Kick off the loading of the Objects so that they will hopefully be loaded before the user navigates to the
            // Object Selection page.
            this.objectSelectionViewModel.BeginRefreshObjects(this.designTimeAuthenticationViewModel.Authentication);
        }

        private void PageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == CommonViewModel.IsValidPropertyName)
            {
                PageViewModel senderPage = (PageViewModel)sender;
                int invalidPageIndex = this.Pages.IndexOf(senderPage);

                for (int i = invalidPageIndex + 1; i < this.Pages.Count; i++)
                {
                    PageViewModel page = (PageViewModel)this.Pages[i];
                    page.IsEnabled = senderPage.IsValid;

                    // If an invalid page is reached, then all subsequent pages are currently disabled
                    // and should remain disabled.
                    if (!page.IsValid)
                    {
                        break;
                    }
                }

                this.RefreshFinishButtonState();
            }
            else if (sender is RuntimeAuthenticationTypeViewModel && e.PropertyName == RuntimeAuthenticationTypeViewModel.RuntimeAuthStrategyPropertyName)
            {
                this.runtimeAuthenticationConfigViewModel.RuntimeAuthStrategy = this.runtimeAuthenticationTypeViewModel.RuntimeAuthStrategy;
            }
        }

        private void RefreshFinishButtonState()
        {
            bool isFinishedEnabled = this.Pages.Cast<PageViewModel>().All(p => p.IsValid);
            this.RaiseEnableNavigation(new NavigationEnabledState(null, null, isFinishedEnabled));
        }
    }
}
