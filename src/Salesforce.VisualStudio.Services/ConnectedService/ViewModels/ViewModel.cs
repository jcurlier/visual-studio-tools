using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    internal abstract class ViewModel : INotifyPropertyChanged
    {
        public virtual bool IsValid
        {
            get { return true; }
        }

        public virtual bool HasErrors
        {
            get { return false; }
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
