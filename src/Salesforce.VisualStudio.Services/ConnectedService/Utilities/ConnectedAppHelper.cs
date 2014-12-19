using EnvDTE;
using Microsoft.VisualStudio.ConnectedServices;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.SalesforceMetadata;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Services.Protocols;

namespace Salesforce.VisualStudio.Services.ConnectedService.Utilities
{
    internal static class ConnectedAppHelper
    {
        public static async Task CreateConnectedApp(
            ConnectedServiceInstance salesforceInstance,
            ILogger logger,
            Project project)
        {
            using (MetadataService metadataService = new MetadataService())
            {
                metadataService.Url = salesforceInstance.DesignTimeAuthentication.MetadataServiceUrl;
                metadataService.SessionHeaderValue = new SessionHeader();
                metadataService.SessionHeaderValue.sessionId = salesforceInstance.DesignTimeAuthentication.AccessToken;

                await AuthenticationHelper.ExecuteSalesforceRequest<SoapHeaderException>(
                    salesforceInstance.DesignTimeAuthentication,
                    () =>
                    {
                        ConnectedAppHelper.CreateConnectedApp(salesforceInstance, metadataService, logger, project);
                        return Task.FromResult<object>(null);
                    },
                    (e) => e.Message.StartsWith("INVALID_SESSION_ID", StringComparison.OrdinalIgnoreCase),
                    () => metadataService.SessionHeaderValue.sessionId = salesforceInstance.DesignTimeAuthentication.AccessToken);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static void CreateConnectedApp(
            ConnectedServiceInstance salesforceInstance,
            MetadataService metadataService,
            ILogger logger,
            Project project)
        {
            ConnectedApp connectedApp = ConnectedAppHelper.ConstructConnectedApp(salesforceInstance, metadataService, project);
            SaveResult[] saveResults = metadataService.createMetadata(new Metadata[] { connectedApp });
            if (saveResults.Length != 1 || !saveResults[0].success)
            {
                string errorMessages = saveResults.SelectMany(r => r.errors)
                   .Select(e => e.message)
                   .Aggregate((w, n) => w + "\r\n" + n);

                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.ConnectedAppHelper_FailedToCreateConnectedApp, errorMessages));
            }
            else
            {
                ConnectedApp readConnectedApp = ConnectedAppHelper.GetConnectedAppByName(connectedApp.fullName, metadataService);
                if (readConnectedApp != null)
                {
                    salesforceInstance.RuntimeAuthentication.ConsumerKey = readConnectedApp.oauthConfig.consumerKey;
                }
                else
                {
                    logger.WriteMessage(LoggerMessageCategory.Error, Resources.LogMessage_FailedReadingConnectedApp);

                    salesforceInstance.RuntimeAuthentication.ConsumerKey = Constants.ConfigValue_RequiredDefault;
                }
            }
        }

        private static string GetUniqueConnectedAppName(MetadataService metadataService, Project project)
        {
            string validAppName = ConnectedAppHelper.MakeValidConnectedAppName(project.Name);

            string appNameSuffix = NamingUtilities.GetUniqueSuffix(suffix =>
                ConnectedAppHelper.GetConnectedAppByName(validAppName + suffix, metadataService) != null);

            return validAppName + appNameSuffix;
        }

        private static string MakeValidConnectedAppName(string name)
        {
            // The connected app name "can only contain underscores and alphanumeric characters. It must 
            // be unique, begin with a letter, not include spaces, not end with an underscore, and not 
            // contain two consecutive underscores."

            // Replace any invalid character with an underscore.
            string connectedAppName = Regex.Replace(name, @"\W", "_");

            // Remove leading digits and underscores.
            connectedAppName = Regex.Replace(connectedAppName, @"^[\d_]*", string.Empty);

            // Remove trailing underscores.
            connectedAppName = Regex.Replace(connectedAppName, @"_*$", string.Empty);

            // Remove consecutive underscores.
            connectedAppName = Regex.Replace(connectedAppName, @"_{2,}", "_");

            return connectedAppName;
        }

        private static ConnectedApp GetConnectedAppByName(string connectedAppName, MetadataService metadataService)
        {
            Metadata[] metadata = metadataService.readMetadata(Constants.Metadata_ConnectedAppType, new string[] { connectedAppName });

            return metadata.Length == 1 ? metadata[0] as ConnectedApp : null;
        }

        private static ConnectedApp ConstructConnectedApp(
            ConnectedServiceInstance salesforceInstance,
            MetadataService metadataService,
            Project project)
        {
            ConnectedApp connectedApp = new ConnectedApp();
            salesforceInstance.ConnectedAppName = ConnectedAppHelper.GetUniqueConnectedAppName(metadataService, project);
            connectedApp.contactEmail = salesforceInstance.DesignTimeAuthentication.UserName;
            connectedApp.fullName = salesforceInstance.ConnectedAppName;
            connectedApp.label = salesforceInstance.ConnectedAppName;

            ConnectedAppOauthConfig oauthConfig = new ConnectedAppOauthConfig();
            connectedApp.oauthConfig = oauthConfig;
            oauthConfig.callbackUrl = ConnectedAppHelper.GenerateAppCallbackUrl(salesforceInstance, project);
            oauthConfig.scopes = new ConnectedAppOauthAccessScope[]
            {
                ConnectedAppOauthAccessScope.Api,
                ConnectedAppOauthAccessScope.Basic,
                ConnectedAppOauthAccessScope.RefreshToken,
            };

            string secret = Guid.NewGuid().ToString("N");
            salesforceInstance.RuntimeAuthentication.ConsumerSecret = secret;
            oauthConfig.consumerSecret = secret;

            return connectedApp;
        }

        private static string GenerateAppCallbackUrl(ConnectedServiceInstance salesforceInstance, Project project)
        {
            string callbackUrl;
            WebServerFlowInfo webServerFlowInfo = salesforceInstance.RuntimeAuthentication as WebServerFlowInfo;
            if (webServerFlowInfo != null)
            {
                webServerFlowInfo.RedirectUri = new Uri(
                    String.Format(CultureInfo.InvariantCulture, Constants.OAuthRedirectHandlerPathFormat, salesforceInstance.GeneratedArtifactSuffix),
                    UriKind.Relative);

                string appUriAuthority;
                try
                {
                    Property iisURLProp = project.Properties.Item("WebApplication.IISUrl");
                    appUriAuthority = (string)iisURLProp.Value;
                }
                catch (ArgumentException)
                {
                    // The user's project doesn't contain a "WebApplication.IISUrl" property, default it to localhost.
                    appUriAuthority = "http://localhost/";
                }

                callbackUrl = new Uri(new Uri(appUriAuthority), webServerFlowInfo.RedirectUri).ToString();
            }
            else
            {
                callbackUrl = "vs://ConnectedService";
            }

            return callbackUrl;
        }
    }
}
