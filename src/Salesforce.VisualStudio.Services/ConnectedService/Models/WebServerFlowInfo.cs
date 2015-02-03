using System;
using System.Collections.Generic;

namespace Salesforce.VisualStudio.Services.ConnectedService.Models
{
    /// <summary>
    /// The information for configuring the user's project to use the web server authentication flow.
    /// </summary>
    internal class WebServerFlowInfo : RuntimeAuthentication
    {
        public WebServerFlowInfo()
            : base()
        {
        }

        public override AuthenticationStrategy AuthStrategy
        {
            get { return AuthenticationStrategy.WebServerFlow; }
        }

        public Uri MyDomain { get; set; }

        public bool HasMyDomain
        {
            get { return this.MyDomain != null; }
        }

        public Uri RedirectUri { get; set; }

        public override IList<ConfigSetting> GetConfigSettings(string connectedAppName)
        {
            IList<ConfigSetting> settings = base.GetConfigSettings(connectedAppName);

            settings.Add(new ConfigSetting(Constants.ConfigKey_RedirectUri, this.RedirectUri));
            settings.Add(new ConfigSetting(
                Constants.ConfigKey_Domain,
                !this.HasMyDomain ? Constants.ProductionDomainUrl : this.MyDomain.ToString()));

            return settings;
        }
    }
}
