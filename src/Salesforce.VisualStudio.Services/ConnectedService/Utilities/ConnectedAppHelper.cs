using EnvDTE;
using Microsoft.VisualStudio.ConnectedServices;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.SalesforceMetadata;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Services.Protocols;

namespace Salesforce.VisualStudio.Services.ConnectedService.Utilities
{
    /// <summary>
    /// A utility class that creates and configures Salesforce Connected Apps.
    /// </summary>
    internal static class ConnectedAppHelper
    {
        public static async Task CreateConnectedAppAsync(
            SalesforceConnectedServiceInstance salesforceInstance,
            ConnectedServiceLogger logger,
            Project project)
        {
            using (MetadataService metadataService = new MetadataService())
            {
                metadataService.Url = salesforceInstance.DesignTimeAuthentication.MetadataServiceUrl;
                metadataService.SessionHeaderValue = new SessionHeader();
                metadataService.SessionHeaderValue.sessionId = salesforceInstance.DesignTimeAuthentication.AccessToken;

                await AuthenticationHelper.ExecuteSalesforceRequestAsync<SoapHeaderException>(
                    salesforceInstance.DesignTimeAuthentication,
                    async () => await ConnectedAppHelper.CreateConnectedAppAsync(salesforceInstance, metadataService, logger, project),
                    (e) => e.Message.StartsWith("INVALID_SESSION_ID", StringComparison.OrdinalIgnoreCase),
                    () => metadataService.SessionHeaderValue.sessionId = salesforceInstance.DesignTimeAuthentication.AccessToken);
            }
        }

        private static async Task CreateConnectedAppAsync(
            SalesforceConnectedServiceInstance salesforceInstance,
            MetadataService metadataService,
            ConnectedServiceLogger logger,
            Project project)
        {
            salesforceInstance.ConnectedAppName = ConnectedAppHelper.GetUniqueConnectedAppName(metadataService, project);
            ConnectedApp connectedApp = ConnectedAppHelper.ConstructConnectedApp(salesforceInstance, project);
            SaveResult[] saveResults = metadataService.createMetadata(new Metadata[] { connectedApp });

            if (ConnectedAppHelper.DoSaveResultsIndicateDuplicateValue(saveResults))
            {
                // The connected app failed to be created because one already exists with the specified name.  This implies that the 
                // attempt to generate a unique name by reading the existing connected apps failed.  It is unknown at this point what 
                // causes the Salesforce server to sometimes respond to a SOAP ReadMetadata request by returning nil for a Connected App
                // name that actually exists.  In this case, retry using a random number as the app name's suffix.

                Debug.Fail(Resources.DebugFailMessage_DuplicateConnectedAppName.FormatCurrentCulture(salesforceInstance.ConnectedAppName));

                string secondAttemptConnectedAppName = ConnectedAppHelper.GetUniqueConnectedAppName(metadataService, project, true);
                await logger.WriteMessageAsync(LoggerMessageCategory.Information, Resources.LogMessage_DuplicateConnectedAppName, salesforceInstance.ConnectedAppName, secondAttemptConnectedAppName);
                
                salesforceInstance.ConnectedAppName = secondAttemptConnectedAppName;
                connectedApp = ConnectedAppHelper.ConstructConnectedApp(salesforceInstance, project);
                saveResults = metadataService.createMetadata(new Metadata[] { connectedApp });
            }

            if (saveResults.Length != 1 || !saveResults[0].success)
            {
                string errorMessages = saveResults.SelectMany(r => r.errors)
                   .Select(e => e.message)
                   .Aggregate((w, n) => w + "\r\n" + n);

                throw new InvalidOperationException(Resources.ConnectedAppHelper_FailedToCreateConnectedApp.FormatCurrentCulture(errorMessages));
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
                    await logger.WriteMessageAsync(LoggerMessageCategory.Warning, Resources.LogMessage_FailedReadingConnectedApp);

                    salesforceInstance.RuntimeAuthentication.ConsumerKey = Constants.ConfigValue_RequiredDefault;
                }
            }
        }

        private static bool DoSaveResultsIndicateDuplicateValue(SaveResult[] saveResults)
        {
            return saveResults.Length == 1 && !saveResults[0].success && saveResults[0].errors.Any(e => e.statusCode == StatusCode.DUPLICATE_VALUE);
        }

        private static string GetUniqueConnectedAppName(MetadataService metadataService, Project project, bool useRandomSuffix = false)
        {
            string validAppName = ConnectedAppHelper.MakeValidConnectedAppName(project.Name);

            string appNameSuffix;
            if (useRandomSuffix)
            {
                // The default Random constructor uses a seed value derived from the system clock and has finite resolution.  However, 
                // because this code path is only hit once per running of the handler, this Random object having the same seed in subsequent 
                // calls (and thus undesirably generating the same number) is not a concern.
                appNameSuffix = new Random().Next(100, 1000).ToString();
            }
            else
            {
                appNameSuffix = GeneralUtilities.GetUniqueSuffix(suffix =>
                    ConnectedAppHelper.GetConnectedAppByName(validAppName + suffix, metadataService) != null);
            }

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
            SalesforceConnectedServiceInstance salesforceInstance,
            Project project)
        {
            ConnectedApp connectedApp = new ConnectedApp();
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

        private static string GenerateAppCallbackUrl(SalesforceConnectedServiceInstance salesforceInstance, Project project)
        {
            string callbackUrl;
            WebServerFlowInfo webServerFlowInfo = salesforceInstance.RuntimeAuthentication as WebServerFlowInfo;
            if (webServerFlowInfo != null)
            {
                webServerFlowInfo.RedirectUri = new Uri(
                    Constants.OAuthRedirectHandlerPathFormat.FormatInvariantCulture(salesforceInstance.DesignerData.ServiceName),
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
