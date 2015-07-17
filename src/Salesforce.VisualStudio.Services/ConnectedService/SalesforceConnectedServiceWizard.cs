using Microsoft.VisualStudio.ConnectedServices;
using Microsoft.VisualStudio.LanguageServices;
using Salesforce.VisualStudio.Services.ConnectedService.CodeModel;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.Utilities;
using Salesforce.VisualStudio.Services.ConnectedService.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Salesforce.VisualStudio.Services.ConnectedService
{
    /// <summary>
    /// A ConnectedServiceWizard that provides a wizard that is used to gather various configuration information
    /// about how the end user would like to configure their project to connect to a Salesforce service.
    /// </summary>
    internal class SalesforceConnectedServiceWizard : ConnectedServiceWizard
    {
        private DesignTimeAuthenticationViewModel designTimeAuthenticationViewModel;
        private RuntimeAuthenticationTypeViewModel runtimeAuthenticationTypeViewModel;
        private RuntimeAuthenticationConfigViewModel runtimeAuthenticationConfigViewModel;
        private ObjectSelectionViewModel objectSelectionViewModel;
        private UserSettings userSettings;
        private TelemetryHelper telemetryHelper;
        private ConnectedServiceProviderContext context;
        private VisualStudioWorkspace visualStudioWorkspace;
        private DesignerData designerData;

        public SalesforceConnectedServiceWizard(ConnectedServiceProviderContext context, VisualStudioWorkspace visualStudioWorkspace)
        {
            this.context = context;
            this.visualStudioWorkspace = visualStudioWorkspace;

            this.telemetryHelper = new TelemetryHelper(context);
            this.telemetryHelper.TrackWizardStartedEvent();

            this.userSettings = UserSettings.Load(context.Logger);

            this.InitializePages();
        }

        public ConnectedServiceProviderContext Context
        {
            get { return this.context; }
        }

        public VisualStudioWorkspace VisualStudioWorkspace
        {
            get { return this.visualStudioWorkspace; }
        }

        public DesignerData DesignerData
        {
            get
            {
                if (this.designerData == null)
                {
                    this.designerData = this.Context.GetExtendedDesignerData<DesignerData>();

                    if (this.designerData == null)
                    {
                        this.designerData = new DesignerData();
                    }

                    if (this.Context.IsUpdating)
                    {
                        this.designerData.ServiceName = this.Context.UpdateContext.ServiceFolder.Text;
                    }
                }

                return this.designerData;
            }
        }

        public TelemetryHelper TelemetryHelper
        {
            get { return this.telemetryHelper; }
        }

        public UserSettings UserSettings
        {
            get { return this.userSettings; }
        }

        public override Task<ConnectedServiceInstance> GetFinishedServiceInstanceAsync()
        {
            this.userSettings.Save();

            SalesforceConnectedServiceInstance serviceInstance = new SalesforceConnectedServiceInstance();
            serviceInstance.DesignerData = this.DesignerData;
            serviceInstance.DesignTimeAuthentication = this.designTimeAuthenticationViewModel.Authentication;
            serviceInstance.RuntimeAuthentication = this.runtimeAuthenticationConfigViewModel.RuntimeAuthentication;
            serviceInstance.SelectedObjects = this.objectSelectionViewModel.GetSelectedObjects();
            serviceInstance.TelemetryHelper = this.telemetryHelper;

            this.telemetryHelper.TrackWizardFinishedEvent(serviceInstance, this.objectSelectionViewModel);

            return Task.FromResult<ConnectedServiceInstance>(serviceInstance);
        }

        private void InitializePages()
        {
            this.designTimeAuthenticationViewModel = new DesignTimeAuthenticationViewModel();
            this.designTimeAuthenticationViewModel.PageLeaving += DesignTimeAuthenticationViewModel_PageLeaving;
            this.runtimeAuthenticationTypeViewModel = new RuntimeAuthenticationTypeViewModel();
            this.runtimeAuthenticationConfigViewModel = new RuntimeAuthenticationConfigViewModel(
                () => this.designTimeAuthenticationViewModel.Authentication?.MyDomain);
            this.runtimeAuthenticationConfigViewModel.RuntimeAuthStrategy = this.runtimeAuthenticationTypeViewModel.RuntimeAuthStrategy;
            this.objectSelectionViewModel = new ObjectSelectionViewModel();

            this.Pages.Add(this.designTimeAuthenticationViewModel);
            this.Pages.Add(this.runtimeAuthenticationTypeViewModel);
            this.Pages.Add(this.runtimeAuthenticationConfigViewModel);
            this.Pages.Add(this.objectSelectionViewModel);

            // Some logic within the view models depend on the Wizard.  This logic must be invoked after the pages have
            // been added to the wizard in order for the Wizard property to be available on the ConnectedServiceWizardPage.
            if (this.Context.IsUpdating)
            {
                this.RestoreAuthenticationSettings();
            }

            foreach (SalesforceConnectedServiceWizardPage page in this.Pages)
            {
                page.PropertyChanged += this.PageViewModel_PropertyChanged;
            }
        }

        /// <summary>
        /// Detects which authentication type/settings the service was previously configured with and
        /// initializes the view models accordingly.
        /// </summary>
        private void RestoreAuthenticationSettings()
        {
            using (XmlConfigHelper configHelper = context.CreateReadOnlyXmlConfigHelper())
            {
                ConfigurationKeyNames configKeys = new ConfigurationKeyNames(this.DesignerData.ServiceName);
                if (configHelper.GetAppSetting(configKeys.UserName) != null)
                {
                    this.runtimeAuthenticationTypeViewModel.RuntimeAuthStrategy = AuthenticationStrategy.UserNamePassword;
                }
                else
                {
                    this.runtimeAuthenticationTypeViewModel.RuntimeAuthStrategy = AuthenticationStrategy.WebServerFlow;

                    string myDomain = configHelper.GetAppSetting(configKeys.Domain);
                    this.runtimeAuthenticationConfigViewModel.IsCustomDomain =
                        myDomain != null && !string.Equals(myDomain, Constants.ProductionDomainUrl, StringComparison.OrdinalIgnoreCase);
                    if (this.runtimeAuthenticationConfigViewModel.IsCustomDomain)
                    {
                        this.runtimeAuthenticationConfigViewModel.MyDomainViewModel.MyDomain = myDomain;
                    }
                }
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
            if (e.PropertyName == nameof(SalesforceConnectedServiceWizardPage.IsValid))
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

                this.IsFinishEnabled = this.Pages.Cast<SalesforceConnectedServiceWizardPage>().All(p => p.IsValid);
            }
            else if (sender is RuntimeAuthenticationTypeViewModel && e.PropertyName == nameof(RuntimeAuthenticationTypeViewModel.RuntimeAuthStrategy))
            {
                this.runtimeAuthenticationConfigViewModel.RuntimeAuthStrategy = this.runtimeAuthenticationTypeViewModel.RuntimeAuthStrategy;
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.telemetryHelper?.Dispose();
                    this.designTimeAuthenticationViewModel?.Dispose();
                    this.runtimeAuthenticationTypeViewModel?.Dispose();
                    this.runtimeAuthenticationConfigViewModel?.Dispose();
                    this.objectSelectionViewModel?.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}