using Salesforce.VisualStudio.Services.ConnectedService.Utilities;
using System.Collections.Generic;

namespace Salesforce.VisualStudio.Services.ConnectedService.Models
{
    /// <summary>
    /// A base class used to store the common information for configuring the user's project to use
    /// a particular authentication flow.
    /// </summary>
    internal abstract class RuntimeAuthentication
    {
        protected RuntimeAuthentication()
        {
        }

        public abstract AuthenticationStrategy AuthStrategy { get; }

        public string ConsumerKey { get; set; }

        public string ConsumerSecret { get; set; }

        public virtual IList<ConfigSetting> GetConfigSettings(string connectedAppName)
        {
            IList<ConfigSetting> settings = new List<ConfigSetting>();

            settings.Add(new ConfigSetting(
                Constants.ConfigKey_ConsumerKey,
                this.ConsumerKey,
                Resources.RuntimeAuthentication_ConsumerKeyComment.FormatCurrentCulture(connectedAppName)));
            settings.Add(new ConfigSetting(Constants.ConfigKey_ConsumerSecret, this.ConsumerSecret));

            return settings;
        }
    }
}
