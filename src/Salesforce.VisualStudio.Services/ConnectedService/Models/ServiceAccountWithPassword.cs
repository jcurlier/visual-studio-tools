using System.Collections.Generic;
using System.Linq;

namespace Salesforce.VisualStudio.Services.ConnectedService.Models
{
    internal class ServiceAccountWithPassword : ServiceAccount, IAuthenticationWithConsumerSecret
    {
        public ServiceAccountWithPassword()
            : base()
        {
        }

        public override AuthenticationStrategy AuthStrategy
        {
            get { return AuthenticationStrategy.UserNamePassword; }
        }

        public string ConsumerSecret { get; set; }

        public string Password { get; set; }

        public string UserSecurityToken { get; set; }

        public override IList<ConfigSetting> GetConfigSettings(string connectedAppName)
        {
            IList<ConfigSetting> settings = base.GetConfigSettings(connectedAppName);

            // Insert the ConsumerSecret right after the ConsumerKey so there is a logical ordering to the settings.
            // Using Add would cause the UserName setting to appear between the ConsumerKey and ConsumerSecret settings.
            ConfigSetting consumerKeySetting = settings.Single(s => s.Key == Constants.ConfigKey_ConsumerKey);
            settings.Insert(
                settings.IndexOf(consumerKeySetting) + 1,
                new ConfigSetting(Constants.ConfigKey_ConsumerSecret, this.ConsumerSecret));

            // Note:  Once UI support is added for configuring service accounts, this code will need to be updated
            // to reference the Password and UserSecurityToken property values.
            settings.Add(new ConfigSetting(Constants.ConfigKey_Password, Constants.ConfigDefaultValue));
            settings.Add(new ConfigSetting(Constants.ConfigKey_SecurityToken, Constants.ConfigDefaultValue));

            return settings;
        }
    }
}
