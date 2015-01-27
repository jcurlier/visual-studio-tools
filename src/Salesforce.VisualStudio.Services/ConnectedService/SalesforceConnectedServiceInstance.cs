using Microsoft.VisualStudio.ConnectedServices;
using Salesforce.VisualStudio.Services.ConnectedService.CodeModel;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.Utilities;
using System.Collections.Generic;

namespace Salesforce.VisualStudio.Services.ConnectedService
{
    internal class SalesforceConnectedServiceInstance : ConnectedServiceInstance
    {
        public SalesforceConnectedServiceInstance(
            DesignTimeAuthentication designTimeAuthentication,
            RuntimeAuthentication runtimeAuthentication,
            IEnumerable<SObjectDescription> selectedObjects)
        {
            this.InstanceId = "Salesforce";
            this.Name = Resources.ConnectedServiceInstance_Name;
            this.RuntimeAuthentication = runtimeAuthentication;
            this.DesignTimeAuthentication = designTimeAuthentication;
            this.SelectedObjects = selectedObjects;
            this.TelemetryHelper = new TelemetryHelper();
        }

        public DesignTimeAuthentication DesignTimeAuthentication { get; private set; }

        public RuntimeAuthentication RuntimeAuthentication { get; private set; }

        public string ConnectedAppName { get; set; }

        public string GeneratedArtifactSuffix { get; set; }

        public IEnumerable<SObjectDescription> SelectedObjects { get; private set; }

        public TelemetryHelper TelemetryHelper { get; private set; }
    }
}
