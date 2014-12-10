using Salesforce.VisualStudio.Services.ConnectedService.Models;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    internal class MyDomainViewModel : ViewModel, INotifyDataErrorInfo
    {
        private static readonly string[] myDomainError = { Resources.MyDomainViewModel_ErrorMessage };
        private const string MyDomainPropertyName = "MyDomain";

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
                        this.RaisePropertyChanged(Constants.IsValidPropertyName);
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

        public override bool IsValid
        {
            get { return this.isValid; }
        }

        public override bool HasErrors
        {
            get { return this.hasErrors; }
        }

        public UserSettings UserSettings
        {
            get { return this.userSettings; }
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
            if (propertyName == MyDomainViewModel.MyDomainPropertyName)
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
                this.RaisePropertyChanged(Constants.HasErrorsPropertyName);
                this.OnErrorsChanged(MyDomainViewModel.MyDomainPropertyName);
            }
        }
    }
}
