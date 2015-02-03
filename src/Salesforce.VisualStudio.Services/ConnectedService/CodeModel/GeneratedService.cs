using System;

namespace Salesforce.VisualStudio.Services.ConnectedService.CodeModel
{
    /// <summary>
    /// Represents a Salesforce service to generate code for.
    /// </summary>
    [Serializable]
    public sealed class GeneratedService
    {
        internal GeneratedService()
        {
        }

        public string ServiceNamespace { get; internal set; }

        public string ModelsNamespace { get; internal set; }

        public string DefaultNamespace { get; internal set; }

        public AuthenticationStrategy AuthenticationStrategy { get; internal set; }

        public ConfigurationKeyNames ConfigurationKeyNames { get; internal set; }
    }
}
