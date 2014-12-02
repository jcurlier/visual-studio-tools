using System.Collections.Generic;

namespace Salesforce.VisualStudio.Services.ConnectedService.Models
{
    internal class ServiceAccountWithJWT : ServiceAccount
    {
        public ServiceAccountWithJWT()
            : base()
        {
        }

        public override AuthenticationStrategy AuthStrategy
        {
            get { return AuthenticationStrategy.DigitalCertificate; }
        }

        public string CertificatePath { get; set; }

        public string CertificatePassword { get; set; }

        public override IList<ConfigSetting> GetConfigSettings(string connectedAppName)
        {
            IList<ConfigSetting> settings = base.GetConfigSettings(connectedAppName);

            // Note:  Once UI support is added for configuring service accounts, this code will need to be updated
            // to reference the CertificatePath and CertificatePassword property values.
            settings.Add(new ConfigSetting(Constants.ConfigKey_CertificatePath, Constants.ConfigDefaultValue));
            settings.Add(new ConfigSetting(Constants.ConfigKey_CertificatePassword, Constants.ConfigDefaultValue));

            return settings;
        }
    }
}
