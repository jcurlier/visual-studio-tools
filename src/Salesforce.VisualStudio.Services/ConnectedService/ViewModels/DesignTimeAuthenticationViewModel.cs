using Microsoft.VisualStudio.ConnectedServices;
using Newtonsoft.Json;
using Salesforce.Common;
using Salesforce.Common.Models;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.Utilities;
using Salesforce.VisualStudio.Services.ConnectedService.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    internal class DesignTimeAuthenticationViewModel : SalesforceConnectedServiceWizardPage
    {
        private MyDomainViewModel myDomainViewModel;
        private DesignTimeAuthentication authentication;
        private Environment[] environments;
        private ConnectedServiceProviderHost host;
        private bool isFirstUse;
        private UserSettings userSettings;

        public DesignTimeAuthenticationViewModel(UserSettings userSettings, ConnectedServiceProviderHost host)
        {
            this.isFirstUse = true;
            this.userSettings = userSettings;
            this.host = host;

            this.environments = new Environment[] {
                new Environment()
                    {
                        DisplayName = Resources.DesignTimeAuthenticationViewModel_Environment_Production,
                        Type = EnvironmentType.Production,
                    },
                new Environment()
                    {
                        DisplayName = Resources.DesignTimeAuthenticationViewModel_Environment_Sandbox,
                        Type = EnvironmentType.Sandbox,
                    },
                new Environment()
                    {
                        DisplayName = Resources.DesignTimeAuthenticationViewModel_Environment_Custom,
                        Type = EnvironmentType.Custom
                    }
            };

            this.Title = Resources.DesignTimeAuthenticationViewModel_Title;
            this.Description = Resources.DesignTimeAuthenticationViewModel_Description;
            this.Legend = Resources.DesignTimeAuthenticationViewModel_Legend;
            this.View = new DesignTimeAuthenticationPage(this);
        }

        private Uri RedirectUrl
        {
            get { return new Uri(this.Authentication.Domain, "/services/oauth2/success"); }
        }

        public Uri AuthorizeUrl
        {
            get
            {
                string relativeUri = string.Format(
                    CultureInfo.InvariantCulture,
                    "/services/oauth2/authorize?response_type=token&client_id={0}&redirect_uri={1}&display=popup",
                    HttpUtility.UrlEncode(Constants.VisualStudioConnectedAppClientId),
                    HttpUtility.UrlEncode(this.RedirectUrl.ToString()));

                if (!this.Authentication.IsNewIdentity)
                {
                    relativeUri = string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}&login_hint={1}",
                        relativeUri,
                        this.Authentication.UserName);
                }

                return new Uri(this.Authentication.Domain, relativeUri);
            }
        }

        public MyDomainViewModel MyDomainViewModel
        {
            get { return this.myDomainViewModel; }
            private set
            {
                this.myDomainViewModel = value;
                this.CalculateIsValid();
                this.CalculateHasErrors();
                this.OnNotifyPropertyChanged();
            }
        }

        public DesignTimeAuthentication Authentication
        {
            get { return this.authentication; }
            set
            {
                if (this.authentication != value)
                {
                    if (this.authentication != null)
                    {
                        this.authentication.PropertyChanged -= this.Authentication_PropertyChanged;
                    }

                    this.authentication = value;
                    this.authentication.PropertyChanged += this.Authentication_PropertyChanged;
                    this.OnNotifyPropertyChanged();
                    this.InitializeMyDomainViewModel();
                }
            }
        }

        public IEnumerable<DesignTimeAuthentication> AvailableAuthentications { get; private set; }

        public IEnumerable<Environment> Environments
        {
            get { return this.environments; }
        }

        public event EventHandler<EventArgs> PageLeaving;

        public override Task<NavigationEnabledState> OnPageEnteringAsync(WizardEnteringArgs args)
        {
            this.AvailableAuthentications = this.userSettings.MruDesignTimeAuthentications.Union(
                new DesignTimeAuthentication[] { new DesignTimeAuthentication() });
            this.Authentication = this.AvailableAuthentications.First();
            this.OnNotifyPropertyChanged("AvailableAuthentications");

            // If this is the first use of the page, default the finish button to be enabled.
            bool? isFinishEnabled = this.isFirstUse ? true : (bool?)null;
            this.isFirstUse = false;

            return Task.FromResult(new NavigationEnabledState(null, null, isFinishEnabled));
        }

        public override async Task<WizardNavigationResult> OnPageLeavingAsync(WizardLeavingArgs args)
        {
            WizardNavigationResult result;

            using (this.host.StartBusyIndicator(Resources.DesignTimeAuthenticationViewModel_AuthenticatingProgress))
            {
                string error = null;

                if (this.Authentication.RefreshToken == null)
                {
                    // New identity or a existing identity w/no refresh token
                    error = this.AuthenticateUser();

                    if (error == null && this.Authentication.EnvironmentType == EnvironmentType.Custom)
                    {
                        UserSettings.AddToTopOfMruList(this.userSettings.MruMyDomains, this.Authentication.MyDomain.ToString());
                    }
                }
                else if (this.Authentication.AccessToken == null)
                {
                    // Existing identity w/no access token
                    try
                    {
                        await AuthenticationHelper.RefreshAccessToken(this.Authentication);
                    }
                    catch (ForceException ex)
                    {
                        if (ex.Error == Error.InvalidGrant) // Expired refresh token
                        {
                            this.Authentication.RefreshToken = null;
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
                    UserSettings.AddToTopOfMruList(this.userSettings.MruDesignTimeAuthentications, this.Authentication);
                    result = WizardNavigationResult.Success;

                    if (this.PageLeaving != null)
                    {
                        this.PageLeaving(this, EventArgs.Empty);
                    }
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

            AuthenticateRedirectHost dialog = new AuthenticateRedirectHost(this);
            dialog.Owner = Window.GetWindow(this.View);
            dialog.ShowDialog();

            if (dialog.AuthenticationError != null)
            {
                error = dialog.AuthenticationError.Message;
            }
            else if (this.Authentication.RefreshToken == null)
            {
                // The user either canceled out of the authentication dialog or did not grant the required permissions.
                error = Resources.DesignTimeAuthenticationViewModel_UnableToAuthenticateUser;
            }

            return error;
        }

        private void InitializeMyDomainViewModel()
        {
            if (this.MyDomainViewModel != null)
            {
                this.MyDomainViewModel.PropertyChanged -= this.MyDomainViewModel_PropertyChanged;
                this.MyDomainViewModel = null;
            }

            if (this.Authentication.EnvironmentType == EnvironmentType.Custom)
            {
                this.MyDomainViewModel = new MyDomainViewModel(
                    this.Authentication.MyDomain,
                    myDomainUri => this.Authentication.MyDomain = myDomainUri,
                    this.userSettings);
                this.MyDomainViewModel.PropertyChanged += this.MyDomainViewModel_PropertyChanged;
            }
        }

        public async Task<bool> RetrieveAuthenticationFromRedirect(Uri uri)
        {
            bool authenticationFound = false;

            if (string.Equals(uri.Authority, this.RedirectUrl.Authority, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(uri.AbsolutePath, this.RedirectUrl.AbsolutePath, StringComparison.OrdinalIgnoreCase))
            {
                DesignTimeAuthenticationViewModel.CheckAuthenticationErrors(uri);

                string[] parameters = uri.Fragment.TrimStart('#').Split('&');

                this.Authentication.AccessToken = DesignTimeAuthenticationViewModel.RetrieveParameterValue(parameters, "access_token=");
                this.Authentication.RefreshToken = DesignTimeAuthenticationViewModel.RetrieveParameterValue(parameters, "refresh_token=");
                this.Authentication.InstanceUrl = DesignTimeAuthenticationViewModel.RetrieveParameterValue(parameters, "instance_url=");

                try
                {
                    string id = DesignTimeAuthenticationViewModel.RetrieveParameterValue(parameters, "id=");
                    await this.RetrieveIdInfo(id);
                }
                catch (Exception)
                {
                    this.Authentication.AccessToken = null;
                    this.Authentication.RefreshToken = null;
                    this.Authentication.InstanceUrl = null;

                    throw;
                }

                authenticationFound = true;
            }

            return authenticationFound;
        }

        private static void CheckAuthenticationErrors(Uri uri)
        {
            string[] parameters = uri.Query.TrimStart('?').Split('&');
            string error = DesignTimeAuthenticationViewModel.RetrieveParameterValue(parameters, "error=");
            if (error != null)
            {
                string errorDescription = DesignTimeAuthenticationViewModel.RetrieveParameterValue(parameters, "error_description=");

                // In the unexpected case when there is no error description, use the error code.
                string exceptionMessage = errorDescription == null ? error : errorDescription;

                throw new InvalidOperationException(exceptionMessage);
            }
        }

        private static string RetrieveParameterValue(string[] parameters, string parameterName)
        {
            string paramValue = parameters.FirstOrDefault(p => p.StartsWith(parameterName, StringComparison.OrdinalIgnoreCase));
            return paramValue == null ? null : HttpUtility.UrlDecode(paramValue.Substring(parameterName.Length));
        }

        private async Task RetrieveIdInfo(string id)
        {
            // Note: Expired AccessTokens are not handled here because the Id request is made as part of the authentication response handler.
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, id);
            request.Headers.Authorization = new AuthenticationHeaderValue(Constants.Header_OAuth, this.Authentication.AccessToken);
            HttpResponseMessage response = await httpClient.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string content = await response.Content.ReadAsStringAsync();
                IdInfo idInfo = JsonConvert.DeserializeObject<IdInfo>(content);
                this.Authentication.UserName = idInfo.UserName;
                this.Authentication.MetadataServiceUrl = idInfo.Urls.Metadata;
            }
            else
            {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture, Resources.AuthenticationHelper_UnableToConnectToSalesforce, request, response));
            }
        }

        private void MyDomainViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Constants.IsValidPropertyName)
            {
                this.CalculateIsValid();
            }
            else if (e.PropertyName == Constants.HasErrorsPropertyName)
            {
                this.CalculateHasErrors();
            }
        }

        private void CalculateIsValid()
        {
            this.IsValid = this.MyDomainViewModel == null || this.MyDomainViewModel.IsValid;
        }

        private void CalculateHasErrors()
        {
            this.HasErrors = this.MyDomainViewModel != null && this.MyDomainViewModel.HasErrors;
        }

        private void Authentication_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == DesignTimeAuthentication.EnvironmentTypePropertyName)
            {
                this.InitializeMyDomainViewModel();
            }
        }

        private class IdInfo
        {
            public string UserName { get; set; }
            public ServiceUrls Urls { get; set; }
        }

        private class ServiceUrls
        {
            public string Metadata { get; set; }
        }
    }
}
