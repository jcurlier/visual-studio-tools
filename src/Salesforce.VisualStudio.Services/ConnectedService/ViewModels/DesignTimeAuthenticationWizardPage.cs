using Microsoft.VisualStudio.ConnectedServices;
using Salesforce.Common;
using Salesforce.Common.Models;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.Utilities;
using Salesforce.VisualStudio.Services.ConnectedService.Views;
using System.Threading.Tasks;
using System.Windows;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    internal class DesignTimeAuthenticationWizardPage : WizardPage<DesignTimeAuthenticationViewModel>
    {
        private IConnectedServiceProviderHost providerHost;
        private ObjectSelectionViewModel objectSelectionViewModel;

        public DesignTimeAuthenticationWizardPage(
            DesignTimeAuthenticationViewModel designTimeAuthenticationViewModel,
            ObjectSelectionViewModel objectSelectionViewModel,
            IConnectedServiceProviderHost providerHost)
            : base(designTimeAuthenticationViewModel)
        {
            this.View = new DesignTimeAuthenticationPage(this.ViewModel);
            this.objectSelectionViewModel = objectSelectionViewModel;
            this.providerHost = providerHost;
        }

        public override string Title
        {
            get { return Resources.DesignTimeAuthenticationWizardPage_Title; }
        }

        public override string Description
        {
            get { return Resources.DesignTimeAuthenticationWizardPage_Description; }
        }

        public override string Legend
        {
            get { return Resources.DesignTimeAuthenticationWizardPage_Legend; }
        }

        public override Task<NavigationEnabledState> OnPageEntering()
        {
            this.ViewModel.InitializeAuthenticationOptions();
            return base.OnPageEntering();
        }

        public override async Task<WizardNavigationResult> OnPageLeaving()
        {
            WizardNavigationResult result;

            using (this.providerHost.StartBusyIndicator(Resources.DesignTimeAuthenticationWizardPage_AuthenticatingProgress))
            {
                string error = null;

                if (this.ViewModel.Authentication.RefreshToken == null)
                {
                    // New identity or a existing identity w/no refresh token
                    error = this.AuthenticateUser();

                    if (error == null && this.ViewModel.Authentication.EnvironmentType == EnvironmentType.Custom)
                    {
                        UserSettings.AddToTopOfMruList(this.ViewModel.UserSettings.MruMyDomains, this.ViewModel.Authentication.MyDomain.ToString());
                    }
                }
                else if (this.ViewModel.Authentication.AccessToken == null)
                {
                    // Existing identity w/no access token
                    try
                    {
                        await AuthenticationHelper.RefreshAccessToken(this.ViewModel.Authentication);
                    }
                    catch (ForceException ex)
                    {
                        if (ex.Error == Error.InvalidGrant) // Expired refresh token
                        {
                            this.ViewModel.Authentication.RefreshToken = null;
                            error = this.AuthenticateUser();
                        }
                        else
                        {
                            error = ex.Message;
                        }
                    }
                }
                // else - Existing identity w/access and refresh token

                if (error == null)
                {
                    // Kick off the loading of the Objects so that they will hopefully be loaded before the user navigates to the
                    // Object Selection page.
                    this.objectSelectionViewModel.BeginRefreshObjects(this.ViewModel.Authentication);

                    UserSettings.AddToTopOfMruList(this.ViewModel.UserSettings.MruDesignTimeAuthentications, this.ViewModel.Authentication);
                    result = WizardNavigationResult.Success;
                }
                else
                {
                    result = new WizardNavigationResult() { ErrorMessage = error, ShowMessageBoxOnFailure = true };
                }
            }

            return result;
        }

        private string AuthenticateUser()
        {
            string error = null;

            AuthenticateRedirectHost dialog = new AuthenticateRedirectHost(this.ViewModel);
            dialog.Owner = Window.GetWindow(this.View);
            dialog.ShowDialog();

            if (dialog.AuthenticationError != null)
            {
                error = dialog.AuthenticationError.Message;
            }
            else if (this.ViewModel.Authentication.RefreshToken == null)
            {
                // The user either canceled out of the authentication dialog or did not grant the required permissions.
                error = Resources.DesignTimeAuthenticationWizardPage_UnableToAuthenticateUser;
            }

            return error;
        }
    }
}
