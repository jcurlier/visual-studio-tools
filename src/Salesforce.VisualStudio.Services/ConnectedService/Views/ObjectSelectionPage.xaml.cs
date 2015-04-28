using Salesforce.VisualStudio.Services.ConnectedService.ViewModels;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Salesforce.VisualStudio.Services.ConnectedService.Views
{
    /// <summary>
    /// Interaction logic for ObjectSelectionPage.xaml
    /// </summary>
    internal partial class ObjectSelectionPage : UserControl
    {
        public ObjectSelectionPage(ObjectSelectionViewModel objectSelectionViewModel)
        {
            InitializeComponent();

            this.DataContext = objectSelectionViewModel;
        }

        private ObjectSelectionViewModel ObjectSelectionViewModel
        {
            get { return (ObjectSelectionViewModel)this.DataContext; }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            this.ObjectSelectionViewModel.NavigateHyperlink(e.Uri);
            e.Handled = true;
        }
    }
}