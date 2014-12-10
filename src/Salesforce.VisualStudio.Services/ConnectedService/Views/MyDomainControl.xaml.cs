using Salesforce.VisualStudio.Services.ConnectedService.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Salesforce.VisualStudio.Services.ConnectedService.Views
{
    /// <summary>
    /// Interaction logic for MyDomainControl.xaml
    /// </summary>
    internal partial class MyDomainControl : UserControl
    {
        public MyDomainControl()
        {
            this.InitializeComponent();
        }

        private MyDomainViewModel MyDomainViewModel
        {
            get { return (MyDomainViewModel)this.DataContext; }
        }

        private void MyDomain_LostFocus(object sender, RoutedEventArgs e)
        {
            this.MyDomainViewModel.IsMyDomainFocused = false;
        }

        private void MyDomain_GotFocus(object sender, RoutedEventArgs e)
        {
            this.MyDomainViewModel.IsMyDomainFocused = true;
        }
    }
}
