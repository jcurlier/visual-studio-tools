using System.Collections.Generic;

namespace Salesforce.VisualStudio.Services.ConnectedService.Models
{
    internal abstract class ServiceAccount : RuntimeAuthentication
    {
        public ServiceAccount()
            : base()
        {
        }

        public string UserName { get; set; }

        public override IList<ConfigSetting> GetConfigSettings(string connectedAppName)
        {
            IList<ConfigSetting> settings = base.GetConfigSettings(connectedAppName);

            // Note:  Once UI support is added for configuring service accounts, this code will need to be updated
            // to reference the UserName property value.
            settings.Add(new ConfigSetting(Constants.ConfigKey_UserName, Constants.ConfigDefaultValue));

            return settings;
        }
    }
}
