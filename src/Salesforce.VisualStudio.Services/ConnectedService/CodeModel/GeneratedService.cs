using System;

namespace Salesforce.VisualStudio.Services.ConnectedService.CodeModel
{
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
