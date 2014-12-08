using Microsoft.VisualStudio.ConnectedServices;
using System;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Salesforce.VisualStudio.Services.ConnectedService
{
    [Export(typeof(IConnectedServiceProvider))]
    [ExportMetadata(Constants.ProviderId, Constants.ProviderIdValue)]
    internal class ConnectedServiceProvider : IConnectedServiceProvider
    {
        private BitmapImage icon;

        public ConnectedServiceProvider()
        {
        }

        public string Category
        {
            get { return Resources.ConnectedServiceProvider_Category; }
        }

        public string CreatedBy
        {
            get { return Resources.ConnectedServiceProvider_CreatedBy; }
        }

        public string Description
        {
            get { return Resources.ConnectedServiceProvider_Description; }
        }

        public ImageSource Icon
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

        public Uri MoreInfoUri
        {
            get { return new Uri(Constants.MoreInfoLink); }
        }

        public string Name
        {
            get { return Resources.ConnectedServiceProvider_Name; }
        }

        public Version Version
        {
            get { return typeof(ConnectedServiceProvider).Assembly.GetName().Version; }
        }

        public Task<object> CreateService(Type serviceType, IServiceProvider serviceProvider)
        {
            object service = null;

            if (serviceType == typeof(IConnectedServiceProviderUI))
            {
                IConnectedServiceProviderHost providerHost =
                    (IConnectedServiceProviderHost)serviceProvider.GetService(typeof(IConnectedServiceProviderHost));
                service = new ConnectedServiceWizardProvider(providerHost);
            }

            return Task.FromResult(service);
        }
    }
}
