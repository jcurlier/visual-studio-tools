using Salesforce.VisualStudio.Services.ConnectedService.Models;
using System;
using System.ComponentModel;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    internal class RuntimeAuthenticationViewModel : ViewModel
    {
        private MyDomainViewModel myDomainViewModel;
        private RuntimeAuthentication runtimeAuthentication;
        private bool isCustomDomain;
        private Func<Uri> getDesignTimeMyDomain;

        public RuntimeAuthenticationViewModel(UserSettings userSettings, Func<Uri> getDesignTimeMyDomain)
        {
            this.RuntimeAuthStrategy = AuthenticationStrategy.WebServerFlow;
            this.UserSettings = userSettings;
            this.getDesignTimeMyDomain = getDesignTimeMyDomain;
        }

        public AuthenticationStrategy RuntimeAuthStrategy
        {
            get { return this.RuntimeAuthentication.AuthStrategy; }
            set
            {
                switch (value)
                {
                    case AuthenticationStrategy.WebServerFlow:
                        this.RuntimeAuthentication = new WebServerFlowInfo();
                        break;
                    case AuthenticationStrategy.DigitalCertificate:
                        this.RuntimeAuthentication = new ServiceAccountWithJWT();
                        break;
                    case AuthenticationStrategy.UserNamePassword:
                        this.RuntimeAuthentication = new ServiceAccountWithPassword();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public MyDomainViewModel MyDomainViewModel
        {
            get { return this.myDomainViewModel; }
            private set
            {
                this.myDomainViewModel = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(Constants.IsValidPropertyName);
                this.RaisePropertyChanged(Constants.HasErrorsPropertyName);
            }
        }

        public RuntimeAuthentication RuntimeAuthentication
        {
            get { return this.runtimeAuthentication; }
            private set
            {
                this.runtimeAuthentication = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged("RuntimeAuthStrategy");
            }
        }

        public bool IsCustomDomain
        {
            get { return this.isCustomDomain; }
            set
            {
                if (value != this.isCustomDomain)
                {
                    this.isCustomDomain = value;

                    if (this.isCustomDomain)
                    {
                        this.MyDomainViewModel = new MyDomainViewModel(
                            this.getDesignTimeMyDomain(),
                            myDomainUri => ((WebServerFlowInfo)(this.RuntimeAuthentication)).MyDomain = myDomainUri);
                        this.MyDomainViewModel.PropertyChanged += this.MyDomainViewModel_PropertyChanged;
                    }
                    else if (this.MyDomainViewModel != null)
                    {
                        this.MyDomainViewModel.PropertyChanged -= this.MyDomainViewModel_PropertyChanged;
                        this.MyDomainViewModel = null;
                        this.RaisePropertyChanged(Constants.HasErrorsPropertyName);
                    }

                    this.RaisePropertyChanged();
                }
            }
        }

        public override bool IsValid
        {
            get { return this.MyDomainViewModel == null || this.MyDomainViewModel.IsValid; }
        }

        public override bool HasErrors
        {
            get { return this.MyDomainViewModel != null && this.MyDomainViewModel.HasErrors; }
        }

        public UserSettings UserSettings { get; private set; }

        private void MyDomainViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
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
    }
}
