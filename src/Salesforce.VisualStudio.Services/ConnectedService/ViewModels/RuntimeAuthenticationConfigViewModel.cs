using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.Views;
using System;
using System.ComponentModel;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    internal class RuntimeAuthenticationConfigViewModel : PageViewModel
    {
        private MyDomainViewModel myDomainViewModel;
        private RuntimeAuthentication runtimeAuthentication;
        private bool isCustomDomain;
        private Func<Uri> getDesignTimeMyDomain;

        public RuntimeAuthenticationConfigViewModel(UserSettings userSettings, Func<Uri> getDesignTimeMyDomain)
        {
            this.UserSettings = userSettings;
            this.getDesignTimeMyDomain = getDesignTimeMyDomain;
            this.View = new RuntimeAuthenticationConfigPage(this);
        }

        public override string Description
        {
            get { return Resources.RuntimeAuthenticationConfigViewModel_Description; }
        }

        public override string Legend
        {
            get { return Resources.RuntimeAuthenticationConfigViewModel_Legend; }
        }

        public override string Title
        {
            get { return Resources.RuntimeAuthenticationConfigViewModel_Title; }
        }

        public MyDomainViewModel MyDomainViewModel
        {
            get { return this.myDomainViewModel; }
            private set
            {
                this.myDomainViewModel = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(CommonViewModel.IsValidPropertyName);
                this.RaisePropertyChanged(CommonViewModel.HasErrorsPropertyName);
            }
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
                    case AuthenticationStrategy.UserNamePassword:
                        this.RuntimeAuthentication = new ServiceAccountWithPassword();
                        break;
                    default:
                        throw new NotImplementedException();
                }

                this.RaisePropertyChanged();
            }
        }

        public RuntimeAuthentication RuntimeAuthentication
        {
            get { return this.runtimeAuthentication; }
            private set
            {
                this.runtimeAuthentication = value;
                this.RaisePropertyChanged();
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
                            myDomainUri => ((WebServerFlowInfo)(this.RuntimeAuthentication)).MyDomain = myDomainUri,
                            this.UserSettings);
                        this.MyDomainViewModel.PropertyChanged += this.MyDomainViewModel_PropertyChanged;
                    }
                    else if (this.MyDomainViewModel != null)
                    {
                        this.MyDomainViewModel.PropertyChanged -= this.MyDomainViewModel_PropertyChanged;
                        this.MyDomainViewModel = null;
                        this.RaisePropertyChanged(CommonViewModel.HasErrorsPropertyName);
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
            if (e.PropertyName == CommonViewModel.IsValidPropertyName)
            {
                this.RaisePropertyChanged(CommonViewModel.IsValidPropertyName);
            }
            else if (e.PropertyName == CommonViewModel.HasErrorsPropertyName)
            {
                this.RaisePropertyChanged(CommonViewModel.HasErrorsPropertyName);
            }
        }
    }
}
