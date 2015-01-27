using Salesforce.Common;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using System;
using System.Threading.Tasks;

namespace Salesforce.VisualStudio.Services.ConnectedService.Utilities
{
    internal static class AuthenticationHelper
    {
        public static async Task ExecuteSalesforceRequestAsync<TException>(
            DesignTimeAuthentication designTimeAuthentication,
            Func<Task> executeRequest,
            Func<TException, bool> getIsAccessTokenExpired,
            Action onRefreshToken = null)
            where TException : Exception
        {
            bool isAccessTokenExpired = false;
            try
            {
                await executeRequest();
            }
            catch (TException e)
            {
                if (getIsAccessTokenExpired(e))
                {
                    isAccessTokenExpired = true;
                }
                else
                {
                    throw;
                }
            }

            if (isAccessTokenExpired)
            {
                await AuthenticationHelper.RefreshAccessTokenAsync(designTimeAuthentication);

                if (onRefreshToken != null)
                {
                    onRefreshToken();
                }

                await executeRequest();
            }
        }

        public static async Task RefreshAccessTokenAsync(DesignTimeAuthentication designTimeAuthentication)
        {
            AuthenticationClient client = new AuthenticationClient();
            client.AccessToken = designTimeAuthentication.AccessToken;
            client.RefreshToken = designTimeAuthentication.RefreshToken;
            client.InstanceUrl = designTimeAuthentication.InstanceUrl;

            await client.TokenRefreshAsync(
                Constants.VisualStudioConnectedAppClientId,
                client.RefreshToken,
                string.Empty,
                new Uri(designTimeAuthentication.Domain, "/services/oauth2/token").ToString());

            designTimeAuthentication.AccessToken = client.AccessToken;
            designTimeAuthentication.InstanceUrl = client.InstanceUrl;
        }
    }
}
