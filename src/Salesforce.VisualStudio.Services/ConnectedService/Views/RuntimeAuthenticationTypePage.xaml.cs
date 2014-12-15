using Salesforce.VisualStudio.Services.ConnectedService.Utilities;
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

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            HyperlinkHelper.OpenSystemBrowser(e.Uri.AbsoluteUri);

            e.Handled = true;
        }
    }
}
