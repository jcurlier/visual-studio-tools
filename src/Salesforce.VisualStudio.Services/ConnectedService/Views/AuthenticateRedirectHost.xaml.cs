using Salesforce.VisualStudio.Services.ConnectedService.ViewModels;
using System;
using System.Windows;
using System.Windows.Navigation;

namespace Salesforce.VisualStudio.Services.ConnectedService.Views
{
    /// <summary>
    /// Interaction logic for AuthenticateRedirectHost.xaml
    /// </summary>
    internal partial class AuthenticateRedirectHost : Window
    {
        public AuthenticateRedirectHost(DesignTimeAuthenticationViewModel authenticationViewModel)
        {
            this.InitializeComponent();

            this.DataContext = authenticationViewModel;

            this.browser.Navigating += browser_Navigating;
            this.browser.Navigate(authenticationViewModel.AuthorizeUrl);
        }

        public Exception AuthenticationError { get; private set; }

        private DesignTimeAuthenticationViewModel AuthenticationViewModel
        {
            get { return (DesignTimeAuthenticationViewModel)this.DataContext; }
        }

        private async void browser_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            try
            {
                if (await this.AuthenticationViewModel.RetrieveAuthenticationFromRedirectAsync(e.Uri))
                {
                    e.Cancel = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                this.AuthenticationError = ex;
                e.Cancel = true;
                this.Close();
            }
        }
    }
}
