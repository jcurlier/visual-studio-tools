using Microsoft.VisualStudio.ConnectedServices;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.Utilities;
using Salesforce.VisualStudio.Services.ConnectedService.Views;
using System;
using System.ComponentModel;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    /// <summary>
    /// The view model for the Runtime Authentication Config wizard page.
    /// </summary>
    internal class RuntimeAuthenticationConfigViewModel : SalesforceConnectedServiceWizardPage
    {
        private MyDomainViewModel myDomainViewModel;
        private RuntimeAuthentication runtimeAuthentication;
        private bool isCustomDomain;
        private Func<Uri> getDesignTimeMyDomain;

        public RuntimeAuthenticationConfigViewModel(
            ConnectedServiceWizard wizard,
            Func<Uri> getDesignTimeMyDomain)
            : base(wizard)
        {
            this.getDesignTimeMyDomain = getDesignTimeMyDomain;
            this.Title = Resources.RuntimeAuthenticationConfigViewModel_Title;
            this.Description = Resources.RuntimeAuthenticationConfigViewModel_Description;
            this.Legend = Resources.RuntimeAuthenticationConfigViewModel_Legend;
            this.View = new RuntimeAuthenticationConfigPage(this);
        }

        public MyDomainViewModel MyDomainViewModel
        {
            get { return this.myDomainViewModel; }
            private set
            {
                this.myDomainViewModel = value;
                this.CalculateIsValid();
                this.CalculateHasErrors();
                this.OnPropertyChanged();
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

                        // Clear IsCustomDomain to trigger validation to rerun in the case where there are
                        // errors related to a custom MyDomain.
                        this.IsCustomDomain = false;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                this.OnPropertyChanged();
            }
        }

        public RuntimeAuthentication RuntimeAuthentication
        {
            get { return this.runtimeAuthentication; }
            private set
            {
                this.runtimeAuthentication = value;
                this.OnPropertyChanged();
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
                            this.Wizard.UserSettings);
                        this.MyDomainViewModel.PropertyChanged += this.MyDomainViewModel_PropertyChanged;
                    }
                    else if (this.MyDomainViewModel != null)
                    {
                        this.MyDomainViewModel.PropertyChanged -= this.MyDomainViewModel_PropertyChanged;
                        this.MyDomainViewModel = null;
                    }

                    this.OnPropertyChanged();
                }
            }
        }

        private void MyDomainViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MyDomainViewModel.IsValid))
            {
                this.CalculateIsValid();
            }
            else if (e.PropertyName == nameof(MyDomainViewModel.HasErrors))
            {
                this.CalculateHasErrors();
            }
        }
        private void CalculateIsValid()
        {
            this.IsValid = this.MyDomainViewModel == null || this.MyDomainViewModel.IsValid;
        }

        private void CalculateHasErrors()
        {
            this.HasErrors = this.MyDomainViewModel != null && this.MyDomainViewModel.HasErrors;
        }
    }
}