using System.Diagnostics.CodeAnalysis;

namespace Salesforce.VisualStudio.Services.ConnectedService
{
    /// <summary>
    /// The types of authentication strategies which are currently supported by the Salesforce connected service provider
    /// that can be used to authenticate to a Salesforce service.
    /// </summary>
    public enum AuthenticationStrategy
    {
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WebServer")]
        WebServerFlow,
        UserNamePassword,
    }
}
