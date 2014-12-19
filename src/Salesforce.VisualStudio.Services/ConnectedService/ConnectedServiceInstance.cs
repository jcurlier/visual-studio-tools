using Microsoft.VisualStudio.ConnectedServices;
using Salesforce.VisualStudio.Services.ConnectedService.CodeModel;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.Utilities;
using System.Collections.Generic;

namespace Salesforce.VisualStudio.Services.ConnectedService
{
    internal class ConnectedServiceInstance : IConnectedServiceInstance
    {
        public ConnectedServiceInstance(
            DesignTimeAuthentication designTimeAuthentication,
            RuntimeAuthentication runtimeAuthentication,
            IEnumerable<SObjectDescription> selectedObjects)
        {
            this.RuntimeAuthentication = runtimeAuthentication;
            this.DesignTimeAuthentication = designTimeAuthentication;
            this.SelectedObjects = selectedObjects;
            this.TelemetryHelper = new TelemetryHelper();
        }

        public string InstanceId
        {
            get { return "Salesforce"; }
        }

        public IReadOnlyDictionary<string, object> Metadata
        {
            get { return null; }
        }

        public string Name
        {
            get { return Resources.ConnectedServiceInstance_Name; }
        }

        public string ProviderId
        {
            get { return Constants.ProviderIdValue; }
        }

        public DesignTimeAuthentication DesignTimeAuthentication { get; private set; }

        public RuntimeAuthentication RuntimeAuthentication { get; private set; }

        public string ConnectedAppName { get; set; }

        public string GeneratedArtifactSuffix { get; set; }

        public IEnumerable<SObjectDescription> SelectedObjects { get; private set; }

        public TelemetryHelper TelemetryHelper { get; private set; }
    }
}
