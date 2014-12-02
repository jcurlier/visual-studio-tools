using Microsoft.VisualStudio.ConnectedServices;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    internal abstract class WizardPage<TViewModel> : IWizardPage, INotifyPropertyChanged
        where TViewModel : ViewModel
    {
        private bool isEnabled = true;

        protected WizardPage(TViewModel viewModel)
        {
            this.ViewModel = viewModel;
            this.ViewModel.PropertyChanged += this.ViewModel_PropertyChanged;
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

        public virtual bool IsValid
        {
            get { return this.ViewModel.IsValid; }
        }

        public virtual bool HasErrors
        {
            get { return this.ViewModel.HasErrors; }
        }

        protected TViewModel ViewModel { get; private set; }

        public FrameworkElement View { get; protected set; }

        public virtual Task<NavigationEnabledState> OnPageEntering()
        {
            return Task.FromResult(new NavigationEnabledState(null, null, null));
        }

        public virtual Task<WizardNavigationResult> OnPageLeaving()
        {
            return Task.FromResult(WizardNavigationResult.Success);
        }

        protected virtual void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Constants.IsValidPropertyName)
            {
                this.RaisePropertyChanged(Constants.IsValidPropertyName);
            }
            else if (e.PropertyName == Constants.HasErrorsPropertyName)
            {
                this.RaisePropertyChanged(Constants.HasErrorsPropertyName);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string name = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
