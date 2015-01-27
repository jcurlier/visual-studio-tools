using Salesforce.VisualStudio.Services.ConnectedService.ViewModels;
using System.Windows.Controls;

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
    }
}
