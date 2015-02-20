using Microsoft.VisualStudio.ConnectedServices;
using Salesforce.VisualStudio.Services.ConnectedService.CodeModel;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.Utilities;
using System.Collections.Generic;

namespace Salesforce.VisualStudio.Services.ConnectedService
{
    /// <summary>
    /// A specific instance of a Salesforce service.
    /// </summary>
    internal class SalesforceConnectedServiceInstance : ConnectedServiceInstance
    {
        public SalesforceConnectedServiceInstance()
        {
            this.InstanceId = "Salesforce";
            this.Name = Resources.ConnectedServiceInstance_Name;
        }

        public string ConnectedAppName { get; set; }

        public DesignerData DesignerData { get; set; }

        public DesignTimeAuthentication DesignTimeAuthentication { get; set; }

        public RuntimeAuthentication RuntimeAuthentication { get; set; }

        /// <summary>
        /// Gets the objects that were selected by the end user to be scaffolded.  In the Update flow,
        /// this is only the new objects selected.
        /// </summary>
        public IEnumerable<SObjectDescription> SelectedObjects { get; set; }

        public TelemetryHelper TelemetryHelper { get; set; }
    }
}
