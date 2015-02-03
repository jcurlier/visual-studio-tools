using Salesforce.VisualStudio.Services.ConnectedService.Models;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    /// <summary>
    /// The view model for a particular Salesforce Environment.
    /// </summary>
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
