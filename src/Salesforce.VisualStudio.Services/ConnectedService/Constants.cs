namespace Salesforce.VisualStudio.Services.ConnectedService
{
    /// <summary>
    /// Miscellaneous constants that are used across the Salesforce connected service implementation.
    /// </summary>
    internal static class Constants
    {
        public const string ConfigKeyFormat = "Salesforce{0}:{1}";

        public const string ConfigKey_ConsumerKey = "ConsumerKey";
        public const string ConfigKey_ConsumerSecret = "ConsumerSecret";
        public const string ConfigKey_Domain = "Domain";
        public const string ConfigKey_RedirectUri = "RedirectUri";
        public const string ConfigKey_UserName = "UserName";
        public const string ConfigKey_Password = "Password";
        public const string ConfigKey_SecurityToken = "SecurityToken";

        public const string ConfigValue_RequiredDefault = "RequiredValue";
        public const string ConfigValue_OptionalDefault = "OptionalValue";

        public const string ProviderId = "ProviderId";
        public const string ProviderIdValue = "SalesforceConnectedService";

        public const string SalesforceApiVersion = "32.0";
        public const string SalesforceApiVersionWithPrefix = "v32.0";

        public const string ServiceInstanceNameFormat = "Salesforce{0}";
        public const string ModelsName = "Models";

        public const string OAuthRedirectHandlerTypeName = "SalesforceOAuthRedirectHandler";
        public const string OAuthRedirectHandlerNameFormat = "Salesforce{0}OAuthRedirectHandler";
        public const string OAuthRedirectHandlerPathFormat = "/Salesforce{0}OAuthRedirectHandler.axd";

        public const string Metadata_ConnectedAppType = "ConnectedApp";

        public const string Header_OAuth = "OAuth";
        public const string ProductionDomainUrl = "https://login.salesforce.com";
        public const string SandboxDomainUrl = "https://test.salesforce.com";

        public const string NextStepsUrl = "http://developer.salesforce.com/go/VSAddinDoc";
        public const string MoreInfoLink = "http://developer.salesforce.com/go/VSMoreInfo";

        public const string VisualStudioConnectedAppClientId = "3MVG9JZ_r.QzrS7gAjO9uCs2VkFkrvkiZiv9w9fBwzt4ds5YE4fN9VVa.3oTwr7KJKk.BZiPNekIw.d_yEVle";

        public const string HasErrorsPropertyName = "HasErrors";
        public const string IsValidPropertyName = "IsValid";
    }
}
