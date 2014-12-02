using Salesforce.VisualStudio.Services.ConnectedService.Utilities;
using Salesforce.VisualStudio.Services.ConnectedService.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            // Currently the CS core hosts the wizard pages in a ScrollViewer.  This does not provide
            // the desired effect because the page gets a scrollbar instead of the object picker.
            // Bug 1072244 was logged for this issue.  To workaround this for now, walk up the parent
            // hierarchy and disable any ScrollViewers.
            ScrollViewer scrollViewer = ObjectSelectionPage.TryFindParent<ScrollViewer>(this);
            if (scrollViewer != null)
            {
                scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }
        }

        private static T TryFindParent<T>(DependencyObject child)
            where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
            {
                return null;
            }

            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                return ObjectSelectionPage.TryFindParent<T>(parentObject);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            HyperlinkHelper.OpenSystemBrowser(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
