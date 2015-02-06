using Salesforce.VisualStudio.Services.ConnectedService.Models;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    /// <summary>
    /// The view model for the My Domain configuration that is used by the design time authentication
    /// as well as web server runtime authentication flow.
    /// </summary>
    internal class MyDomainViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private static readonly string[] myDomainError = { Resources.MyDomainViewModel_ErrorMessage };

        private string myDomain;
        private bool isValid;
        private bool isMyDomainFocused;
        private bool hasMyDomainLostFocus;
        private bool hasErrors;
        private Action<Uri> onValidMyDomain;
        private UserSettings userSettings;

        public MyDomainViewModel(Uri myDomain, Action<Uri> onValidMyDomain, UserSettings userSettings)
        {
            this.onValidMyDomain = onValidMyDomain;
            this.MyDomain = myDomain == null ? null : myDomain.ToString();
            this.userSettings = userSettings;

            this.RefreshErrorState();
        }

        public string MyDomain
        {
            get { return this.myDomain; }
            set
            {
                if (value != this.myDomain)
                {
                    this.myDomain = value;

                    bool isMyDomainValid;
                    Uri myDomainUri;
                    if (Uri.TryCreate(this.MyDomain, UriKind.Absolute, out myDomainUri))
                    {
                        isMyDomainValid = true;
                        this.onValidMyDomain(myDomainUri);
                    }
                    else
                    {
                        isMyDomainValid = false;
                    }

                    if (isMyDomainValid != this.isValid)
                    {
                        this.isValid = isMyDomainValid;
                        this.OnPropertyChanged(nameof(MyDomainViewModel.IsValid));
                    }

                    this.RefreshErrorState();
                }
            }
        }

        public bool IsMyDomainFocused
        {
            get { return this.isMyDomainFocused; }
            set
            {
                if (value != this.isMyDomainFocused)
                {
                    this.isMyDomainFocused = value;

                    if (value == false)
                    {
                        this.hasMyDomainLostFocus = true;
                        this.RefreshErrorState();
                    }
                }
            }
        }

        public bool IsValid
        {
            get { return this.isValid; }
        }

        public bool HasErrors
        {
            get { return this.hasErrors; }
        }

        public UserSettings UserSettings
        {
            get { return this.userSettings; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private void OnErrorsChanged(string propertyName)
        {
            if (this.ErrorsChanged != null)
            {
                this.ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (propertyName == nameof(MyDomainViewModel.MyDomain))
            {
                return this.HasErrors ? MyDomainViewModel.myDomainError : Enumerable.Empty<string>();
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }

        private void RefreshErrorState()
        {
            // The MyDomain control is considered invalid when it doesn't represent a valid URL.  A validation error 
            // is only displayed for it when the control loses focus, but a currently displayed error is removed 
            // as soon as its value becomes valid.  An error is not displayed for a null or empty value because all of
            // the fields are considered required.

            bool hasErrorsNewValue = this.IsMyDomainFocused
                ? this.hasErrors && !this.isValid && !string.IsNullOrEmpty(this.MyDomain)
                : this.hasMyDomainLostFocus && !this.isValid && !string.IsNullOrEmpty(this.MyDomain);

            if (hasErrorsNewValue != this.hasErrors)
            {
                this.hasErrors = hasErrorsNewValue;
                this.OnPropertyChanged(nameof(MyDomainViewModel.HasErrors));
                this.OnErrorsChanged(nameof(MyDomainViewModel.MyDomain));
            }
        }
    }
}