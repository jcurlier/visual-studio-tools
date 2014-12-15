using Microsoft.VisualStudio.ConnectedServices;
using System.Threading.Tasks;
using System.Windows;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    internal abstract class PageViewModel : CommonViewModel, IConnectedServiceWizardPage
    {
        private bool isEnabled = true;

        protected PageViewModel()
        {
        }

        public abstract string Title { get; }

        public abstract string Description { get; }

        public abstract string Legend { get; }

        public bool IsEnabled
        {
            get { return this.isEnabled; }
            set
            {
                if (value != this.isEnabled)
                {
                    this.isEnabled = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public bool IsSelected { get; set; }

        public FrameworkElement View { get; protected set; }

        public virtual Task<NavigationEnabledState> OnPageEntering()
        {
            return Task.FromResult(new NavigationEnabledState(null, null, null));
        }

        public virtual Task<WizardNavigationResult> OnPageLeaving()
        {
            return Task.FromResult(WizardNavigationResult.Success);
        }
    }
}
