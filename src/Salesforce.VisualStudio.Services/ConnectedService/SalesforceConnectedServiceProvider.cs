using Microsoft.VisualStudio.ConnectedServices;
using System;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Salesforce.VisualStudio.Services.ConnectedService
{
    [Export(typeof(ConnectedServiceProvider))]
    [ExportMetadata(Constants.ProviderId, Constants.ProviderIdValue)]
    internal class SalesforceConnectedServiceProvider : ConnectedServiceProvider
    {
        private BitmapImage icon;

        public SalesforceConnectedServiceProvider()
        {
        }

        public override string Category
        {
            get { return Resources.ConnectedServiceProvider_Category; }
        }

        public override string CreatedBy
        {
            get { return Resources.ConnectedServiceProvider_CreatedBy; }
        }

        public override string Description
        {
            get { return Resources.ConnectedServiceProvider_Description; }
        }

        public override ImageSource Icon
        {
            get
            {
                if (this.icon == null)
                {
                    this.icon = new BitmapImage();
                    this.icon.BeginInit();
                    this.icon.UriSource = new Uri("pack://application:,,/" + Assembly.GetAssembly(this.GetType()).ToString() + ";component/ConnectedService/Views/Resources/ProviderIcon.png");
                    this.icon.EndInit();
                }

                return this.icon;
            }
        }

        public override Uri MoreInfoUri
        {
            get { return new Uri(Constants.MoreInfoLink); }
        }

        public override string Name
        {
            get { return Resources.ConnectedServiceProvider_Name; }
        }

        public override Version Version
        {
            get { return typeof(SalesforceConnectedServiceProvider).Assembly.GetName().Version; }
        }

        public override Task<ConnectedServiceConfigurator> CreateConfiguratorAsync(ConnectedServiceProviderHost host)
        {
            ConnectedServiceConfigurator wizard = new SalesforceConnectedServiceWizard(host);

            return Task.FromResult(wizard);
        }
    }
}
