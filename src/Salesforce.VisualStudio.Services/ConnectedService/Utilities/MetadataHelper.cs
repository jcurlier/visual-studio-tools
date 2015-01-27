using Salesforce.Common;
using Salesforce.Common.Models;
using Salesforce.VisualStudio.Services.ConnectedService.CodeModel;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Salesforce.VisualStudio.Services.ConnectedService.Utilities
{
    /// <summary>
    /// A utility class that loads the object metadata from Salesforce.
    /// </summary>
    internal class MetadataLoader
    {
        public static async Task<IEnumerable<SObjectDescription>> LoadObjectsAsync(DesignTimeAuthentication authentication)
        {
            GlobalDescribeResponse describeResponse = await MetadataLoader.GetSalesforceAsync<GlobalDescribeResponse>(
                "sobjects", authentication);

            return describeResponse.SObjects;
        }

        public static async Task<IEnumerable<SObjectDescription>> LoadObjectDetailsAsync(
            IEnumerable<SObjectDescription> sObjects, DesignTimeAuthentication authentication)
        {
            // Note:  There is no batch support with the REST endpoint therefore individual requests must be made.
            IEnumerable<Task<SObjectDescription>> detailTasks = sObjects
                .Select(o => MetadataLoader.GetSalesforceAsync<SObjectDescription>(
                    "sobjects/" + o.Name + "/describe/", authentication));

            return await Task.WhenAll<SObjectDescription>(detailTasks);
        }

        private static async Task<T> GetSalesforceAsync<T>(string urlSuffix, DesignTimeAuthentication authentication)
        {
            T result = default(T);
            await AuthenticationHelper.ExecuteSalesforceRequestAsync<ForceException>(
                authentication,
                async () =>
                {
                    ServiceHttpClient client = new ServiceHttpClient(
                        authentication.InstanceUrl,
                        Constants.SalesforceApiVersionWithPrefix,
                        authentication.AccessToken,
                        new HttpClient());
                    result = await client.HttpGetAsync<T>(urlSuffix);
                },
                (e) => e.Error == Error.InvalidGrant);

            return result;
        }

        private class GlobalDescribeResponse
        {
            public int MaxBatchSize { get; set; }
            public IEnumerable<SObjectDescription> SObjects { get; set; }
        }
    }
}
