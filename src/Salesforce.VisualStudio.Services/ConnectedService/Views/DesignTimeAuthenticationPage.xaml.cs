using Salesforce.VisualStudio.Services.ConnectedService.ViewModels;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Salesforce.VisualStudio.Services.ConnectedService.Views
{
    /// <summary>
    /// Interaction logic for DesignTimeAuthenticationPage.xaml
    /// </summary>
    internal partial class DesignTimeAuthenticationPage : UserControl
    {
        public DesignTimeAuthenticationPage(DesignTimeAuthenticationViewModel designTimeAuthenticationViewModel)
        {
            this.InitializeComponent();

            this.DataContext = designTimeAuthenticationViewModel;
        }

        private DesignTimeAuthenticationViewModel DesignTimeAuthenticationViewModel
        {
            get { return (DesignTimeAuthenticationViewModel)this.DataContext; }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            this.DesignTimeAuthenticationViewModel.NavigateHyperlink(e.Uri);

            e.Handled = true;
        }
    }
}
