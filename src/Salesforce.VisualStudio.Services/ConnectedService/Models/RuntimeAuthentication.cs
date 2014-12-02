using System.Collections.Generic;
using System.Globalization;

namespace Salesforce.VisualStudio.Services.ConnectedService.Models
{
    internal abstract class RuntimeAuthentication
    {
        protected RuntimeAuthentication()
        {
        }

        public abstract AuthenticationStrategy AuthStrategy { get; }

        public string ConsumerKey { get; set; }

        public virtual IList<ConfigSetting> GetConfigSettings(string connectedAppName)
        {
            IList<ConfigSetting> settings = new List<ConfigSetting>();

            settings.Add(new ConfigSetting(
                Constants.ConfigKey_ConsumerKey,
                this.ConsumerKey,
                string.Format(CultureInfo.CurrentCulture, Resources.RuntimeAuthentication_ConsumerKeyComment, connectedAppName)));

            return settings;
        }
    }
}
