using Salesforce.VisualStudio.Services.ConnectedService.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Salesforce.VisualStudio.Services.ConnectedService.Views
{
    /// <summary>
    /// Interaction logic for DesignTimeAuthenticationPage.xaml
    /// </summary>
    internal partial class DesignTimeAuthenticationPage : UserControl
    {
        public DesignTimeAuthenticationPage(DesignTimeAuthenticationViewModel authenticationViewModel)
        {
            this.InitializeComponent();

            this.DataContext = authenticationViewModel;
        }

        private DesignTimeAuthenticationViewModel DesignTimeAuthenticationViewModel
        {
            get { return (DesignTimeAuthenticationViewModel)this.DataContext; }
        }

        private void MyDomain_LostFocus(object sender, RoutedEventArgs e)
        {
            this.DesignTimeAuthenticationViewModel.MyDomainViewModel.IsMyDomainFocused = false;
        }

        private void MyDomain_GotFocus(object sender, RoutedEventArgs e)
        {
            this.DesignTimeAuthenticationViewModel.MyDomainViewModel.IsMyDomainFocused = true;
        }
    }
}
