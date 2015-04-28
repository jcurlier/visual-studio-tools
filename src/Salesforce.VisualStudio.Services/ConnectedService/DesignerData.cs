using Microsoft.VisualStudio.ConnectedServices;
using System.Runtime.Serialization;

namespace Salesforce.VisualStudio.Services.ConnectedService
{
    /// <summary>
    /// The provider's custom designer data that is written to the ConnectedService.json file that is
    /// added to the project.
    /// </summary>
    [DataContract]
    internal class DesignerData
    {
        public DesignerData()
        {
        }

        [DataMember(EmitDefaultValue = false)]
        public string ModelsHintPath { get; set; }

        // This isn't a DataMember because it is persisted as part of the core designer data as the ServiceFolder.
        // This member is here as a convienent way to track/flow the state.
        public string ServiceName { get; set; }

        public string GetDefaultedModelsHintPath()
        {
            string modelsHintPath = this.ModelsHintPath;
            if (string.IsNullOrWhiteSpace(modelsHintPath))
            {
                modelsHintPath = SalesforceConnectedServiceHandler.GetModelsDirectoryName(this.ServiceName);
            }

            return modelsHintPath;
        }

        public void StoreExtendedDesignerData(ConnectedServiceContext context)
        {
            // Only set the extended data when settings exists.
            if (this.ModelsHintPath == null)
            {
                context.SetExtendedDesignerData<DesignerData>(null);
            }
            else
            {
                context.SetExtendedDesignerData(this);
            }
        }
    }
}
