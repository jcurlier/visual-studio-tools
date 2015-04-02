using Microsoft.VisualStudio.ConnectedServices;
using Microsoft.VisualStudio.LanguageServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
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

            ResourceSet resourceSet = SupportedProjectTypeStrings.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            IEnumerable<string> supportedProjectTypes = resourceSet
                .OfType<DictionaryEntry>()
                .Select(e => e.Value)
                .OfType<string>()
                .OrderBy(v => v);
            foreach (string supportedProjectType in supportedProjectTypes)
            {
                this.SupportedProjectTypes.Add(supportedProjectType);
            }
        }

        public override Task<ConnectedServiceConfigurator> CreateConfiguratorAsync(ConnectedServiceProviderContext context)
        {
            ConnectedServiceConfigurator wizard = new SalesforceConnectedServiceWizard(context, this.VisualStudioWorkspace);

            return Task.FromResult(wizard);
        }
    }
}
