using Salesforce.VisualStudio.Services.ConnectedService.Models;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    internal class Environment
    {
        public Environment()
        {
        }

        public string DisplayName { get; set; }

        public EnvironmentType Type { get; set; }

        public override string ToString()
        {
            return this.DisplayName;
        }
    }
}
