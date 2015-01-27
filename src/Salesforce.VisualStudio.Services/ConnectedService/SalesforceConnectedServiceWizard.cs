using Microsoft.VisualStudio.ConnectedServices;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.Utilities;
using Salesforce.VisualStudio.Services.ConnectedService.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Salesforce.VisualStudio.Services.ConnectedService
{
    internal class SalesforceConnectedServiceWizard : ConnectedServiceWizard
    {
        private DesignTimeAuthenticationViewModel designTimeAuthenticationViewModel;
        private RuntimeAuthenticationTypeViewModel runtimeAuthenticationTypeViewModel;
        private RuntimeAuthenticationConfigViewModel runtimeAuthenticationConfigViewModel;
        private ObjectSelectionViewModel objectSelectionViewModel;
        private UserSettings userSettings;
        private TelemetryHelper telemetryHelper;

        public SalesforceConnectedServiceWizard(ConnectedServiceProviderHost host)
        {
            this.telemetryHelper = new TelemetryHelper(host.ProjectHierarchy);
            this.telemetryHelper.TrackWizardStartedEvent();

            this.userSettings = UserSettings.Load();
            this.designTimeAuthenticationViewModel = new DesignTimeAuthenticationViewModel(host, this.telemetryHelper, this.userSettings);
            this.designTimeAuthenticationViewModel.PageLeaving += DesignTimeAuthenticationViewModel_PageLeaving;
            this.runtimeAuthenticationTypeViewModel = new RuntimeAuthenticationTypeViewModel(host, this.telemetryHelper, this.userSettings);
            this.runtimeAuthenticationConfigViewModel = new RuntimeAuthenticationConfigViewModel(
                host, this.telemetryHelper, this.userSettings, () => this.designTimeAuthenticationViewModel.Authentication.MyDomain);
            this.runtimeAuthenticationConfigViewModel.RuntimeAuthStrategy = this.runtimeAuthenticationTypeViewModel.RuntimeAuthStrategy;
            this.objectSelectionViewModel = new ObjectSelectionViewModel(host, this.telemetryHelper, this.userSettings);

            this.Pages.Add(this.designTimeAuthenticationViewModel);
            this.Pages.Add(this.runtimeAuthenticationTypeViewModel);
            this.Pages.Add(this.runtimeAuthenticationConfigViewModel);
            this.Pages.Add(this.objectSelectionViewModel);

            foreach (SalesforceConnectedServiceWizardPage page in this.Pages)
            {
                page.PropertyChanged += this.PageViewModel_PropertyChanged;
            }
        }

        public override Task<ConnectedServiceInstance> GetFinishedServiceInstanceAsync()
        {
            this.userSettings.Save();

            SalesforceConnectedServiceInstance serviceInstance = new SalesforceConnectedServiceInstance(
                    this.designTimeAuthenticationViewModel.Authentication,
                    this.runtimeAuthenticationConfigViewModel.RuntimeAuthentication,
                    this.objectSelectionViewModel.GetSelectedObjects(),
                    this.telemetryHelper);

            this.telemetryHelper.TrackWizardFinishedEvent(serviceInstance, this.objectSelectionViewModel);

            return Task.FromResult<ConnectedServiceInstance>(serviceInstance);
        }

        private void DesignTimeAuthenticationViewModel_PageLeaving(object sender, EventArgs e)
        {
            // Kick off the loading of the Objects so that they will hopefully be loaded before the user navigates to the
            // Object Selection page.
            this.objectSelectionViewModel.BeginRefreshObjects(this.designTimeAuthenticationViewModel.Authentication);
        }

        private void PageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Constants.IsValidPropertyName)
            {
                SalesforceConnectedServiceWizardPage senderPage = (SalesforceConnectedServiceWizardPage)sender;
                int invalidPageIndex = this.Pages.IndexOf(senderPage);

                for (int i = invalidPageIndex + 1; i < this.Pages.Count; i++)
                {
                    SalesforceConnectedServiceWizardPage page = (SalesforceConnectedServiceWizardPage)this.Pages[i];
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
            bool isFinishedEnabled = this.Pages.Cast<SalesforceConnectedServiceWizardPage>().All(p => p.IsValid);
            this.OnEnableNavigation(new NavigationEnabledState(null, null, isFinishedEnabled));
        }
    }
}
