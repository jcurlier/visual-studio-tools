using System.Diagnostics.CodeAnalysis;

namespace Salesforce.VisualStudio.Services.ConnectedService
{
    // The names used here are the names that appear in the config file.
    public enum AuthenticationStrategy
    {
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WebServer")]
        WebServerFlow,
        UserNamePassword,
    }
}
