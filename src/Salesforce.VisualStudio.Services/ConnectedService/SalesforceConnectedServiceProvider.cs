using Microsoft.VisualStudio.ConnectedServices;
using Microsoft.VisualStudio.LanguageServices;
using System;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Salesforce.VisualStudio.Services.ConnectedService
{
    /// <summary>
    /// A ConnectedServiceProvider that exposes the ability to add a Salesforce service to a project.
    /// </summary>
    [ConnectedServiceProviderExport(Constants.ProviderId, SupportsUpdate = true)]
    internal class SalesforceConnectedServiceProvider : ConnectedServiceProvider
    {
        [Import]
        internal VisualStudioWorkspace VisualStudioWorkspace { get; private set; }

        public SalesforceConnectedServiceProvider()
        {
            this.Category = Resources.ConnectedServiceProvider_Category;
            this.CreatedBy = Resources.ConnectedServiceProvider_CreatedBy;
            this.Description = Resources.ConnectedServiceProvider_Description;
            this.Icon = new BitmapImage(new Uri("pack://application:,,/" + Assembly.GetAssembly(this.GetType()).ToString() + ";component/ConnectedService/Views/Resources/ProviderIcon.png"));
            this.MoreInfoUri = new Uri(Constants.MoreInfoLink);
            this.Name = Resources.ConnectedServiceProvider_Name;
            this.Version = typeof(SalesforceConnectedServiceProvider).Assembly.GetName().Version;
        }

        public override Task<ConnectedServiceConfigurator> CreateConfiguratorAsync(ConnectedServiceProviderContext context)
        {
            ConnectedServiceConfigurator wizard = new SalesforceConnectedServiceWizard(context, this.VisualStudioWorkspace);

            return Task.FromResult(wizard);
        }
    }
}
