using Salesforce.VisualStudio.Services.ConnectedService.ViewModels;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Salesforce.VisualStudio.Services.ConnectedService.Views
{
    /// <summary>
    /// Interaction logic for RuntimeAuthenticationTypePage.xaml
    /// </summary>
    internal partial class RuntimeAuthenticationTypePage : UserControl
    {
        public RuntimeAuthenticationTypePage(RuntimeAuthenticationTypeViewModel runtimeAuthenticationTypeViewModel)
        {
            this.InitializeComponent();

            this.DataContext = runtimeAuthenticationTypeViewModel;
        }

        private RuntimeAuthenticationTypeViewModel RuntimeAuthenticationTypeViewModel
        {
            get { return (RuntimeAuthenticationTypeViewModel)this.DataContext; }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            this.RuntimeAuthenticationTypeViewModel.NavigateHyperlink(e.Uri);

            e.Handled = true;
        }
    }
}
